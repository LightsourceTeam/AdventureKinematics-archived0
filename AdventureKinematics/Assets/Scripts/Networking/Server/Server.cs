using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Text;
using System.Net;
using System.Net.Sockets;
using System;

namespace Server
{
    public class Server
    {
        public static Server server;

        public static IPAddress ip = IPAddress.Parse("127.0.0.1");
        public static int port = 23852;

        public TcpListener tcpListener;
        public Dictionary<int, Client> clients = new Dictionary<int, Client>();

        public int lastId = 0;
        
        public void Start()
        {
            // setting active instance of the server
            if(server == null) server = this;
            else if(server != this) Debug.LogError(this + ": You can not have two servers run simultaneously!");
            else Debug.LogWarning(this +": No need to set it as active server - it already has this value");

            // initilaize tcp listener and start accepting clients
            tcpListener = new TcpListener(ip, port);
            tcpListener.Start();
            tcpListener.BeginAcceptTcpClient(new AsyncCallback(TCPConnectCallback), null);
        }

        public void TCPConnectCallback(IAsyncResult result)
        {
            // accept new client, and start listening for the new one.
            TcpClient client = AcceptClient(result);

            // alert that new client has connected
            Debug.Log("Incoming Connection: " + client.Client.RemoteEndPoint);
            
            AddClient(new Client(client));

            //Debug.Log("Connection Failed: " + client.Client.RemoteEndPoint);
        }

        public TcpClient AcceptClient(IAsyncResult result)
        {
            // accept new client, and start listening for the new one.
            TcpClient client = tcpListener.EndAcceptTcpClient(result);
            tcpListener.BeginAcceptTcpClient(new AsyncCallback(TCPConnectCallback), null);

            return client;
        }
        
        public void AddClient(Client client)
        {
            clients.Add(lastId, client);
            clients[lastId].Connect();
            clients[lastId].id = lastId;
            lastId++;
        }

        public void RemoveClient(Client client)
        {
            clients.Remove(client.id);
        }

    }
}
