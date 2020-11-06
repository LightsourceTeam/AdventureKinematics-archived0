using System.Collections.Generic;
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

        public static Dictionary<int, Client> clients = new Dictionary<int, Client>();
        public static Dictionary<IPEndPoint, Client> udpConnections = new Dictionary<IPEndPoint, Client>();

        public TcpListener tcpListener { get; private set; } = null;
        public UdpClient udpListener { get; private set; } = null;

        bool isAlive = false;



        #endregion
        //--------------------------------------------------
        #region SERVER INITIALIZING
        /// initializing server



        private void Awake()       // unity function for starting server 
        {
            ServerSideObject.RegisterAllInstructions();
            Raise();
        }

        public void Raise()        // raises (starts) server 
        {
            isAlive = true;

            // setting active instance of the server
            if (server == null) server = this;
            else if (server != this) Logging.LogError(this + ": You can not have two servers run simultaneously!");
            else Logging.LogWarning(this + ": No need to set it as active server - it already has this value");

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



        #endregion
        //--------------------------------------------------
        #region ADMINISTRATIVE
        /// initializing administator functions



        public void RemoveClient(Client client) { if (client == null) return; lock (clients) clients.Remove(client.clientId); }

        public void RegisterUdp(IPEndPoint endPoint, Client client)
        { 
            if (endPoint == null) return; 
            lock (udpConnections) udpConnections[endPoint] = client; 
        }

        public bool UnregisterUdp(IPEndPoint endPoint) { if (endPoint == null) return false; lock (udpConnections) return udpConnections.Remove(endPoint); }



        #endregion
        //--------------------------------------------------
        #region INTERNAL
        /// these methods will be executed only here



        private void TCPConnectCallback(IAsyncResult result)
        {
            // accept new client, and start listening for the new one.
            TcpClient tcp = tcpListener.EndAcceptTcpClient(result);
            lastId++;
            if (shouldLive) tcpListener.BeginAcceptTcpClient(TCPConnectCallback, null);

            Client client = new Client();
            clients.Add(lastId, client);
            client.Connect(tcp, lastId);
        }
        bool shouldLive { get { lock (this) return isAlive; } set { lock (this) isAlive = value; } }
        int lastId = 0;

        private void UDPManageDataCallback(IAsyncResult result)
        {
            try
            {
                IPEndPoint endPoint = null;


                Bytes.Couple data = udpListener.EndReceive(result, ref endPoint);
                udpListener.BeginReceive(UDPManageDataCallback, null);
                
                Client destination = null;
                lock (udpConnections) if (!udpConnections.TryGetValue(endPoint, out destination)) return;

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
