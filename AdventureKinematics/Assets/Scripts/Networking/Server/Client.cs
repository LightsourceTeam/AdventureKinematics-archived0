using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Net.Sockets;
using System.Net;
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
            Logging.LogAccept($"Client {id} on address {tcp.Client.RemoteEndPoint} joined the server.");

            // get tcp network stream
            clientId = id;
            this.tcp = new TCPCore(tcp, HandleInstruction, Delete);

            // now we are connected and ready to begin data reading
            Connected = true;
            onBeforeConnect?.Invoke();
            this.tcp.Open();
        }

        public void InvolveUdp()
        {
            Logging.LogInfo($"Client {clientId}: a request to involve udp has been initialed.");

            udp = new UDPCore(Server.server.udpListener, HandleInstruction);
            tcp.Send(Instructions.RequestUdp, Bytes.ToBytes(false));
        }

        public void Disconnect()        // safely disconnects client 
        {
            if (!Connected) { Logging.LogError($"Client {clientId}: Disconnecting was turned down: socket is already closed."); return; }
            Connected = false;

            tcp.Send(Instructions.Disconnect);
        }

        public void ForceDisconnect(string reason = null)   // without notifying it about such an intention, forcibly disconnects client 
        {
            lock (instructionsLock)
            {
                // invoke event before disconnecting
                try { onBeforeForceDisconnect?.Invoke(); }
                finally
                {

                    try
                    {
                        if (reason != null && reason.Length > 0) tcp.Send(Instructions.ForceDisconnect, Bytes.ToBytes(reason));
                        else tcp.Send(Instructions.ForceDisconnect);
                    }
                    finally
                    {
                        // force-disconnect client
                        try { tcp.Shutdown(SocketShutdown.Both); }
                        catch (ObjectDisposedException) { Logging.LogError($"Client {clientId}: Force disconnecting is disallowed: socket was closed."); }
                        catch (SocketException sockExc) { Logging.LogError($"Client {clientId}: Socket exception occured: " + sockExc); }
                    }
                }
            }
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
            catch (KeyNotFoundException) { Logging.LogError($"Client {clientId}: Instruction with id {instructionData?.Item1} does not exist or isn't declared properly!"); }
            catch (Exception exc) { Logging.LogError($"Client {clientId}: Unhandled Exception occured while executing an instruction with id {instructionData?.Item1}: {exc}"); }
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



        public event Action onBeforeConnect;
        public event Action onBeforeDisconnect;
        public event Action onBeforeForceDisconnect;
        public event Action onAfterUdpInvolved;

        protected override void BeforeConenct()
        {
            InvolveUdp();
        }

        protected override void BeforeDisconnect()
        {
        }

        protected override void BeforeForceDisconnect()
        {
        }

        protected override void AfterUdpInvolved()
        {
            udp.Send(Instructions.HelloWorld, Bytes.ToBytes("Hello from server!"));
        }

        public override void Delete()
        {
            Logging.LogDeny($"Client {clientId} on address {tcp.endPoint} ended it's session.");

            registeredInstructionClasses.Clear();

            if (Connected) Disconnect();
            tcp.Close();

            foreach (var deleg in onBeforeConnect.GetInvocationList()) onBeforeConnect -= (Action)deleg;
            foreach (var deleg in onBeforeDisconnect.GetInvocationList()) onBeforeConnect -= (Action)deleg;
            foreach (var deleg in onBeforeForceDisconnect.GetInvocationList()) onBeforeConnect -= (Action)deleg;

            base.Delete();

            Server.server.RemoveClient(this);
        }



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
            try { onBeforeDisconnect?.Invoke(); } finally { }

            tcp.Shutdown(SocketShutdown.Both);
            Connected = false;
        }


        [Instruction(Instructions.RequestUdp)]
        private void instRequestUdp(byte[] data)
        {
            if (udp != null)
            {
                IPEndPoint endp = new IPEndPoint(((IPEndPoint)tcp.client.Client.RemoteEndPoint).Address, Bytes.ToInt(data));
                Server.server.RegisterUdp(endp, this);
                udp.Open(endp);
                availableUdp = true;

                tcp.Send(Instructions.RequestUdp, Bytes.ToBytes(true));

                onAfterUdpInvolved?.Invoke();
            }
            else ForceDisconnect();
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
        private object instructionsLock = new object();

        private bool Connected = false;
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
