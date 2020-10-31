using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Net.Sockets;
using System;
using SourceExtensions;

namespace Networking.Server
{
    public class Client : ServerSideObject
    {
        //--------------------------------------------------
        #region VARIABLES
        /// public variables declaration



        public TCPCore tcp { get; private set; }
        public UDPCore udp { get; private set; }       // TODO
        
        public int clientId;                // client unique connection identifier, TODO replace it with something not stupid



        #endregion
        //--------------------------------------------------
        #region INTERACTION METHODS
        /// these methods will be executed only here



        public Client()
        {
            // initialize client-sent methods
            RegisterInstructions(this);
            Initialize(this);
        }

        public void Connect(TcpClient tcp, int id)        //  initializes a connection with client 
        {
            // get tcp network stream
            clientId = id;
            this.tcp = new TCPCore(tcp, HandleInstruction, Delete);

            // now we are connected and ready to begin data reading
            Connected = true;
            onStart?.Invoke();
            this.tcp.Open();
        }

        public void Disconnect()        // safely disconnects client 
        {
            if (!Connected) { Logging.LogError("Client " + clientId + ": Disconnecting was turned down: socket is already closed."); return; }
            Connected = false;

            tcp.Send(Instructions.Disconnect);
        }

        public void ForceDisconnect()   // without notifying it about such an intention, forcibly disconnects client 
        {
            // invoke event before disconnecting
            try { onBeforeForceDisconnect?.Invoke(); } finally { }

            // force-disconnect client
            try
            {
                tcp.Shutdown(SocketShutdown.Both);
            }
            catch (ObjectDisposedException) { Logging.LogError(tcp.endPoint + ": Sending is turned down: socket was closed."); }
            catch (SocketException sockExc) { Logging.LogError(tcp.endPoint + ": Socket exception occured: " + sockExc); }
        }

        public bool RegisterInstructions(object objToRegisterInstructionsFor, bool reRegisterIfPresent=false)    // registers instructions for the specified class instance 
        {
            if (!reRegisterIfPresent && registeredInstructionClasses.ContainsKey(objToRegisterInstructionsFor.GetType())) return false;
            
                 
            registeredInstructionClasses[objToRegisterInstructionsFor.GetType()] = objToRegisterInstructionsFor; return true;
        }

        public bool ExecuteOne()    // execute one instruction from the buffered ones 
        {
            Tuple<short, byte[]> instructionData = null;

            try
            {
                lock (instructionsLock) { if (instructionsToExecute.Count == 0) return false; instructionData = instructionsToExecute.Pop(); }

                MethodInfo instruction = instructions[instructionData.Item1];
                instruction.Invoke(registeredInstructionClasses[instruction.DeclaringType], new object[] { instructionData.Item2 });
                return true;
            }
            catch (KeyNotFoundException) { Logging.LogError("Instruction with id " + instructionData?.Item1 + " does not exist or isn't declared properly!"); }
            return false;
        }

        public void ExecuteAll()    // execute all the buffered instructions 
        {

            Tuple<short, byte[]> instructionData;

            lock (instructionsLock)
            {
                while (instructionsToExecute.Count > 0)
                {
                    instructionData = instructionsToExecute.Pop();
                    MethodInfo instruction = instructions[instructionData.Item1];
                    instruction.Invoke(registeredInstructionClasses[instruction.DeclaringType], new object[] { instructionData.Item2 });
                }
            }

        }



        #endregion
        //--------------------------------------------------
        #region EVENTS



        public event Action onStart;
        public event Action onBeforeDisconnect;
        public event Action onBeforeForceDisconnect;

        protected override void Start()
        {
            // notify client about disconnection
            ///tcp.Send(Instructions.HelloWorld, Bytes.ToBytes("Hello darkness my old friend!"));
        } 

        protected override void BeforeDisconnect()
        {
        }

        protected override void BeforeForceDisconnect()
        {
        }

