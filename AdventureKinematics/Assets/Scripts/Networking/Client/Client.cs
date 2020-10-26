using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Net;
using System.Net.Sockets;
using System;
using System.Threading;
using SourceExtensions;
using System.IO;

namespace Networking.Client
{
    public class Client : CustomMonoBehaviour
    {
        //--------------------------------------------------
        #region VARIABLES
        /// variables declaration



        public static Client client = null;     // the main instance of the client

        // connection-necessary params
        public TcpClient tcp;
        public NetworkStream stream;

        public int clientId;                    // client unique connection identifier, TODO replace it with something not stupid



        #endregion
        //--------------------------------------------------
        #region INTERACTION METHODS
        /// these methods will be executed only here



        public void Connect()    //  initializes a connection with client 
        {
            // set static instance of the client
            if (client == null) client = this;
            else if (client != this) Logging.LogError(this + ": Failed to set active instance - it already is set to " + client);
            else Logging.LogWarning(this + ": No need to setup this as active object - it already is");

            Logging.LogInfo("Building connection to server...");

            // get tcp network stream
            tcp = new TcpClient();
            tcp.Connect(ip, port);
            stream = tcp.GetStream();


            Logging.LogAccept("Successfully connected. Starting data transfer protocol...");

            
            // begin data reading
            stream.BeginRead(headerBuffer, 0, 6, OnDataReceive, null);
        }

        public void Disconnect()              // safely disconnects client 
        {
            // invoke event before disconnecting
            try { CustomEventSystem.InvokeBeforeDisconnect(); } catch { }

            // send instruction to server, and shutdown sending
            try
            { 
                Send(Instructions.Disconnect);
                tcp.Client.Shutdown(SocketShutdown.Send);
            }
            catch (ObjectDisposedException) { Logging.LogError("Disconnecting was turned down: socket is already closed."); }
            catch (SocketException sockExc) { Logging.LogError("Socket exception occured: " + sockExc); }

        }

        public void Send(Instructions instructionId, byte[] data = null, bool waitTillEnd = false)  // sends an instruction 
        {
            byte[] dataToSend;

            try
            {
                if (data != null && data.Length > 0) dataToSend = Bytes.Combine(Bytes.ToBytes((short)instructionId), Bytes.ToBytes(data.Length), data);
                else dataToSend = Bytes.Combine(Bytes.ToBytes((short)instructionId), Bytes.ToBytes(0));

                if (waitTillEnd) stream.Write(dataToSend, 0, dataToSend.Length);
                else stream.WriteAsync(dataToSend, 0, dataToSend.Length);
            }
            catch (IOException) { Logging.LogError("Failed to send data!"); }
        }

        public void RegisterInstructions(object objToRegisterInstructionsFor)     // registers instructions for the specified class instance 
        {
            /*
            super puper large line which:
                1. gets non-public methods from *Client*, which are marked with *ServerSentAttribute*
                2. gets delegates to the gotten functions from their *MethodInfo*'s
                3. stores them to *sentInstructions*-dictionary, where keys are Ids of *ServerSentAttribute*, and values are delegates themselves
            */

            instructions = (from method in objToRegisterInstructionsFor.GetType().GetMethods(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance)
                            where method.GetCustomAttribute(typeof(InstructionAttribute)) != null
                            select method.CreateDelegate(typeof(Action<byte[]>), objToRegisterInstructionsFor) as Action<byte[]>).ToDictionary(x => (x.GetMethodInfo().GetCustomAttribute(typeof(InstructionAttribute)) as InstructionAttribute).id);

            //instructions[-1](Converter.ToBytes("Hello World!")); ; // debugging
        }

        public bool ExecuteOne()    // execute one instruction from the buffered ones 
        {
            Tuple<short, byte[]> instruction;

            try
            {
                lock (instructionsLock) { if (instructionsToExecute.Count == 0) return false; instruction = instructionsToExecute.Pop(); }

                instructions[instruction.Item1]?.Invoke(instruction.Item2);

                return true;
            }
            catch (KeyNotFoundException) { Logging.LogError("Instruction with id " + instructionId + " does not exist or isn't declared properly!"); }
            return false;
        }

        public void ExecuteAll()    // execute all the buffered instructions 
        {

            Tuple<short, byte[]> instruction;

            try
            {
                lock (instructionsLock)
                {
                    while (instructionsToExecute.Count > 0)
                    {
                        instruction = instructionsToExecute.Pop();
                        instructions[instruction.Item1]?.Invoke(instruction.Item2);
                    }
                }
            }
            catch (KeyNotFoundException) { Logging.LogError("Instruction with id " + instructionId + " does not exist or isn't declared properly!"); }
        }



