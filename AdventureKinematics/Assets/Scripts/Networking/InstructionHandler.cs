using System.Collections.Generic;
using System.Threading;
using System.Reflection;
using System.Linq;
using System;
using SourceExtensions;

namespace Networking
{


    public class InstructionHandler
    {
        //--------------------------------------------------
        #region VARIABLES


        public static IReadOnlyDictionary<short, MethodInfo> instructions { get; private set; }  // contains all the methods marked with Instruction attribute
        private Dictionary<Type, object> registeredInstructionClasses = new Dictionary<Type, object>();        // dictionary of the registered classes which contain instruction


        private Stack<Tuple<short, byte[]>> bufferedInstructions = new Stack<Tuple<short, byte[]>>();
        public int instructionsCount { get { return bufferedInstructions.Count; } }

        public object executionLock { get; private set; } = new object();

        public bool bufferInstructions { get; private set; } = false;
        public bool alive { get; private set; } = true;



        #endregion
        //--------------------------------------------------
        #region REGISTRATION



        public InstructionHandler(object executionLock)
        {
            if (executionLock != null) this.executionLock = executionLock;
            else throw new ArgumentException("executionLock is null!");
        }

        public static void RegisterAllInstructions(Type instructionType)    // makes all instructions ready for usage 
        {
            if (instructions == null)
                try
                {
                    instructions =
                        (from type in Assembly.GetExecutingAssembly().GetTypes()
                         from method in type.GetMethods(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance)
                         where method.GetCustomAttribute(instructionType) != null
                         select method).ToDictionary(x => (short)((dynamic)x.GetCustomAttribute(instructionType)).id);
                }
                catch (Exception exc)
                {
                    Logging.LogCritical($"Instructions haven't been set: {exc}");
                }
            else return;
        }

        public bool RegisterInstructions(object objToRegisterInstructionsFor, bool reRegisterIfPresent = false)    // registers instructions for the specified class instance 
        {
            lock (executionLock)
            {
                if (!alive) return false;

                if (!reRegisterIfPresent && registeredInstructionClasses.ContainsKey(objToRegisterInstructionsFor.GetType())) return false;

                registeredInstructionClasses[objToRegisterInstructionsFor.GetType()] = objToRegisterInstructionsFor; return true;
            }
        }



        #endregion
        //--------------------------------------------------
        #region INTRUCTIONS



        public void HandleInstruction(short instructionId, byte[] data)    // specifies the metods of handling received data 
        {
            lock (executionLock)
            {
                if (!alive) return;

                try
                {
                    if (bufferInstructions)
                    {
                        bufferedInstructions.Push(new Tuple<short, byte[]>(instructionId, data));
                        Monitor.Pulse(executionLock);
                    }
                    else
                    {
                        MethodInfo instruction = instructions[instructionId];
                        instruction?.Invoke(registeredInstructionClasses[instruction.DeclaringType], new object[] { data });
                    }
                }
                catch (KeyNotFoundException) { Logging.LogError($"Instruction with id {instructionId} does not exist or isn't declared properly!"); }
            }
        }

        public bool Execute(Tuple<short, byte[]> instruction)
        {
            lock (executionLock)
            {
                if (!alive) return false;

                try
                {
                    MethodInfo instructionMethod = instructions[instruction.Item1];
                    instructionMethod.Invoke(registeredInstructionClasses[instructionMethod.DeclaringType], new object[] { instruction.Item2 });
                    return true;
                }
                catch (KeyNotFoundException) { Logging.LogError($"Instruction with id {instruction?.Item1} does not exist or isn't declared properly!"); }
                catch (Exception exc) { Logging.LogError($"Unhandled Exception occured while executing an instruction with id {instruction?.Item1}: {exc}"); }
                return false;
            }
        }

        public void ExecuteOneBuffered()    // execute one instruction from the buffered ones 
        {
            lock (executionLock)
            {
                Tuple<short, byte[]> instruction = null;

                if (bufferedInstructions?.Count == 0) return;
                instruction = bufferedInstructions.Pop();
                Execute(instruction);
            }
        }

        public void ExecuteAllBuffered()    // execute all the buffered instructions 
        {
            lock (executionLock)
            {
                while (bufferedInstructions?.Count > 0) ExecuteOneBuffered();
            }
        }

        public Tuple<short, byte[]> WaitForInstruction()
        {
            lock (executionLock)
            {
                if (!alive || !bufferInstructions) return null;
                if (bufferedInstructions?.Count == 0) Monitor.Wait(executionLock);
                return bufferedInstructions?.Pop();
            }
        }


        #endregion
        //--------------------------------------------------
        #region INTERACTION



        public void SwitchToBufferingMode() // switches execution method to buffering, which means all the received instructions will be buffered instead of being executed in place 
        {
            lock (executionLock)
            {
                if (!alive) return;

                if (!bufferInstructions) bufferInstructions = true; 
            }
        }

        public void SwitchToEventMode(bool executeBuffered = true) // switches execution method to event-driven, which means all the received instructions will be executed in place 
        {
            lock (executionLock)
            {
                if (!alive || !bufferInstructions) return;

                bufferInstructions = false;
                if (executeBuffered) ExecuteAllBuffered();

                bufferedInstructions.Clear();
                bufferedInstructions = null;
            }
        }
    
        public void Deactivate()
        {
            lock (executionLock)
            {
                if (!alive) return;

                registeredInstructionClasses.Clear();
                registeredInstructionClasses = null;
                alive = false;
            }
        }



        #endregion
        //--------------------------------------------------
    }
}
