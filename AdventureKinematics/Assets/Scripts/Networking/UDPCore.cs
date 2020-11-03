using System.Net.Sockets;
using System.Net;
using System;
using SourceExtensions;
using System.IO;
using System.Linq;


namespace Networking
{
    public class UDPCore
    {
        //--------------------------------------------------
        #region VARIABLES



        // connection-necessary params
        public UdpClient client { get; private set; }
        public IPEndPoint endPoint { get; private set; }

        // callbacks
        public Action<short, byte[]> handleDataCallback { get; private set; }
        public bool isConnected;


        #endregion
        //--------------------------------------------------
        #region INTERACTION



        public UDPCore(Action<short, byte[]> handleDataCallback = null)
        {
            client = new UdpClient();
            isConnected = true;

            this.handleDataCallback = handleDataCallback;
        }

        public UDPCore(UdpClient client, Action<short, byte[]> handleDataCallback = null)
        {
            this.client = client;
            isConnected = false;

            this.handleDataCallback = handleDataCallback;
        }

        public void Send(Instructions instructionId, byte[] data = null, AsyncCallback endCallback = null) // sends an instruction 
        {
            byte[] dataToSend;

            try
            {
                if (data != null && data.Length > 0) dataToSend = Bytes.Combine(Bytes.ToBytes((short)instructionId), Bytes.ToBytes(data.Length), data);
                else dataToSend = Bytes.Combine(Bytes.ToBytes((short)instructionId), Bytes.ToBytes(0));

                if (isConnected) client.BeginSend(dataToSend, dataToSend.Length, endCallback, null); 
                else client.BeginSend(dataToSend, dataToSend.Length, endPoint, endCallback, null);
            }
            catch (IOException) { Logging.LogError("Failed to send data!"); }
            catch (ObjectDisposedException) { Logging.LogError("Failed to send data! Socket is closed."); }
        }

        public void Open(IPEndPoint remoteEndPoint) 
        {
            endPoint = remoteEndPoint;
            if (isConnected)
            {
                client.Connect(remoteEndPoint);
                client.BeginReceive(OnDataReceive, null);
            }
        }

        public void Shutdown(SocketShutdown how)
        {
            client.Client.Shutdown(how);
        }

        public void Close()
        {
            handleDataCallback = null;
            client.Close();
        }



        #endregion
        //--------------------------------------------------
        #region INTERNAL



        private void OnDataReceive(IAsyncResult result)     // client version. accepts data, and processes it 
        {
            try
            {
                IPEndPoint localEndPoint = null;
                Bytes.Couple data = client.EndReceive(result, ref localEndPoint);

                if (!localEndPoint.Equals((IPEndPoint)client.Client.RemoteEndPoint) || data.data.Length < 6) return;

                short instructionId = data.GetShort();
                int dataSize = data.GetInt();

                if ((data.data.Length - 6) < dataSize) return;


                handleDataCallback.Invoke(instructionId, data);


                client.BeginReceive(OnDataReceive, null);
            }
            catch (ObjectDisposedException)
            {
                Close();
                return; // Connection closed
            }
        }

        public void OnDataReceive(Bytes.Couple data)     // server version. accepts data, and processes it 
        {
            if (data.data.Length < 6) return;


            short instructionId = data.GetShort();
            int dataSize = data.GetInt();

            if ((data.data.Length - 6) < dataSize) return;

            handleDataCallback?.Invoke(instructionId, data);
        }



        #endregion
        //--------------------------------------------------

    }
}
