using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Net.Sockets;
using System.Net;
using System;
using SourceExtensions;
using System.IO;

namespace Networking
{
    public class TCPCore
    {
        // connection-necessary params
        public TcpClient client { get; private set; }
        public NetworkStream stream { get; private set; }
        public IPEndPoint endPoint { get; private set; }

        // callbacks
        public Action<short, byte[]> handleDataCallback { get; private set; }
        public Action finalDisconnectCallback { get; private set; }

        public TCPCore(IPEndPoint endPoint, Action<short, byte[]> handleDataCallback, Action finalDisconnectCallback)
        {
            client = new TcpClient();
            client.Connect(endPoint);
            stream = client.GetStream();
            this.endPoint = endPoint;

            this.handleDataCallback = handleDataCallback;
            this.finalDisconnectCallback = finalDisconnectCallback;
        }

        public TCPCore(TcpClient client, Action<short, byte[]> handleDataCallback, Action finalDisconnectCallback)
        {
            // get tcp network stream
            this.client = client;
            stream = client.GetStream();
            endPoint = (IPEndPoint)client.Client.RemoteEndPoint;

            this.handleDataCallback = handleDataCallback;
            this.finalDisconnectCallback = finalDisconnectCallback;
        }

        public void Open() { stream.BeginRead(headerBuffer, 0, 6, OnHeaderReceive, null); }

        public void Send(Instructions instructionId, byte[] data = null) // sends an instruction 
        {
            byte[] dataToSend;


            try
            {
                if (data != null && data.Length > 0) dataToSend = Bytes.Combine(Bytes.ToBytes((short)instructionId), Bytes.ToBytes(data.Length), data);
                else dataToSend = Bytes.Combine(Bytes.ToBytes((short)instructionId), Bytes.ToBytes(0));

                stream.WriteAsync(dataToSend, 0, dataToSend.Length);
            }
            catch (IOException) { Logging.LogError("Failed to send data!"); }
            catch (ObjectDisposedException) { Logging.LogError("Failed to send data! Socket is closed."); }
        }

        public void Shutdown(SocketShutdown how)
        {
            client.Client.Shutdown(how);
        }

        public void Close()
        {
            handleDataCallback = null;
            finalDisconnectCallback = null;

            stream.Close();
            client.Close();
        }



        private void OnHeaderReceive(IAsyncResult result)     // accepts data header, and processes it 
        {
            try
            {
                // read header bytes, get their count, and add them to the general read bytes sum
                int currentlyReadBytesCount = stream.EndRead(result);
                if (currentlyReadBytesCount == 0) throw new ClientDisconnectedException();
                readBytesCount += currentlyReadBytesCount;


                // if we've read too few bytes, repeat reading process, until we read all of the necessary bytes
                if (readBytesCount < 6)
                {
                    stream.BeginRead(headerBuffer, readBytesCount, (6 - readBytesCount), OnHeaderReceive, null);
                    return;
                }


                /// if we read enough bytes, reset bytes count to 0 (next time when we read, we'll need it to be 0) and decode all header data


                readBytesCount = 0;

                // decode and store header data
                instructionId = Bytes.ToShort(headerBuffer);
                dataSize = Bytes.ToInt(headerBuffer, 2);

                if (dataSize > 0)
                {
                    // create buffer for the incoming data
                    dataBuffer = new byte[dataSize];

                    // begin reading the incoming data
                    stream.BeginRead(dataBuffer, 0, dataSize, OnDataReceive, null);
                }
                else
                {
                    handleDataCallback?.Invoke(instructionId, null);

                    // begin reading the next packet, or disconnect, depending on the client stste
                    stream.BeginRead(headerBuffer, 0, 6, OnHeaderReceive, null);
                }
            }
            catch (ClientDisconnectedException) { finalDisconnectCallback?.Invoke(); }
            catch (IOException) { finalDisconnectCallback?.Invoke(); }
            catch (Exception exc)
            {
                Logging.LogError(exc);
                finalDisconnectCallback?.Invoke();
            }
        }
        byte[] headerBuffer = new byte[6];
        short instructionId = -1;
        int dataSize = 0;

        private void OnDataReceive(IAsyncResult result)    // accepts data itself, and handles it 
        {
            try
            {
                // read data bytes, and get their count
                int currentlyReadBytesCount = stream.EndRead(result);
                if (currentlyReadBytesCount == 0) throw new ClientDisconnectedException();
                readBytesCount += currentlyReadBytesCount;

                // if we read too few bytes, repeat reading process, until we read all of the necessary bytes
                if (readBytesCount < dataSize)
                {
                    stream.BeginRead(dataBuffer, readBytesCount, (dataSize - readBytesCount), OnDataReceive, null);
                    return;
                }


                /// if we read enough bytes, reset bytes count to 0 (next time when we read, we'll need it to be 0) and invoke received instruction


                readBytesCount = 0;

                // invoke instruction and the corresponding event
                handleDataCallback?.Invoke(instructionId, dataBuffer);


                // begin reading the next packet, or disconnect, depending on the client stste
                stream.BeginRead(headerBuffer, 0, 6, OnHeaderReceive, null);
            }
            catch (ClientDisconnectedException) { finalDisconnectCallback?.Invoke(); }
            catch (IOException) { finalDisconnectCallback?.Invoke(); }
            catch (Exception exc)
            {
                Logging.LogError(exc);
                finalDisconnectCallback?.Invoke();
            }
        }
        byte[] dataBuffer = null;
        int readBytesCount = 0;
    }
}
