using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Text;
using System.Net;
using System.Net.Sockets;
using System;
using System.Threading;


namespace Client
{
    public class Client : MonoBehaviour
    {
        public static Client client = null;
        public Dictionary<byte, Channel> channels = new Dictionary<byte, Channel>();

        private IPAddress ip = IPAddress.Parse("127.0.0.1");
        private int port = 23852;

        private TcpClient tcp;
        private NetworkStream stream;

        // constructor for intitializing new client
        public void Awake()
        {
            tcp = new TcpClient();
            tcp.Connect(ip, port);

            Connect();
        }


        // function for connectiong client-side client

        public void Connect()
        {
            // ser static instance of the client
            if (client == null) client = this;
            else if (client != this) Debug.LogError(this + ": Failed to set active instance - it already is set to " + client);
            else Debug.LogWarning(this + ": No need to setup this as active object - it already is");
            
            // get tcp network stream
            stream = tcp.GetStream();

            // add default system channel
            channels.Add(0, new DynamicChannel());

            // begin data reading
            stream.BeginRead(recvChannelBytes, 0, 1, OnDataIncome, null);
        }

        private byte[] recvChannelBytes = new byte[1];
        private void OnDataIncome(IAsyncResult result)
        {
            // get channel to which we are reading
            stream.EndRead(result);
            byte channel = recvChannelBytes[0];
            Channel currentChannel = channels[channel];

            lock(currentChannel)
            {
                
                byte[] data = currentChannel.Receive(stream);
                currentChannel.Write(data);

                
                Gdebug.Log(Converter.ToString(currentChannel.Read()));

            }
            // start waiting for the next data
            stream.BeginRead(recvChannelBytes, 0, 1, OnDataIncome, null);
        }

        private void ClearAllBuffers()
        {
            foreach(KeyValuePair<byte, Channel> channel in channels)
            {
                channel.Value.ClearBuffer();
            }
        }

    }

    public class Channel
    {
        public Channel() { }

        public Stack<byte[]> dataBuffer = new Stack<byte[]>();

        public byte id = 0;

        public void ClearBuffer() { lock (this) dataBuffer.Clear(); }

        public void Write(byte[] data) { dataBuffer.Push(data); }

        public byte[] Read() { return dataBuffer.Pop(); }

        public virtual byte[] Receive(NetworkStream stream) { return null; }

        public virtual void Send(NetworkStream stream, byte[] data) { }
    }

    public class DynamicChannel : Channel
    {
        public override void Send(NetworkStream stream, byte[] data)
        {
            byte[] sendChannelBytes = new byte[1];
            sendChannelBytes[0] = id;

            stream.Write(sendChannelBytes, 0, 1);
            stream.Write(Converter.ToBytes(data.Length), 0, 4);
            stream.Write(data, 0, data.Length);
        }  


        private byte[] dataSizeBytes = new byte[4];
        public override byte[] Receive(NetworkStream stream)
        {
            // get data size
            stream.Read(dataSizeBytes, 0, 4);
            int dataSize = BitConverter.ToInt32(dataSizeBytes, 0);


            // receive data
            byte[] data = new byte[dataSize];
            stream.Read(data, 0, dataSize);

            return data;
        }
    }

    public class StreamChannel : Channel
    {
        public int readSize = 0;

        public override void Send(NetworkStream stream, byte[] data)
        {
            byte[] sendChannelBytes = new byte[1];
            sendChannelBytes[0] = id;

            stream.Write(sendChannelBytes, 0, 1);
            stream.Write(data, 0, data.Length);
        }

        public override byte[] Receive(NetworkStream stream)
        {
            byte[] data = new byte[readSize];
            stream.Read(data, 0, readSize);

            return data;
        }
    }

}