        #endregion
        //--------------------------------------------------
        #region EVENTS

        protected void Awake()
        {
            // initialize server-sent methods
            RegisterInstructions(this);
        }

        protected void Start()
        {
            Connect();

            byte[] data = Bytes.Combine(Bytes.ToBytes("Hello you silly!"), Bytes.ToBytes("Inneccessary message!"));
            Send(Instructions.HelloWorld, data);
        }

        protected override void NetworkUpdate()
        {
            ExecuteOne();
        }

        protected override void BeforeDisconnect()
        {
        }

        protected void OnApplicationQuit()
        {
            Disconnect();
        }


        private void InTheEnd()
        {
            Logging.LogDeny("Client " + clientId + " on address " + tcp.Client.RemoteEndPoint + " ended it's session.");

            client = null;
            instructions.Clear();

            stream.Close();
            tcp.Close();
        }



        #endregion
        //--------------------------------------------------
        #region SERVER-SENT INSTRUCTIONS
        /// instructions coming from server



        [Instruction(Instructions.HelloWorld)]
        private void instHelloWorld(byte[] data)       // testing function 
        {
            if (data != null && data.Length > 0) Logging.LogWarning("Hello from the server-side! data: " + Bytes.ToString(data));
            else Logging.LogWarning("Hello from the server-side! No data was given...");
        }


        [Instruction(Instructions.Connect)]
        private void instConnect(byte[] data)        // connect 
        {

        }


        [Instruction(Instructions.Disconnect)]
        private void instDisconnect(byte[] data)        // disconnect 
        {
            CustomEventSystem.InvokeBeforeDisconnect();

            Send(Instructions.Disconnect);
            tcp.Client.Shutdown(SocketShutdown.Both);
        }


        #endregion
        //--------------------------------------------------
        #region INTERNAL VARIABLES
        /// inaccessible variables, that are responsible for internal client data storing



        private Dictionary<short, Action<byte[]>> instructions;        // dictionary of instructions
     

        // connection address
        private IPAddress ip = IPAddress.Parse("127.0.0.1");
        private int port = 23852;


        // vars for supportiong instructions buffering
        private bool bufferInstructions = true;
        private Stack<Tuple<short, byte[]>> instructionsToExecute = new Stack<Tuple<short, byte[]>>();
        private object instructionsLock = new object();



        #endregion
        //--------------------------------------------------
        #region INTERNAL METHODS
        /// inaccessible methods, that are responsible for internal client functioning



        // accepts data header
        private void OnHeaderReceive(IAsyncResult result)     // gets invoked every time data is available, and processes it 
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
                    dataBuffer = null;
                    HandleInstruction();

                    // begin reading the next packet, or disconnect, depending on the client stste
                    stream.BeginRead(headerBuffer, 0, 6, OnHeaderReceive, null);
                }
            }
            catch (ClientDisconnectedException) { InTheEnd(); }
            catch (IOException) { InTheEnd(); }
            catch (Exception exc)
            {
                Logging.LogError(exc);
                Disconnect();
                InTheEnd();
            }
        }
        byte[] headerBuffer = new byte[6];
        short instructionId = -1;
        int dataSize = 0;

        // accepts data itself, and handles it
        private void OnDataReceive(IAsyncResult result)
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
                HandleInstruction();


                // begin reading the next packet, or disconnect, depending on the client stste
                stream.BeginRead(headerBuffer, 0, 6, OnHeaderReceive, null);
            }
            catch (ClientDisconnectedException) { InTheEnd(); }
            catch (IOException) { InTheEnd(); }
            catch (Exception exc)
            {
                Logging.LogError(exc);
                Disconnect();
                InTheEnd();
            }
        }
        byte[] dataBuffer = null;
        int readBytesCount = 0;

        // specifies the metods of handling received data
        private void HandleInstruction()
        {
            try
            {
                lock (instructionsLock)
                {
                    if (bufferInstructions)
                    {
                        instructionsToExecute.Push(new Tuple<short, byte[]>(instructionId, dataBuffer));
                    }
                    else
                    {
                        instructions[instructionId](dataBuffer);
                    }
                }
            }
            catch (KeyNotFoundException) { Logging.LogError("Instruction with id " + instructionId + " does not exist or isn't declared properly!"); }

        }


        #endregion
        //--------------------------------------------------
        #region CLASSES
        /// classes, used by client internally



        [AttributeUsage(AttributeTargets.Method)]
        public class InstructionAttribute : Attribute
        {
            public short id;

            public InstructionAttribute(Instructions instructionId)
            {
                id = (short)instructionId;
            }
        }



        #endregion
        //--------------------------------------------------
}



}
