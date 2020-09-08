﻿using System.Collections;
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
       
        public static Client instance;
        public Dictionary<byte, Channel> channels = new Dictionary<byte, Channel>();
        
        public int id;

        public TcpClient tcp;
        private NetworkStream stream;


        // function for strating server-side client management

        public void Connect(TcpClient client)
        {
            // get tcp network stream
            tcp = client;
            stream = tcp.GetStream();

            // add default system channel
            channels.Add(0, new DynamicChannel(0));

            // begin data reading
            stream.BeginRead(recvChannelBytes, 0, 1, onDataIncome, null);

            channels[0].Send(stream, Converter.ToBytes("Hello World!"));
        }
          
        
        // function that responds every time data comes to you

        private byte[] recvChannelBytes = new byte[1];
        private void onDataIncome(IAsyncResult result)
        {
            // end receiveing data
            stream.EndRead(result);

            // get channel to which we are reading
            byte channel = recvChannelBytes[0];
            Channel currentChannel = channels[channel];

            // accept data
            lock(currentChannel)        // lock this channel to prevent data races
            {
                byte[] data = currentChannel.Receive(stream);
                currentChannel.Write(data);
            }

            // start waiting for the next data
            stream.BeginRead(recvChannelBytes, 0, 1, onDataIncome, null);
        }

        private void ClearAllBuffers()
        {
            foreach (KeyValuePair<byte, Channel> channel in channels)
            {
                channel.Value.ClearBuffer();
            }
        }

    }

    public class Channel
    {
        protected Channel() { }

        public Stack<byte[]> dataBuffer = new Stack<byte[]>();

        public byte id = 0;

        public void ClearBuffer() { lock (this) dataBuffer.Clear(); }

        public void Write(byte[] data) { dataBuffer.Push(data); }

        public byte[] Read() { return dataBuffer.Pop();  }

        public virtual byte[] Receive(NetworkStream stream) { return null; }

        public virtual void Send(NetworkStream stream, byte[] data) { }
    }

    public class DynamicChannel : Channel
    {
        public DynamicChannel(byte id) { this.id = id; }

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
        public StreamChannel(byte id) { this.id = id; }

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
