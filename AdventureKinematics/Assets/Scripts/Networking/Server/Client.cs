using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Net.Sockets;
using System;
using System.Threading;

namespace Server
{
    public class Client
    {
        //--------------------------------------------------
        #region VARIABLES
        /// public variables declaration
        


        // state of the client
        public bool isAlive { get; private set; } = false;


        // client identifier at server
        public int id;

        public TcpClient tcp;
        public NetworkStream stream;




        #endregion
        //--------------------------------------------------
        #region LOCAL METHODS
        /// these methods will be executed only here



        public Channel this[byte i]         // operator for accessing channels 
        { get { return (channels != null) ? channels[i] : null; } }

        public void Connect(TcpClient client, int clientId)        // start server-side client management 
        {
            // get tcp network stream
            tcp = client;
            stream = tcp.GetStream();
            id = clientId;

            // add default system channel
            channels.Add(0, new Channel(this, 0));

            // state that client is alive now;
            isAlive = true;

            // initialize client-sent methods
            RegisterInstructionsDelegates();

            // begin data reading
            stream.BeginRead(receiveTempBuffer, 0, 1, OnDataReceive, null);

        }

        public void ClearAllBuffers()        // clear all buffered messages in all channels except system channel 
        {
            foreach (KeyValuePair<byte, Channel> channel in channels)
            {
                channel.Value?.ClearBuffer();
            }
        }




        #endregion
        //--------------------------------------------------
        #region GLOBAL METHODS
        /// these methods will execute both here, and on client



        public void Disconnect()              // safely disconnect client 
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


        public Channel AcquireChannel()                // acquire channel 
        {
            for (byte i = 1; i <= 255; i++)
            {
                if (!channels.ContainsKey(i))
                {
                    Channel newChannel = new Channel(this, i);
                    channels.Add(i, newChannel);
                    return newChannel;
                }
            }

            return null;
        }


        public bool ReleaseChannel(byte channelId)     // release channel 
        {
            return channels.Remove(channelId);
        }




        #endregion
        //--------------------------------------------------
        #region CLIENT-SENT INSTRUCTIONS
        /// instructions coming from client



        [ClientInstruction(-1)]
        private void instHelloWorld(byte[] data)       // testing function
        { Logging.LogWarning("Hello from the client-side!"); }


        [ClientInstruction(0)]
        private void instAcquireChannel(byte[] data)  // acquire channel 
        {
            byte channelId = data[2];
        }


        [ClientInstruction(1)]
        private void instReleaseChannel(byte[] data)  // release channel 
        {  }



        #endregion
        //--------------------------------------------------
        #region INTERNAL VARIABLES
        /// inaccessible variables, that are responsible for internal client data storing



        private Dictionary<byte, Channel> channels = new Dictionary<byte, Channel>();         // communication channels
        private Channel systemChannel;                                                        // system channel

        private Dictionary<short, Action<byte[]>> sentInstructions;                     // dictionary of sent instructions



        #endregion
        //--------------------------------------------------
        #region INTERNAL METHODS
        /// inaccessible methods, that are responsible for internal client functioning


        
        private void RegisterInstructionsDelegates()    // bind instructions received from system channel to the instructions methods 
        {
            /*
            super puper large line which:
                1. gets non-public methods from *Client*, which are marked with *ClientSentAttribute*
                2. gets delegates to the gotten functions from their *MethodInfo*'s
                3. stores them to *sentInstructions*-dictionary, where keys are from *callbackId* of *ClientSentAttribute*, and values are delegates themselves
            */
            sentInstructions = GetType().GetMethods(BindingFlags.NonPublic | BindingFlags.Instance).Where(m => m.GetCustomAttribute(typeof(ClientInstructionAttribute)) != null).Select(x => (x.CreateDelegate(typeof(Action<byte[]>), this) as Action<byte[]>)).ToDictionary(x => (x.GetMethodInfo().GetCustomAttribute(typeof(ClientInstructionAttribute)) as ClientInstructionAttribute).id);


            sentInstructions[-1](null); // debugging
        }


        private void OnDataReceive(IAsyncResult result)     // function that gets invoked every time data comes to you
        {
            // get channel to which we are reading
            stream.EndRead(result);

            // get channel to which we are reading
            byte channel = receiveTempBuffer[0];
            Channel currentChannel = null;
            lock (this) currentChannel = channels[channel];
            
            // perform data receiving and buffering
            lock(currentChannel)
            {
                stream.Read(receiveTempBuffer, 0, 4);
                int dataSize = Converter.ToInt(receiveTempBuffer);

                byte[] data = new byte[dataSize];
                stream.Read(data, 0, dataSize);

                currentChannel.dataBuffer.Push(data);
            }


            // start waiting for the next data
            if(isAlive) stream.BeginRead(receiveTempBuffer, 0, 1, OnDataReceive, null);
        }
        private byte[] receiveTempBuffer = new byte[4];




        #endregion
        //--------------------------------------------------
        #region CLASSES
        /// classes, used by client internally



        public class Channel
        {
            public Channel(Client client, byte identifier) { this.client = client; id = identifier; }


            public Stack<byte[]> dataBuffer = new Stack<byte[]>();
            protected Client client;
            public byte id = 0;



            ///     INTERACTION METHODS     ///



            // clear all buffered messages in this channel
            public virtual void ClearBuffer() { lock (this) dataBuffer.Clear(); }


            // send data through this channel
            public void Send(byte[] data)
            {
                lock (this)
                {
                    sendTempBuffer[0] = id;
                    client.stream.Write(sendTempBuffer, 0, 1);
                    client.stream.Write(Converter.ToBytes(data.Length), 0, 4);
                    client.stream.Write(data, 0, data.Length);
                }
            }
            byte[] sendTempBuffer = new byte[1];


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
        }


        [AttributeUsage(AttributeTargets.Method)]
        public class ClientInstructionAttribute : Attribute
        {
            public short id;

            public ClientInstructionAttribute(short instructionId)
            {
                id = instructionId;
            }
        }




        #endregion
        //--------------------------------------------------
    }


}
