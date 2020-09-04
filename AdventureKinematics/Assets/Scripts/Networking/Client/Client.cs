using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Text;
using System.Net;
using System.Net.Sockets;
using System;


namespace Client
{
    public class Client : MonoBehaviour
    {
        public static Client client = null;
        public Dictionary<byte, Channel> channels = new Dictionary<byte, Channel>();

        private TcpClient tcp;
        private NetworkStream stream;

        // constructor for intitializing new client
        public Client(TcpClient tcpClient)
        {
            tcp = tcpClient;
        }

        public void Connect()
        {
            // ser static instance of the client
            if (client == null) client = this;
            else if (client != this) Debug.LogError(this + ": Failed to set active instance - it already is set to " + client);
            else Debug.LogWarning(this + ": No need to setup this as active object - it already is");
            
            // get tcp network stream
            stream = tcp.GetStream();

            // add default system channel
            channels.Add(0, new Channel());

            // begin data reading
            stream.BeginRead(recvChannelBytes, 0, 1, Read, null);
        }

        private byte[] recvChannelBytes = new byte[1];
        private byte[] dataSizeBytes = new byte[4];
        private void Read(IAsyncResult result)
        {
            // get channel to which we are reading
            stream.EndRead(result);
            byte channel = recvChannelBytes[0];


            Channel currentChannel = channels[channel];
            if (currentChannel.type == ChannelType.stream)
            {
                // receive data
                byte[] data = new byte[currentChannel.readSize];
                stream.Read(data, 0, currentChannel.readSize);
                currentChannel.dataBuffer.Push(data);
            }
            else
            {
                // get data size
                stream.Read(dataSizeBytes, 0, currentChannel.readSize);
                int dataSize = BitConverter.ToInt32(dataSizeBytes, 0);

                // receive data
                byte[] data = new byte[dataSize];
                stream.Read(data, 0, dataSize);
                currentChannel.dataBuffer.Push(data);
            }


            // start waiting for the next data
            stream.BeginRead(recvChannelBytes, 0, 1, Read, null);
        }

        private byte[] sendChannelBytes = new byte[1];
        private void Write(byte[] data, int size, byte channel)
        {
            sendChannelBytes[0] = channel;
            stream.Write(sendChannelBytes, 0, 1);
            stream.Write(data, 0, size);
        }

    }

    public enum ChannelType
    {
        json,
        list,
        stream
    }

    public class Channel
    {
        public Channel() { }

        public Stack<byte[]> dataBuffer = new Stack<byte[]>();

        public byte id = 0;
        public int readSize = 0;
        public ChannelType type = ChannelType.list;

    }
}
