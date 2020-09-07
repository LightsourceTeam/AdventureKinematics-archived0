using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Text;
using System.Net;
using System.Net.Sockets;
using System;

namespace Server
{
    public class ServerSend : MonoBehaviour
    {
        private static void SendTCPData(int client, byte[] data, int size)
        {
            Server.clients[client].SendData(data, size); 
        }

        private static void SendTCPDataToAll(int client, byte[] data, int size)
        {
            for (int i = 0; i < Server.clients.Count; i++)
            {
                Server.clients[i].SendData(data, size);
            }
        }

        public static void Welcome(int client, string msg, int size)
        {
            byte[] buffer = new byte[size];
            buffer = Converter.ToBytes(msg);

            SendTCPData(client, buffer, size);
        }
    }
}