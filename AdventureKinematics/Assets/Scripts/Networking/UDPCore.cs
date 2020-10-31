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
        public Action finalDisconnectCallback { get; private set; }



        #endregion
        //--------------------------------------------------
        #region INTERACTION



        public UDPCore(IPEndPoint endPoint, Action<short, byte[]> handleDataCallback = null, Action finalDisconnectCallback = null)
        {
            client = new UdpClient(endPoint);
            this.endPoint = (IPEndPoint)client.Client.RemoteEndPoint;


            this.handleDataCallback = handleDataCallback;
            this.finalDisconnectCallback = finalDisconnectCallback;

        }

        public UDPCore(UdpClient client, Action<short, byte[]> handleDataCallback = null, Action finalDisconnectCallback = null)
        {
            this.client = client;
            endPoint = (IPEndPoint)client.Client.RemoteEndPoint;

            this.handleDataCallback = handleDataCallback;
            this.finalDisconnectCallback = finalDisconnectCallback;
        }

        public void Send(Instructions instructionId, byte[] data = null) // sends an instruction 
        {
            byte[] dataToSend;

            try
            {
                if (data != null && data.Length > 0) dataToSend = Bytes.Combine(Bytes.ToBytes((short)instructionId), Bytes.ToBytes(data.Length), data);
                else dataToSend = Bytes.Combine(Bytes.ToBytes((short)instructionId), Bytes.ToBytes(0));

                client.BeginSend(dataToSend, dataToSend.Length, null, null);
            }
            catch (IOException) { Logging.LogError("Failed to send data!"); }
            catch (ObjectDisposedException) { Logging.LogError("Failed to send data! Socket is closed."); }
        }

        public void Open() => client.BeginReceive(OnDataReceive, null);

        public void Close()
        {
            handleDataCallback = null;
            finalDisconnectCallback = null;
        }



        #endregion
        //--------------------------------------------------
        #region INTERNAL



        private void OnDataReceive(IAsyncResult result)     // accepts data header, and processes it 
        {
            IPEndPoint endPoint = null;
            Bytes.Couple data = client.EndReceive(result, ref endPoint);

            if (!endPoint.Equals(this.endPoint) || data.data.Length < 6) return;

            short instructionId = data.GetShort();
            int dataSize = data.GetInt();

            if ((data.data.Length - 6) < dataSize) return;

            handleDataCallback?.Invoke(instructionId, data);
        }

        public void OnDataReceive(Bytes.Couple data)     // accepts data header, and processes it 
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
