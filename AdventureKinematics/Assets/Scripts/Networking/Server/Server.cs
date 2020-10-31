using System.Collections.Generic;
using UnityEngine;
using System.Net;
using System.Net.Sockets;
using System;
using SourceExtensions;
using System.Reflection;
using System.Linq;


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
        public static IPEndPoint tcpEndpPoint = new IPEndPoint(IPAddress.Parse("127.0.0.1"), 23852);
        public static IPEndPoint udpEndpPoint = new IPEndPoint(IPAddress.Parse("127.0.0.1"), 23853);

        public static Dictionary<int, Client> clients = new Dictionary<int, Client>();
        public static Dictionary<IPEndPoint, Client> udpConnections = new Dictionary<IPEndPoint, Client>();
        
        private TcpListener tcpListener;
        private UdpClient udpListener;

        bool isAcceptingNewClients = false;



        #endregion
        //--------------------------------------------------
        #region SERVER INITIALIZING
        /// initializing server



        private void Awake()       // unity function for starting server 
        {
            Client.RegisterAllInstructions();
            Raise();
        }           

        public void Raise()        // reises (starts) server 
        {
            isAcceptingNewClients = true;

            // setting active instance of the server
            if (server == null) server = this;
            else if (server != this) Logging.LogError(this + ": You can not have two servers run simultaneously!");
            else Logging.LogWarning(this + ": No need to set it as active server - it already has this value");

            Logging.Log("\n\n");

            // initilaize tcp listener
            Logging.LogInfo("Starting server on " + tcpEndpPoint + "...");
            tcpListener = new TcpListener(tcpEndpPoint);
            
            // initialize udp listener
            Logging.LogInfo("Initializing udp on " + udpEndpPoint + "...");
            udpListener = new UdpClient();

            Logging.LogInfo("Starting tcp management...");
            tcpListener.Start();
            tcpListener.BeginAcceptTcpClient(ConnectCallback, null);


            Logging.LogInfo("Starting udp management...");
            udpListener.BeginReceive(ManageUdpData, null);


            Logging.LogAccept("Done! Starting to manange clients on " + tcpEndpPoint + "...");
        }            



        #endregion
        //--------------------------------------------------
        #region ADMINISTRATIVE
        /// initializing administator functions



        public void Shutdown()
        {
            Logging.Log("Warning: Server has been put into shutdown state! Waiting until connected clients leave...");
        }

        public void RemoveClient(Client client)
        {
            lock (this) clients.Remove(client.clientId);
        }



        #endregion
        //--------------------------------------------------
        #region INTERNAL
        /// these methods will be executed only here



        private void ConnectCallback(IAsyncResult result)
        {
            // accept new client, and start listening for the new one.
            TcpClient tcp = tcpListener.EndAcceptTcpClient(result);

            // alert that new client has connected
            Logging.LogAccept("Incoming Connection: " + tcp.Client.RemoteEndPoint);

            Client client = new Client();


            lock (this) clients.Add(lastId, client);
            client.Connect(tcp, lastId);
            lastId++;

            if (isAcceptingNewClients) tcpListener.BeginAcceptTcpClient(ConnectCallback, null);
        }
        private int lastId = 0;

        private void ManageUdpData(IAsyncResult result)
        {
            IPEndPoint endPoint = null;
            Bytes.Couple data = udpListener.EndReceive(result, ref endPoint);
            udpListener.BeginReceive(ManageUdpData, null);

            Client destination = null;
            if (udpConnections.TryGetValue(endPoint, out destination)) destination.udp.OnDataReceive(data);
        }

        #endregion
        //--------------------------------------------------
    }
}
