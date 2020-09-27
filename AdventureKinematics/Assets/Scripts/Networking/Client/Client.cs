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

        public Channel this[byte i]
        {
            get { return channels[i]; }
            set { channels[i] = value; }
        }

        private IPAddress ip = IPAddress.Parse("127.0.0.1");
        private int port = 23852;

        public TcpClient tcp;
        public NetworkStream stream;


        // constructor for intitializing new client
        public void Awake()
        {
            Connect();
        }


        // function for connecting client-side client

        public void Connect()
        {
            // ser static instance of the client
            if (client == null) client = this;
            else if (client != this) Logging.LogError(this + ": Failed to set active instance - it already is set to " + client);
            else Logging.LogWarning(this + ": No need to setup this as active object - it already is");

            Logging.LogInfo("Building connection to server...");

            // get tcp network stream
            tcp = new TcpClient();
            tcp.Connect(ip, port);
            stream = tcp.GetStream();

            Logging.LogInfo("Successfully connected. Starting data transfer protocol...");

            // add default system channel
            AddDynamicChannel();

            // begin data reading
            stream.BeginRead(recvChannelBytes, 0, 1, DataAvailable, null);
        }

        private byte[] recvChannelBytes = new byte[1];
        private void DataAvailable(IAsyncResult result)
        {
            // get channel to which we are reading
            stream.EndRead(result);

            // get channel to which we are reading
            byte channel = recvChannelBytes[0];
            Channel currentChannel = null;
            lock (this) currentChannel = channels[channel];

            // accept data
            currentChannel.ReadFromStream();


            // start waiting for the next data
            stream.BeginRead(recvChannelBytes, 0, 1, DataAvailable, null);
        }

        private void ClearAllBuffers()
        {
            foreach (KeyValuePair<byte, Channel> channel in channels)
            {
                channel.Value.ClearBuffer();
            }
        }

        byte lastChannel = 0;
        public byte AddStreamChannel()
        {
            lock (this)
            {
                channels.Add(lastChannel, new StreamChannel(lastChannel));
                return lastChannel++;
            }
        }

        public byte AddDynamicChannel()
        {
            lock (this)
            {
                channels.Add(lastChannel, new DynamicChannel(lastChannel));
                return lastChannel++;
            }
        }

        public void RemoveChannel(byte channel)
        {
            lock (this) channels.Remove(channel);
        }
    }



    public class Channel
    {
        protected Channel(byte identifier) { id = identifier; }


        public Stack<byte[]> dataBuffer = new Stack<byte[]>();
        protected Client client = Client.client;
        public byte id = 0;

        public void ClearBuffer() { lock (this) dataBuffer.Clear(); }


        // send data through this channel
        public virtual void Send(byte[] data) { }

        // receive data from this channel
        public byte[] Recv(int timeout = -1)
        {
            lock (this)
            {
                // if data is already present, we'll get it
                if (dataBuffer.Count > 0) return dataBuffer.Pop();

                // if not, we'll wait for it *timeout* milliseconds. *timeout = -1* means wait forever.
                if (Monitor.Wait(this, timeout)) return dataBuffer.Pop();

                // if no data within specified time, stop waiting and return null
                return null;
            }
        }


        // function which gets called by server when new data arrives. Do NOT call it when client is running!
        public virtual void ReadFromStream() { }
    }



    public class DynamicChannel : Channel
    {
        public DynamicChannel(byte identifier) : base(identifier) { }

        public override void Send(byte[] data)
        {
            byte[] sendChannelBytes = new byte[1];

            lock (this)
            {
                sendChannelBytes[0] = id;
                client.stream.Write(sendChannelBytes, 0, 1);
                client.stream.Write(Converter.ToBytes(data.Length), 0, 4);
                client.stream.Write(data, 0, data.Length);
            }
        }


        private byte[] dataSizeBytes = new byte[4];
        public override void ReadFromStream()
        {
            // get data size
            client.stream.Read(dataSizeBytes, 0, 4);
            int dataSize = BitConverter.ToInt32(dataSizeBytes, 0);

            // receive data
            byte[] data = new byte[dataSize];
            client.stream.Read(data, 0, dataSize);


            // push received data to buffer, and fire a message thet new data has been sent
            lock (this)
            {
                dataBuffer.Push(data);
                Monitor.Pulse(this);
            }
        }
    }



    public class StreamChannel : Channel
    {
        public StreamChannel(byte identifier) : base(identifier) { }

        public int readSize = 0;

        public override void Send(byte[] data)
        {
            byte[] sendChannelBytes = new byte[1];

            lock (this)
            {
                sendChannelBytes[0] = id;
                client.stream.Write(sendChannelBytes, 0, 1);
                client.stream.Write(data, 0, data.Length);
            }
        }


        public override void ReadFromStream()
        {
            // receive data
            byte[] data = new byte[readSize];
            client.stream.Read(data, 0, readSize);


            // push received data to buffer, and fire a message thet new data has been sent
            lock (this)
            {
                dataBuffer.Push(data);
                Monitor.Pulse(this);
            }
        }
    }

}
