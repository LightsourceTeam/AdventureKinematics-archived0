using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Text;
using System.Net;
using System.Net.Sockets;
using System;

namespace Server
{
    public class Server : MonoBehaviour
    {
        public static Server server;

        public static IPAddress ip = IPAddress.Parse("127.0.0.1");
        public static int port = 23852;
        public static Dictionary<int, Client> clients = new Dictionary<int, Client>();

        private TcpListener tcpListener;


        public event Action onServerInit;
        public event Action onServerShutdown;
        public event Action<Client> onClientConnect;
        public event Action<Client> onClientDisconnect;


        public bool shutdownState { get; private set; } = false;
        public bool initState { get; private set; } = false;

        private void Awake()
        {
            Raise();
        }


        public void Raise()
        {
            // setting active instance of the server
            if(server == null) server = this;
            else if(server != this) Logging.LogError(this + ": You can not have two servers run simultaneously!");
            else Logging.LogWarning(this +": No need to set it as active server - it already has this value");

            Logging.Log("\n\n");

            // initilaize tcp listener and start accepting clients
            tcpListener = new TcpListener(ip, port);          
            Logging.LogInfo("Starting server on " + ip + " " + port + "...");
            tcpListener.Start();

            Logging.LogInfo("Start accepting clients on " + ip + " " + port + "...");
            tcpListener.BeginAcceptTcpClient(new AsyncCallback(ConnectCallback), null);

            onClientConnect += ClientHandler.InitializeClientHandler;

            initState = true;
            onServerInit?.Invoke();
        }

        public void Shutdown()
        {
            Logging.Log("Warning: Server was put into shutdown state! Waiting until connected clients leave...");
            
            shutdownState = true;
            onServerShutdown?.Invoke();
        }

        public void ConnectCallback(IAsyncResult result)
        {
            // accept new client, and start listening for the new one.
            TcpClient client = AcceptClient(result);

            // alert that new client has connected
            Logging.LogInfo("Incoming Connection: " + client.Client.RemoteEndPoint);
            AddClient(new Client(), client);

        }

        public TcpClient AcceptClient(IAsyncResult result)
        {
            // accept new client, and start listening for the new one.
            TcpClient client = tcpListener.EndAcceptTcpClient(result);
            if(!shutdownState) tcpListener.BeginAcceptTcpClient(new AsyncCallback(ConnectCallback), null);

            return client;
        }
         
        
        private int lastId = 0;
        public void AddClient(Client client, TcpClient newTcp)
        {
            clients.Add(lastId, client);
            client.Connect(newTcp, lastId++);

            onClientConnect?.Invoke(client);
        }

        public void RemoveClient(Client client)
        {
            clients.Remove(client.id);
            onClientDisconnect?.Invoke(client);
        }

    }
}
