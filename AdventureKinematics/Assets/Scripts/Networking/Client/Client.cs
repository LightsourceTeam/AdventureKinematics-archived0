﻿using System.Collections.Generic;
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




        public static Client current = null;     // the main instance of the client
        public static IPEndPoint tcpEndPoint = new IPEndPoint(IPAddress.Parse("127.0.0.1"), 23852);
        public static IPEndPoint udpEndPoint = new IPEndPoint(IPAddress.Parse("127.0.0.1"), 23853);


        // connection-necessary params
        public TCPCore tcp { get; private set; }
        public UDPCore udp { get; private set; } 
        public InstructionHandler instructionHandler { get; private set; }
        public object executionLock { get; private set; } = new object();


        public int clientId;                    // client unique connection identifier, TODO replace it with something not stupid



        #endregion
        //--------------------------------------------------
        #region INTERACTION
        /// these methods will be executed only here



        public void Connect()    //  initializes a connection with client 
        {
            lock (executionLock)
            {
                try
                {
                    Connected = true;


                    // set static instance of the client
                    if (current == null) current = this;
                    else if (current != this) Logging.LogError(this + ": Failed to set active instance - it already is set to " + current);
                    else Logging.LogWarning(this + ": No need to setup this as active object - it already is");

                    Logging.LogInfo("Building connection to server...");

                    // get tcp and instruction handler ready
                    instructionHandler = new InstructionHandler(executionLock);
                    tcp = new TCPCore(tcpEndPoint, instructionHandler.HandleInstruction, () => { lock (executionLock) { availableTcp = false; instructionHandler.Deactivate(); } });
                    instructionHandler.SwitchToBufferingMode();
                    availableTcp = true;


                    Logging.LogAccept("Successfully connected. Starting data transfer protocol...");


                    // now we are connected and ready to begin data reading
                    CustomEventSystem.NotifyAboutConnect();
                    tcp.Open();
                }
                catch (SocketException exc)
                {
                    if (current == this) current = null;
                    tcp?.Close();
                    udp?.Close();
                    instructionHandler?.Deactivate();

                    tcp = null;
                    udp = null;
                    instructionHandler = null;
                    Connected = false;
                    availableTcp = false;

                    Destroy(gameObject);
                    Logging.LogCritical($"SocketException: {exc}");
                }
            }
        }

        public void Disconnect()              // safely disconnects client 
        {
            lock (executionLock)
            {
                // check if client is not already closed
                if (!Connected || Deleted || !availableTcp) { Logging.LogError("Disconnecting was turned down: socket is already closed."); return; }
                Connected = false;

                // invoke event before disconnecting
                try { CustomEventSystem.NotifyAboutDisconnect(); } finally { }

                tcp.Send(Instructions.Disconnect, new byte[] { 0 });       
            }
        }

        public void Delete()
        {
            lock (executionLock)
            {
                if (Deleted) return;

                Logging.LogDeny("Client " + clientId + " on address " + tcp.endPoint + " left the server.");

                if (Connected) try { CustomEventSystem.NotifyAboutForceDisconnect(); } finally { }


                instructionHandler.Deactivate();
                tcp.Close();
                udp?.Close();

                Connected = false;
                availableUdp = false;
                availableTcp = false;

                if (current == this) current = null;
                tcp = null;
                udp = null;
                instructionHandler = null;

                Deleted = true;

                Destroy(gameObject);
            }
        }




        #endregion
        //--------------------------------------------------
        #region EVENTS



        protected void Awake()
        {
            // initialize server-sent methods
            InstructionHandler.RegisterAllInstructions(typeof(InstructionAttribute));

            //Bytes.TestFunction();
        }

        protected void Start()
        {
            Connect();
        }

        protected override void BeforeConnect()
        {
            instructionHandler.RegisterInstructions(this);
        }

        protected override void NetworkUpdate()
        {
            if (availableUdp) udp.Send(Instructions.HelloWorld, Bytes.ToBytes($"{i++}"));
            instructionHandler?.ExecuteAllBuffered();

            lock (executionLock) if (!availableTcp) { instructionHandler.ExecuteAllBuffered(); Delete(); }
        }
        int i = 0;

        protected override void BeforeDisconnect() { }

        protected override void AfterUdpInvolved() { }


        protected void OnApplicationQuit()
        {
            lock (executionLock)
            {
                Disconnect();

                Tuple<short, byte[]> instruction = null;
                while(availableTcp)
                {
                    instruction = instructionHandler?.WaitForInstruction();
                    if (instruction == null || instructionHandler == null) break;
                    instructionHandler.Execute(instruction);
                }
                instructionHandler?.ExecuteAllBuffered();
                Delete();
            }
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


        [Instruction(Instructions.NotifyShutdown)]
        private void instNotifyShutdown(byte[] data)       // testing function 
        {
            if (data != null && data.Length > 0) Logging.LogInfo($"Server is going to be shut down. Reason: {Bytes.ToString(data)}");
            else Logging.LogInfo("Server is going to be shut down. Reason: unknown");
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
                    tcp.Send(Instructions.Disconnect, new byte[] { 1 }).Wait();
                    Delete();
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

                    udp = new UDPCore(instructionHandler.HandleInstruction);
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



        private bool Connected = false;
        private bool Deleted = false;
        private bool availableUdp = false;
        private bool availableTcp = false;



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
