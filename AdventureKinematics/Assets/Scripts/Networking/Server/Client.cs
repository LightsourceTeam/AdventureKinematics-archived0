using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Text;
using System.Net;
using System.Net.Sockets;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Server
{
    public class Client
    {
        // state of the client
        public bool isAlive { get; private set; } = false;


        // main  channel list
        public Dictionary<byte, Channel> channels = new Dictionary<byte, Channel>();

        // overload list constructor for convenience
        public Channel this[byte i]
        {
            get { return channels[i]; }
            set { channels[i] = value; }
        }

        // cliant identifier at server
        public int id;

        public TcpClient tcp;
        public NetworkStream stream;


        // function for strating server-side client management
        public void Connect(TcpClient client, int clientId)
        {
            // get tcp network stream
            tcp = client;
            stream = tcp.GetStream();
            id = clientId;

            // add default system channel
            AddDynamicChannel();

            // state that client is alive now;
            isAlive = true;

            // begin data reading
            stream.BeginRead(recvChannelBytes, 0, 1, DataAvailable, null);

        }

        public void Disconnect()
        {
            lock (this)
            {
                if (isAlive)
                {
                    stream.Close();
                    tcp.Close();
                    Server.server.RemoveClient(this);
                    isAlive = false;
                }
            }
        }


        // function that responds every time data comes to you
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
                channels.Add(lastChannel, new StreamChannel(this, lastChannel));
                return lastChannel++;
            }
        }

        public byte AddDynamicChannel()
        {
            lock (this)
            {
                channels.Add(lastChannel, new DynamicChannel(this, lastChannel));
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
        protected Channel(Client client, byte identifier) { this.client = client; id = identifier; }


        public Stack<byte[]> dataBuffer = new Stack<byte[]>();
        protected Client client;
        public byte id = 0;


        public void ClearBuffer() { lock(this) dataBuffer.Clear(); }


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
        public DynamicChannel(Client client, byte identifier) : base(client, identifier) { }


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
        public StreamChannel(Client client, byte identifier) : base(client, identifier) { }


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
