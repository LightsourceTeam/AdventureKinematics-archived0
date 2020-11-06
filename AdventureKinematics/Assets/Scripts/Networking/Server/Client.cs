using System.Collections.Generic;
using System.Threading;
using System.Reflection;
using System.Net.Sockets;
using System.Net;
using System;
using SourceExtensions;
using static SourceExtensions.UnlockObject;

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
        #region INTERACTION
        /// these methods will be executed only here



        public Client()
        {
            // initialize client-sent methods
            Initialize(this);
            RegisterInstructions(this);
        }

        public void Connect(TcpClient tcp, int id)        //  initializes a connection with client 
        {
            Logging.LogAccept($"Client {id} on address {tcp.Client.RemoteEndPoint} joined the server.");

            // get tcp network stream
            clientId = id;
            this.tcp = new TCPCore(tcp, HandleInstruction);

            // now we are connected and ready to begin data reading
            Connected = true;
            onBeforeConnect?.Invoke();
            this.tcp.Open();
            onAfterConnect?.Invoke();
        }

        public void InvolveUdp()
        {
            lock (executionLock)
            {
                Logging.LogInfo($"Client {clientId}: a request to involve udp has been initialed.");

                udp = new UDPCore(Server.server.udpListener, HandleInstruction);
            }
            tcp.Send(Instructions.RequestUdp, new byte[] { 0 });
        }

        public void Disconnect()        // safely disconnects client 
        {
            lock (executionLock)
            {
                // check if client is not already closed
                if (!Connected || Deleted) { Logging.LogError($"Client {clientId}: Disconnecting was turned down: socket is already closed."); return; }
                Connected = false;

                // invoke event before disconnecting
                try { onBeforeDisconnect?.Invoke(); } finally { }

                // send instruction to server, and shutdown sending
                try { tcp.Send(Instructions.Disconnect, new byte[] { 0 }); }
                catch (ObjectDisposedException) { Logging.LogError($"Client {clientId}: Disconnecting was turned down: failed to send data - socket is closed."); }
                catch (SocketException sockExc) { Logging.LogCritical($"Client {clientId}:Socket exception occured: {sockExc}"); }
                catch (Exception exc) { Logging.LogError($"Client {clientId}: Unhandled exception occured while disconnecting: {exc}"); }
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

        public override void Delete()
        {
            lock (executionLock)
            {
                if (Deleted) return;

                try
                {
                    Logging.LogDeny($"Client {clientId} on address {tcp.endPoint} left the server.");

                    if (Connected) try { onBeforeForceDisconnect?.Invoke(); } finally { }

                    Connected = false;
                    UdpConnected = false;
                    Deleted = true;

                    base.Delete();
                    client = null;

                    tcp.Close();
                    registeredInstructionClasses.Clear();
                }
                catch (Exception Exc) { Logging.LogCritical($"Client {clientId}: while deleting, an exception occured: " + Exc); }
            }

            Server.server.RemoveClient(this);
            Server.server.UnregisterUdp(udp?.endPoint);
        }



        #endregion
        //--------------------------------------------------
        #region EVENTS



        public event Action onBeforeConnect;
        public event Action onAfterConnect;
        public event Action onBeforeDisconnect;
        public event Action onBeforeForceDisconnect;
        public event Action onAfterUdpInvolved;

        protected override void BeforeConenct() { }

        protected override void AfterConnect() { }

        protected override void BeforeDisconnect() { }

        protected override void BeforeForceDisconnect() { }

        protected override void AfterUdpInvolved() { }



        #endregion
        //--------------------------------------------------
        #region INSTRUCTIONS
        /// instructions coming from client



        [Instruction(Instructions.HelloWorld)]
        private void instHelloWorld(byte[] data)       // testing function 
        {
            if (data != null && data.Length > 0) Logging.LogInfo($"Hello from the server-side! data: {Bytes.ToString(data)}");
            else Logging.LogInfo("Hello from the server-side! No data was given...");
        }


        [Instruction(Instructions.Connect)]
        private void instConnect(byte[] data)        // connect 
        {
            Logging.LogWarning("Hello from the client-side! " + Bytes.ToString(data));
        }


        [Instruction(Instructions.Disconnect)]
        private void instDisconnect(byte[] data)        // disconnect 
        {
            switch (data[0])
            {
                case 0:


                    if (Connected)
                    {
                        try { onBeforeDisconnect?.Invoke(); } finally { }
                        Connected = false;
                    }

                    tcp.Send(Instructions.Disconnect, new byte[] { 1, });
                    break;
                case 1:
                    Delete();

                    break;
            }
                
        }


        [Instruction(Instructions.RequestUdp)]
        private void instRequestUdp(byte[] data)
        {
            if (udp != null)
            {
                IPEndPoint endp = new IPEndPoint(((IPEndPoint)tcp.client.Client.RemoteEndPoint).Address, Bytes.ToInt(data));
                Server.server.RegisterUdp(endp, this);
                udp.Open(endp);
                UdpConnected = true;

                tcp.Send(Instructions.RequestUdp, new byte[] { 1 });

                onAfterUdpInvolved?.Invoke();
            }
            else Delete();
        }



        #endregion
        //--------------------------------------------------
        #region INTERNAL 
        /// inaccessible fields, that are responsible for internal client functioning



        // instructions
        private Dictionary<Type, object> registeredInstructionClasses = new Dictionary<Type, object>();        // dictionary of the registered classes which contain instruction


        // vars for supportiong instructions buffering
        private bool bufferInstructions = false;
        private Stack<Tuple<short, byte[]>> instructionsToExecute = new Stack<Tuple<short, byte[]>>();

        private bool Connected = false;
        private bool UdpConnected = false;
        private bool Deleted = false;

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
