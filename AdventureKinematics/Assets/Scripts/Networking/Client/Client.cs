using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Net;
using System.Net.Sockets;
using System;
using System.Threading;
using System.Threading.Tasks;
using SourceExtensions;
using static SourceExtensions.UnlockObject;

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
        #region INTERACTION
        /// these methods will be executed only here



        public void Connect()    //  initializes a connection with client 
        {
            Connected = true;

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
            lock (executionLock)
            {
                // check if client is not already closed
                if (!Connected || Deleted) { Logging.LogError("Disconnecting was turned down: socket is already closed."); return; }
                Connected = false;

                // invoke event before disconnecting
                try { CustomEventSystem.NotifyAboutDisconnect(); } finally { }

                // send instruction to server, and wait 
                try
                {
                    tcp.Send(Instructions.Disconnect, new byte[] { 0 });
                }
                catch (ObjectDisposedException) { Logging.LogError($"Disconnecting was turned down: failed to send data - socket is closed."); }
                catch (SocketException sockExc) { Logging.LogCritical($"Socket exception occured: {sockExc}"); }
                catch (Exception exc) { Logging.LogError($"Unhandled exception occured while disconnecting: {exc}"); }
            }
        }

        public bool RegisterInstructions(object objToRegisterInstructionsFor, bool reRegisterIfPresent = false)    // registers instructions for the specified class instance 
        {
            lock (executionLock)
            {
                if (!reRegisterIfPresent && registeredInstructionClasses.ContainsKey(objToRegisterInstructionsFor.GetType())) return false;

                registeredInstructionClasses[objToRegisterInstructionsFor.GetType()] = objToRegisterInstructionsFor; return true;
            }
        }

        public bool Execute(Tuple<short, byte[]> instruction)
        {
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

        public void ExecuteOneBuffered()    // execute one instruction from the buffered ones 
        {
            Tuple<short, byte[]> instruction = null;

            lock (executionLock)
            {
                if (instructionsToExecute.Count == 0) return;
                instruction = instructionsToExecute.Pop();
                Execute(instruction);
            }
        }

        public void ExecuteAllBuffered()    // execute all the buffered instructions 
        {
            lock (executionLock)
            {
                while (instructionsToExecute.Count > 0) ExecuteOneBuffered();
            }
        }

        public Tuple<short, byte[]> WaitForInstruction()
        {
            lock (executionLock)
            {
                if (!bufferInstructions) return null;
                if (instructionsToExecute.Count == 0) Monitor.Wait(executionLock);
                return instructionsToExecute.Pop();
            }
        }

        public void SwitchToBufferingMode() // switches execution method to buffering, which means all the received instructions will be buffered instead of being executed in place 
        {
            lock (executionLock) { if (!bufferInstructions) bufferInstructions = true; }
        }

        public void SwitchToEventMode(bool executeBuffered = true) // switches execution method to event-driven, which means all the received instructions will be executed in place 
        {
            lock (executionLock)
            {
                if (!bufferInstructions) return;

                bufferInstructions = false;
                if (executeBuffered) ExecuteAllBuffered();
            }
        }

        public void Delete()
        {
            lock (executionLock)
            {
                if (Deleted) return;

                Logging.LogDeny("Client " + clientId + " on address " + tcp.endPoint + " left the server.");

                if (Connected) try { CustomEventSystem.NotifyAboutForceDisconnect(); } finally { }


                Connected = false;
                availableUdp = false;
                client = null;
                Deleted = true;

                tcp.Close();
                udp?.Close();

                registeredInstructionClasses.Clear();

                lock (deleteSignalizer) Monitor.Pulse(deleteSignalizer);
            }
        }

        public void SafeSessionEnd()
        {
            lock (executionLock)
            {
                // signalize that client goes to sleep
                Disconnect();

                // and keep executing instructions from server, until disconnection is confirmed
                Tuple<short, byte[]> inst = null;
                while (inst?.Item1 != (short)Instructions.Disconnect)
                {
                    inst = WaitForInstruction();
                    Execute(inst);
                }

                if (!Deleted)
                {
                    lock (deleteSignalizer)
                    {
                        using (unlock(executionLock))
                        {
                            Monitor.Wait(deleteSignalizer);
                        }
                    }
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
            if (availableUdp) udp.Send(Instructions.HelloWorld, Bytes.ToBytes($"{i++}"));
            ExecuteAllBuffered();
        }
        int i = 0;

        protected override void BeforeDisconnect() { }

        protected override void AfterUdpInvolved() { }


        protected void OnApplicationQuit()
        {
            SafeSessionEnd();
        }



        #endregion
        //--------------------------------------------------
        #region INSTRUCTIONS
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
            switch (data[0])
            {
                case 0:
                    if (Connected) CustomEventSystem.NotifyAboutDisconnect();
                    Connected = false;

                    tcp.Send(Instructions.Disconnect, new byte[] { 1 });
                    break;
                case 1:
                    tcp.Send(Instructions.Disconnect, new byte[] { 1 });
                    break;
            }
        }


        [Instruction(Instructions.RequestUdp)]
        private void instRequestUdp(byte[] data)
        {
            switch (data[0])
            {
                case 0:
                    Logging.LogInfo($"A server request to involve udp was initialed...");

                    udp = new UDPCore(HandleInstruction);
                    udp.Open(udpEndPoint);
                    tcp.Send(Instructions.RequestUdp, Bytes.ToBytes(((IPEndPoint)udp.client.Client.LocalEndPoint).Port));

                    break;
                case 1:
                    availableUdp = true;
                    Logging.LogAccept($"Udp-involoving request has been approved. Starting udp communication with {udp.endPoint}.");

                    CustomEventSystem.NotifyAboutInvolvingUdp();

                    break;
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

        private bool Connected = false;
        private bool availableUdp = false;
        private bool Deleted = false;
        private object deleteSignalizer = new object();


        private void HandleInstruction(short instructionId, byte[] data)    // specifies the metods of handling received data 
        {
            lock (executionLock)
            {
                if (Deleted) return;

                try
                {
                    if (bufferInstructions)
                    {
                        instructionsToExecute.Push(new Tuple<short, byte[]>(instructionId, data));
                        Monitor.Pulse(executionLock);
                    }
                    else
                    {
                        MethodInfo instruction = instructions[instructionId];
                        instruction?.Invoke(registeredInstructionClasses[instruction.DeclaringType], new object[] { data });
                    }
                }
                catch (KeyNotFoundException) { Logging.LogError($"Client {clientId}: instruction with id {instructionId} does not exist or isn't declared properly!"); }
            }
        }



        #endregion
        //--------------------------------------------------
        #region UTILITIES
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

        public object executionLock { get; private set; } = new object();



        #endregion
        //--------------------------------------------------
    }



}