        public override void Delete()
        {
            Logging.LogDeny("Client " + clientId + " on address " + tcp.endPoint + " ended it's session.");

            registeredInstructionClasses.Clear();

            if (Connected) Disconnect();
            tcp.Close();

            foreach (var deleg in onStart.GetInvocationList()) onStart -= (Action)deleg;
            foreach (var deleg in onBeforeDisconnect.GetInvocationList()) onStart -= (Action)deleg;
            foreach (var deleg in onBeforeForceDisconnect.GetInvocationList()) onStart -= (Action)deleg;

            base.Delete();

            Server.server.RemoveClient(this);
        }



        #endregion
        //--------------------------------------------------
        #region CLIENT-SENT INSTRUCTIONS
        /// instructions coming from client



        [Instruction(Instructions.HelloWorld)]
        private void instHelloWorld(byte[] data)       // testing function 
        {
            Logging.Log("Not this!");
            if(data != null && data.Length > 0) Logging.LogWarning("Hello from the client-side! data: " + Bytes.ToString(data));
            else Logging.LogWarning("Hello from the client-side! No data was given...");
        }


        [Instruction(Instructions.Connect)]
        private void instConnect(byte[] data)        // connect 
        {
            Logging.LogWarning("Hello from the client-side! " + Bytes.ToString(data));
        }


        [Instruction(Instructions.Disconnect)]
        private void instDisconnect(byte[] data)        // disconnect 
        {
            try { onBeforeDisconnect?.Invoke(); } finally { }

            tcp.Shutdown(SocketShutdown.Both);
            Connected = false;
        }


        #endregion
        //--------------------------------------------------
        #region INTERNAL VARIABLES
        /// inaccessible variables, that are responsible for internal client data storing



        // instructions
        private static Dictionary<short, MethodInfo> instructions;
        private Dictionary<Type, object> registeredInstructionClasses = new Dictionary<Type, object>();        // dictionary of the registered classes which contain instruction


        // vars for supportiong instructions buffering
        private bool bufferInstructions = false;
        private Stack<Tuple<short, byte[]>> instructionsToExecute = new Stack<Tuple<short, byte[]>>();
        private object instructionsLock = new object();

        private bool Connected = false;



        #endregion
        //--------------------------------------------------
        #region INTERNAL METHODS
        /// inaccessible methods, that are responsible for internal client functioning



        public static void RegisterAllInstructions()    // makes all instructions ready for usage 
        {
            if (instructions == null)
                instructions = (from type in Assembly.GetExecutingAssembly().GetTypes()
                                from method in type.GetMethods(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance)
                                where method.GetCustomAttribute(typeof(Client.InstructionAttribute)) != null
                                select method).ToDictionary(x => (x.GetCustomAttribute(typeof(Client.InstructionAttribute)) as Client.InstructionAttribute).id);
        }

        private void HandleInstruction(short instructionId, byte[] data)    // specifies the metods of handling received data 
        {
            try
            {
                lock (instructionsLock)
                {
                    if (bufferInstructions)
                    {
                        instructionsToExecute.Push(new Tuple<short, byte[]>(instructionId, data));
                    }
                    else
                    {
                        MethodInfo instruction = instructions[instructionId];
                        instruction?.Invoke(registeredInstructionClasses[instruction.DeclaringType], new object[] { data });
                    }
                }
            }
            catch (KeyNotFoundException) { Logging.LogError("Instruction with id " + instructionId + " does not exist or isn't declared properly!"); }
        }



        #endregion
        //--------------------------------------------------
        #region CLASSES
        /// classes, used by client internally



        [AttributeUsage(AttributeTargets.Method)]
        public class InstructionAttribute : Attribute
        {
            public short id;

            public InstructionAttribute(Instructions instructionId)
            {
                id = (short)instructionId;
            }
        }



        #endregion
        //--------------------------------------------------
    }


}
