using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Net;
using System.Net.Sockets;
using System;
using System.Threading.Tasks;
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
        public static IPEndPoint tcpEndPoint = new IPEndPoint(IPAddress.Parse("127.0.0.1"), 23852);
        public static IPEndPoint udpEndPoint = new IPEndPoint(IPAddress.Parse("127.0.0.1"), 23853);


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
            tcp = new TCPCore(tcpEndPoint, HandleInstruction, Delete);


            Logging.LogAccept("Successfully connected. Starting data transfer protocol...");


            // now we are connected and ready to begin data reading
            CustomEventSystem.NotifyAboutConnect();
            tcp.Open();
        }

        public void Disconnect()              // safely disconnects client 
        {
            // check if client is not already closed
            if (!CustomEventSystem.ConnectedToServer) { Logging.LogError("Disconnecting was turned down: socket is already closed."); return; }
            CustomEventSystem.ConnectedToServer = false;

            // invoke event before disconnecting
            try { CustomEventSystem.NotifyAboutDisconnect(); } finally { }

            // send instruction to server, and shutdown sending
            try
            {
                tcp.Send(Instructions.Disconnect);
                tcp.Shutdown(SocketShutdown.Send);
                udp?.Shutdown(SocketShutdown.Send);
            }
            catch (ObjectDisposedException) { Logging.LogError("Disconnecting was turned down: socket is already closed."); }
            catch (SocketException sockExc) { Logging.LogCritical($"Socket exception occured: {sockExc}"); }
            catch (Exception exc) { Logging.LogError($"Unhandled exception occured while disconnecting: {exc}"); }
        }

        public bool RegisterInstructions(object objToRegisterInstructionsFor, bool reRegisterIfPresent = false)    // registers instructions for the specified class instance 
        {
            if (!reRegisterIfPresent && client.registeredInstructionClasses.ContainsKey(objToRegisterInstructionsFor.GetType())) return false;


            client.registeredInstructionClasses[objToRegisterInstructionsFor.GetType()] = objToRegisterInstructionsFor; return true;
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
            catch (KeyNotFoundException) { Logging.LogError($"Instruction with id {instructionData?.Item1} does not exist or isn't declared properly!"); }
            catch (Exception exc) { Logging.LogError($"Unhandled Exception occured while executing an instruction with id {instructionData?.Item1}: {exc}"); }
            return false;
        }

        public void ExecuteAll()    // execute all the buffered instructions 
        {
            lock (instructionsLock)
            {
                while (instructionsToExecute.Count > 0)
                {
                    ExecuteOne();
                }
            }

        }

        public void SwitchToBufferingMode() // switches execution method to buffering, which means all the received instructions will be buffered instead of being executed in place 
        { 
            lock (instructionsLock) { if (!bufferInstructions) bufferInstructions = true; }
        }

        public void SwichToEventMode(bool executeBuffered = true) // switches execution method to event-driven, which means all the received instructions will be executed in place 
        {
            lock (instructionsLock)
            {
                if (!bufferInstructions) return;

                bufferInstructions = false;
                if (executeBuffered) ExecuteAll();
            }
        }



        #endregion
        //--------------------------------------------------
        #region EVENTS



        protected void Awake()
        {
            // initialize server-sent methods
            RegisterAllInstructions();

            //Bytes.TestFunction();
        }

        protected void Start()
        {
            Connect();
        }

        protected override void BeforeConnect()
        {
            RegisterInstructions(this);
        }

        protected override void NetworkUpdate()
        {
            if(availableUdp) udp.Send(Instructions.HelloWorld, Bytes.ToBytes($"{tcp.client.Client.LocalEndPoint} {DateTime.Now} {i++}"));
            ExecuteAll();
        }

        protected override void BeforeDisconnect()
        {
        }

        protected override void AfterUdpInvolved() { }

        protected void OnApplicationQuit()
        {
            Disconnect();
        }

        private void Delete()
        {
            lock (instructionsLock)
            {
                Logging.LogDeny("Client " + clientId + " on address " + tcp.endPoint + " ended it's session.");

                
                tcp.Close();
                udp.Close();
                client = null;

                registeredInstructionClasses.Clear();
            }
        }



        #endregion
        //--------------------------------------------------
        #region SERVER-SENT INSTRUCTIONS
        /// instructions coming from server



        [Instruction(Instructions.HelloWorld)]
        private void instHelloWorld(byte[] data)       // testing function 
        {
            if (data != null && data.Length > 0) Logging.LogInfo($"Hello from the server-side! data: {Bytes.ToString(data)}");
            else Logging.LogInfo("Hello from the server-side! No data was given...");
        }


        [Instruction(Instructions.Connect)]
        private void instConnect(byte[] data)        // connect 
        {

        }


        [Instruction(Instructions.Disconnect)]
        private void instDisconnect(byte[] data)        // disconnect 
        {
            CustomEventSystem.NotifyAboutDisconnect();

            tcp.Send(Instructions.Disconnect);
            tcp.Shutdown(SocketShutdown.Both);
            udp?.Shutdown(SocketShutdown.Both);

            availableUdp = false;
            CustomEventSystem.ConnectedToServer = false;
        }


        [Instruction(Instructions.ForceDisconnect)]
        private void instForceDisconnect(byte[] data)        // disconnect 
        {
            tcp.Shutdown(SocketShutdown.Both);
            CustomEventSystem.ConnectedToServer = false;

            if(data != null) Logging.LogError(Bytes.ToString(data));
        }


        [Instruction(Instructions.RequestUdp)]
        private void instRequestUdp(byte[] data)
        {
            if (!Bytes.ToBool(data))
            {
                Logging.LogInfo($"A server request to involve udp was initialed...");

                udp = new UDPCore(HandleInstruction);
                udp.Open(udpEndPoint);
                tcp.Send(Instructions.RequestUdp, Bytes.ToBytes(((IPEndPoint)udp.client.Client.LocalEndPoint).Port));
            }
            else
            {
                availableUdp = true;
                Logging.LogAccept($"Udp-involoving request has been approved. Starting udp communication with {udp.endPoint}.");

                CustomEventSystem.NotifyAboutInvolvingUdp();
            }
        }



        #endregion
        //--------------------------------------------------
        #region INTERNAL
        /// inaccessible fields, that are responsible for internal client functioning



        // instructions
        private Dictionary<Type, object> registeredInstructionClasses = new Dictionary<Type, object>();        // dictionary of the registered classes which contain instruction


        // vars for supportiong instructions buffering
        private bool bufferInstructions = true;
        private Stack<Tuple<short, byte[]>> instructionsToExecute = new Stack<Tuple<short, byte[]>>();
        private object instructionsLock = new object();


        private bool availableUdp = false;



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
