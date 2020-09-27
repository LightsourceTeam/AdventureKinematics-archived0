using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;
using System.Collections.Specialized;

namespace Server
{
    public class ClientHandler
    {
        public event Action onUpdate;

        public Client client { get; private set; } = null;

        protected ClientHandler(Client clientToHandle) { client = clientToHandle; }

        public static void InitializeClientHandler(Client clientToHandle)
        {
            // create new client handler and initialize it
            ClientHandler clientHandler = new ClientHandler(clientToHandle);

            // run handling client loop in parallel
            Task.Run(() => { HandlerLoop(clientHandler); });
        }

        private bool shouldContinue() { lock (client) return client.isAlive; }

        protected static void HandlerLoop(ClientHandler self)
        {
            while (self.shouldContinue())
            {
                self.Update();
                self.onUpdate?.Invoke();
            }
        }

        protected void Update()
        {
            // 
        }

    }
}
