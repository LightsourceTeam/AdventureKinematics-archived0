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

        private TcpListener tcpListener;
        public static Dictionary<int, Client> clients = new Dictionary<int, Client>();

        public void Start()
        {
            // setting active instance of the server
            if(server == null) server = this;
            else if(server != this) Gdebug.LogError(this + ": You can not have two servers run simultaneously!");
            else Gdebug.LogWarning(this +": No need to set it as active server - it already has this value");

            // initilaize tcp listener and start accepting clients
            tcpListener = new TcpListener(ip, port);          
            Gdebug.Log("Starting server on " + ip + " " + port + "...");
            tcpListener.Start();
            
            Gdebug.Log("Start accepting clients on " + ip + " " + port + "...");
            tcpListener.BeginAcceptTcpClient(new AsyncCallback(TCPConnectCallback), null);
        }

        public void TCPConnectCallback(IAsyncResult result)
        {
            // accept new client, and start listening for the new one.
            TcpClient client = AcceptClient(result);

            // alert that new client has connected
            Gdebug.Log("Incoming Connection: " + client.Client.RemoteEndPoint);
            AddClient(new Client(), client);

            //Gdebug.Log("Connection Failed: " + client.Client.RemoteEndPoint);
        }

        public TcpClient AcceptClient(IAsyncResult result)
        {
            // accept new client, and start listening for the new one.
            TcpClient client = tcpListener.EndAcceptTcpClient(result);
            tcpListener.BeginAcceptTcpClient(new AsyncCallback(TCPConnectCallback), null);

            return client;
        }
                
        private int lastId = 0;
        public void AddClient(Client client, TcpClient newTcp)
        {
            clients.Add(lastId, client);
            clients[lastId].Connect(newTcp);
            clients[lastId].id = lastId;
            lastId++;
        }

        public void RemoveClient(Client client)
        {
            client.tcp.Close();
            clients.Remove(client.id);
        }

    }
}
