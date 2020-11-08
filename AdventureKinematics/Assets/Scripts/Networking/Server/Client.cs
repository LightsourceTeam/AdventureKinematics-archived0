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
        public UDPCore udp { get; private set; }
        public InstructionHandler instructionHandler { get; private set; }
        public object executionLock { get; private set; } = new object();


        public int clientId;                // client unique connection identifier, TODO replace it with something not stupid
   


        #endregion
        //--------------------------------------------------
        #region INTERACTION
        /// these methods will be executed only here



        public Client()
        {
            // initialize client-sent methods
            Initialize(this);
        }

        public void Connect(TcpClient tcp, int id)        //  initializes a connection with client 
        {
            Logging.LogAccept($"Client {id} on address {tcp.Client.RemoteEndPoint} joined the server.");

            // get tcp and instruction handler ready
            clientId = id;
            instructionHandler = new InstructionHandler(executionLock);
            this.tcp = new TCPCore(tcp, instructionHandler.HandleInstruction);

            instructionHandler.RegisterInstructions(this);

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

                udp = new UDPCore(Server.server.udpListener, instructionHandler.HandleInstruction);
            }
            tcp.Send(Instructions.RequestUdp, new byte[] { 0 });
        }

        public void NotifyShutdown(byte[] reason = null)
        {
            Logging.Log("Hello");
            lock (executionLock) tcp?.Send(Instructions.NotifyShutdown, reason);
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

        public override void Delete()
        {
            lock (executionLock)
            {
                if (Deleted) return;

                try
                {
                    Logging.LogDeny($"Client {clientId} on address {tcp.endPoint} left the server.");

                    if (Connected) try { onBeforeForceDisconnect?.Invoke(); } finally { }
                    tcp.Send(Instructions.HelloWorld);

                    Connected = false;
                    UdpConnected = false;
                    Deleted = true;

                    base.Delete();
                    client = null;

                    tcp.Close();
                    instructionHandler.Deactivate();

                    tcp = null;
                    udp = null;
                    instructionHandler = null;
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

        protected override void AfterConnect() { InvolveUdp(); }

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



        private bool Connected = false;
        private bool UdpConnected = false;
        private bool Deleted = false;



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



        #endregion
        //--------------------------------------------------
    }


}
