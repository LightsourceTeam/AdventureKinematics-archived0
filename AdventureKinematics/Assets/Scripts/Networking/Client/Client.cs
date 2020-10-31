using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Net;
using System.Net.Sockets;
using System;
using System.Threading;
using SourceExtensions;
using System.IO;

namespace Networking.Client
{
    public class Client : CustomMonoBehaviour
    {
        //--------------------------------------------------
        #region VARIABLES
        /// variables declaration



        public static Client client = null;     // the main instance of the client


        // connection-necessary params
        public TCPCore tcp { get; private set; }
        public UDPCore udp { get; private set; }       // TODO


        public int clientId;                    // client unique connection identifier, TODO replace it with something not stupid



        #endregion
        //--------------------------------------------------
        #region INTERACTION METHODS
        /// these methods will be executed only here



        public void Connect()    //  initializes a connection with client 
        {
            // set static instance of the client
            if (client == null) client = this;
            else if (client != this) Logging.LogError(this + ": Failed to set active instance - it already is set to " + client);
            else Logging.LogWarning(this + ": No need to setup this as active object - it already is");

            Logging.LogInfo("Building connection to server...");

            // get tcp network stream
            tcp = new TCPCore(new IPEndPoint(IPAddress.Parse("127.0.0.1"), 23852), HandleInstruction, Delete);


            Logging.LogAccept("Successfully connected. Starting data transfer protocol...");


            // now we are connected and ready to begin data reading
            Connected = true;
            tcp.Open();
        }

        public void Disconnect()              // safely disconnects client 
        {
            // check if client is not already closed
            if (!Connected) { Logging.LogError("Disconnecting was turned down: socket is already closed."); return; }
            Connected = false;

            // invoke event before disconnecting
            try { CustomEventSystem.InvokeBeforeDisconnect(); } finally { }

            // send instruction to server, and shutdown sending
            try
            {
                tcp.Send(Instructions.Disconnect);
                tcp.Shutdown(SocketShutdown.Send);
            }
            catch (ObjectDisposedException) { Logging.LogError("Disconnecting was turned down: socket is already closed."); }
            catch (SocketException sockExc) { Logging.LogCritical("Socket exception occured: " + sockExc); }

        }

        public bool RegisterInstructions(object objToRegisterInstructionsFor, bool reRegisterIfPresent = false)    // registers instructions for the specified class instance 
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



        protected void Awake()
        {
            // initialize server-sent methods
            RegisterAllInstructions();
            RegisterInstructions(this);

            //Bytes.TestFunction();
        }

        protected void Start()
        {
            Connect();

            // Send(Instructions.HelloWorld, Bytes.ToBytes("Hello World"));
        }

        protected override void NetworkUpdate()
        {
            ExecuteOne();
        }

        protected override void BeforeDisconnect()
        {
        }

        protected void OnApplicationQuit()
        {
            Disconnect();
        }

        private void Delete()
        {
            Logging.LogDeny("Client " + clientId + " on address " + tcp.endPoint + " ended it's session.");

            if (Connected) Disconnect();
            tcp.Close();
            client = null;
            registeredInstructionClasses.Clear();

        }



        #endregion
        //--------------------------------------------------
        #region SERVER-SENT INSTRUCTIONS
        /// instructions coming from server



        [Instruction(Instructions.HelloWorld)]
        private void instHelloWorld(byte[] data)       // testing function 
        {
            if (data != null && data.Length > 0) Logging.LogWarning("Hello from the server-side! data: " + Bytes.ToString(data));
            else Logging.LogWarning("Hello from the server-side! No data was given...");
        }


        [Instruction(Instructions.Connect)]
        private void instConnect(byte[] data)        // connect 
        {

        }


        [Instruction(Instructions.Disconnect)]
        private void instDisconnect(byte[] data)        // disconnect 
        {
            CustomEventSystem.InvokeBeforeDisconnect();

            tcp.Send(Instructions.Disconnect);
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


        // connection address
        private IPAddress ip = IPAddress.Parse("127.0.0.1");
        private int port = 23852;


        // vars for supportiong instructions buffering
        private bool bufferInstructions = true;
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
