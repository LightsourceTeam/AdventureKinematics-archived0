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
       
        public static Client instance;
        public Dictionary<byte, Channel> channels = new Dictionary<byte, Channel>();
        
        public int id;

        public TcpClient tcp;
        private NetworkStream stream;

        private Mutex mutex = new Mutex();

        // constructor for intitializing new client
        public Client(TcpClient client)
        {
            tcp = client;
        }

        public void Connect()
        {
            // get tcp network stream
            stream = tcp.GetStream();

            // add default system channel
            channels.Add(0, new Channel());

            // begin data reading
            stream.BeginRead(recvChannelBytes, 0, 1, onDataIncome, null);

            ServerSend.Welcome(0, "Hello World", 4096);
        }
          
        private byte[] recvChannelBytes = new byte[1];
        private byte[] dataSizeBytes = new byte[4];
        private void onDataIncome(IAsyncResult result)
        {
            // get channel to which we are reading
            stream.EndRead(result);
            byte channel = recvChannelBytes[0];


            Channel currentChannel = channels[channel];


            if (currentChannel.type == ChannelType.stream)
            {
                // receive data
                mutex.WaitOne();
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
                mutex.WaitOne();
                byte[] data = new byte[currentChannel.readSize];
                stream.Read(data, 0, dataSize);
                currentChannel.dataBuffer.Push(data);
            }

            mutex.ReleaseMutex();
            // start waiting for the next data
            stream.BeginRead(recvChannelBytes, 0, 1, onDataIncome, null);
        }

        public byte[] Read(byte channel, bool waitForData)
        {
            
            mutex.WaitOne();
            byte[] data = null;
            if (!waitForData)
            {
                Channel currentChannel = channels[channel];
                if (currentChannel.dataBuffer.Count > 0) 
                {
                    data = channels[channel].dataBuffer.Pop();
                }
            }
            mutex.ReleaseMutex();

            return data;
        }

        private byte[] sendChannelBytes = new byte[1];
        private void Write(byte[] data, int size, byte channel)
        {
            sendChannelBytes[0] = channel;
            stream.Write(sendChannelBytes, 0, 1);
            stream.Write(data, 0, size);
        }

        public void SendData(byte[] data, int size)
        {
            if(tcp != null)
            {
                stream.Write(data, 0, size);
            }
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