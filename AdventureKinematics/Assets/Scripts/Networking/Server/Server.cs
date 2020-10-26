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
        public static IPAddress ip = IPAddress.Parse("127.0.0.1");
        public static int port = 23852;

        public static Dictionary<int, Client> clients = new Dictionary<int, Client>();
        private TcpListener tcpListener;

        bool isAcceptingNewClients = false;


        public static Dictionary<short, MethodInfo> instructions { get; private set; }



        #endregion
        //--------------------------------------------------
        #region SERVER INITIALIZING
        /// initializing server



        private void Awake()       // unity function for starting server 
        {
            RegisterAllInstructions();
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

            // initilaize tcp listener and start accepting clients
            tcpListener = new TcpListener(ip, port);
            Logging.LogInfo("Starting server on " + ip + " " + port + "...");
            tcpListener.Start();

            Logging.LogInfo("Start accepting clients on " + ip + " " + port + "...");
            tcpListener.BeginAcceptTcpClient(ConnectCallback, null);

        }            



        #endregion
        //--------------------------------------------------
        #region ADMINISTRATOR FUNCTIONS
        /// initializing administator functions



        public void Shutdown()
        {
            Logging.Log("Warning: Server has been put into shutdown state! Waiting until connected clients leave...");
        }



        #endregion
        //--------------------------------------------------
        #region INTERNAL METHODS
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


        public void RemoveClient(Client client)
        {
            lock(this) clients.Remove(client.clientId);
        } 

        public void RegisterAllInstructions()
        {
            if (instructions != null) return;

            instructions = (from type in Assembly.GetExecutingAssembly().GetTypes()
                            from method in type.GetMethods(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance)
                            where method.GetCustomAttribute(typeof(Client.InstructionAttribute)) != null
                            select method).ToDictionary(x => (x.GetCustomAttribute(typeof(Client.InstructionAttribute)) as Client.InstructionAttribute).id);
        }


        #endregion
        //--------------------------------------------------
    }
}
