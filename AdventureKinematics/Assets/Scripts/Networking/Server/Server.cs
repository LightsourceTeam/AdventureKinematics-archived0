using System.Collections.Generic;
using System.Collections.Concurrent;
using UnityEngine;
using System.Net;
using System.Net.Sockets;
using System;
using SourceExtensions;
using System.Threading.Tasks;
using static SourceExtensions.UnlockObject;

namespace Networking.Server
{
    public class Server : MonoBehaviour
    {
        //--------------------------------------------------
        #region VARIABLES
        /// public variables declaration



        /// initializing server instance
        public static Server server;

        /// initializing connection variables & client dictionary
        public static IPEndPoint tcpEndPoint = new IPEndPoint(IPAddress.Any, 23852);
        public static IPEndPoint udpEndPoint = new IPEndPoint(IPAddress.Any, 23853);

        public static ConcurrentDictionary<int, Client> clients = new ConcurrentDictionary<int, Client>();
        public static ConcurrentDictionary<IPEndPoint, Client> udpConnections = new ConcurrentDictionary<IPEndPoint, Client>();

        public TcpListener tcpListener { get; private set; } = null;
        public UdpClient udpListener { get; private set; } = null;

        bool isAlive = false;


        #endregion
        //--------------------------------------------------
        #region ADMINISTRATIVE
        /// initializing server



        private void Awake()       // unity function for starting server 
        {
            InstructionHandler.RegisterAllInstructions(typeof(Client.InstructionAttribute));
            Raise();
        }

        public void Raise()        // raises (starts) server 
        {
            isAlive = true;

            // setting active instance of the server
            if (server == null) server = this;
            else if (server != this) Logging.LogError(this + ": You can not have two servers run simultaneously!");
            else Logging.LogWarning(this + ": No need to set it as active server - it already has this value");

            ///Application.wantsToQuit += OnServerQuit;             // WORKS BAD

            Logging.Log("\n\n");

            // initilaize tcp listener
            Logging.LogInfo($"Initializing tcp on {tcpEndPoint}...");
            tcpListener = new TcpListener(tcpEndPoint);
            tcpEndPoint = (IPEndPoint)tcpListener.LocalEndpoint;

            // initialize udp listener
            Logging.LogInfo($"Initializing udp on {udpEndPoint}...");
            udpListener = new UdpClient(udpEndPoint.Port);
            udpEndPoint = (IPEndPoint)udpListener.Client.LocalEndPoint;


            Logging.LogInfo("Starting tcp management...");
            tcpListener.Start();
            tcpListener.BeginAcceptTcpClient(TCPConnectCallback, null);


            Logging.LogInfo("Starting udp management...");
            udpListener.BeginReceive(UDPManageDataCallback, null);


            Logging.LogAccept($"Done! Starting to accept clients on {tcpEndPoint}...");
        }            

        public Task Shutdown()
        {
            return Task.Run(() => 
            {
                shouldAcceptNewConnections = false;
                while (!clients.IsEmpty)
                    foreach (var client in clients)
                        client.Value.Delete();
                    });
        }

        public Task NotifyShutdown(string reason)
        {
            byte[] reasonInBytes = Bytes.ToBytes(reason);
            return Task.Run(() =>
            {
                shouldAcceptNewConnections = false;
                foreach (var client in clients)
                    client.Value.NotifyShutdown(reasonInBytes);
            });
        }



        #endregion
        //--------------------------------------------------
        #region EVENTS



        /// WARNING! Works bad, needs more attention
        public bool OnServerQuit()
        {
            Task.Run(() =>
            {
                try
                {
                    int n = 5;


                    for (int i = 0; i < n; i++)
                    {
                        NotifyShutdown($"Test {i}").Wait();
                    }
                    Task.Delay(50000).Wait();
                    Shutdown().Wait();
                }
                catch (Exception exc) { Logging.Log(exc); }
            });
            return false;
        }




        #endregion
        //--------------------------------------------------
        #region CALLBACKS
        /// initializing administator functions



        public Client RemoveClient(Client client) 
        { if (client == null) return null;


            clients?.TryRemove(client.clientId, out client);

            return client;
        }

        public void RegisterUdp(IPEndPoint endPoint, Client client)
        { 
            if (endPoint == null) return; 
            lock (udpConnections) udpConnections[endPoint] = client; 
        }

        public Client UnregisterUdp(IPEndPoint endPoint) 
        { 
            if (endPoint == null) return null;

            Client removed = null;
            udpConnections.TryRemove(endPoint, out removed);
            return removed;
        }



        #endregion
        //--------------------------------------------------
        #region INTERNAL
        /// these methods will be executed only here



        private void TCPConnectCallback(IAsyncResult result)
        {
            // accept new client, and (if allowed) start listening for the new one.
            TcpClient tcp = tcpListener.EndAcceptTcpClient(result);
            lastId++;
            if (shouldAcceptNewConnections) tcpListener.BeginAcceptTcpClient(TCPConnectCallback, null);
            else tcpListener.Stop();

            // initialize a new client instance, and connect it
            Client client = new Client();
            lock (client.executionLock)
            {
                if (!clients.TryAdd(lastId, client)) { tcp.Close(); return; }

                client.Connect(tcp, lastId);
            }
        }
        bool shouldAcceptNewConnections { get { lock (this) return isAlive; } set { lock (this) isAlive = value; } }
        int lastId = 0;

        private void UDPManageDataCallback(IAsyncResult result)
        {
            try
            {
                IPEndPoint endPoint = null;


                Bytes.Couple data = udpListener.EndReceive(result, ref endPoint);
                udpListener.BeginReceive(UDPManageDataCallback, null);
                
                Client destination = null;
                if (!udpConnections.TryGetValue(endPoint, out destination)) return;

                destination.udp.OnDataReceive(data);
            }
            catch (ObjectDisposedException)
            {
                Logging.LogCritical("UDP LISTENER IS CLOSED!!! Shutting down the server...");
                return;
            }

        }



        #endregion
        //--------------------------------------------------
    }
}
