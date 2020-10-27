using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Net.Sockets;
using System;
using SourceExtensions;
using System.IO;

namespace Networking.Server
{
    public class Client : ServerSideObject, IDisposable
    {
        //--------------------------------------------------
        #region VARIABLES
        /// public variables declaration



        // connection-necessary params
        public TcpClient tcp;
        public NetworkStream stream;

        public int clientId;                // client unique connection identifier, TODO replace it with something not stupid



        #endregion
        //--------------------------------------------------
        #region INTERACTION METHODS
        /// these methods will be executed only here



        public Client()
        {
            // initialize client-sent methods
            RegisterInstructions(this);

            client = this;
        }

        public void Connect(TcpClient tcp, int id)        //  initializes a connection with client 
        {

            // get tcp network stream
            this.tcp = tcp;
            clientId = id;
            stream = tcp.GetStream();


            // begin data reading
            stream.BeginRead(headerBuffer, 0, 6, OnHeaderReceive, null);
        }

        public void Disconnect()        // safely disconnects client 
        {
            // notify client about disconnection
            try
            {
                Send(Instructions.Disconnect);
                tcp.Client.Shutdown(SocketShutdown.Send);
            }
            catch (ObjectDisposedException) { Logging.LogError(clientId + ": Disconnecting was turned down: socket is already closed."); }
            catch (SocketException sockExc) { Logging.LogError(clientId + ": Socket exception occured: " + sockExc); }
        }

        public void ForceDisconnect()   // without notifying it about such an intention, forcibly disconnects client 
        {
            // invoke event before disconnecting
            try
            {
                onBeforeForceDisconnect?.Invoke();
                BeforeForceDisconnect();
            }
            catch { }

            // force-disconnect client
            try
            {
                tcp.Client.Shutdown(SocketShutdown.Both);
            }
            catch (ObjectDisposedException) { Logging.LogError(tcp.Client.RemoteEndPoint + ": Sending is turned down: socket was closed."); }
            catch (SocketException sockExc) { Logging.LogError(tcp.Client.RemoteEndPoint + ": Socket exception occured: " + sockExc); }
        }

        public void Send(Instructions instructionId, byte[] data = null, bool waitTillEnd = false) // sends an instruction 
        {
            try
            {
                byte[] dataToSend = Bytes.Combine(Bytes.ToBytes((short)instructionId), Bytes.ToBytes(data.Length), data);

                if (waitTillEnd) stream.Write(dataToSend, 0, dataToSend.Length);
                else stream.WriteAsync(dataToSend, 0, dataToSend.Length);
            }
            catch (IOException) { Logging.LogError("Failed to send data!"); }
        }

        public bool RegisterInstructions(object objToRegisterInstructionsFor)    // registers instructions for the specified class instance 
        {
            /*
            super puper large line which:
                1. gets non-public methods from *Client*, which are marked with *ServerSentAttribute*
                2. gets delegates to the gotten functions from their *MethodInfo*'s
                3. stores them to *sentInstructions*-dictionary, where keys are Ids of *ServerSentAttribute*, and values are delegates themselves
            */

            if (!registeredInstructionClasses.ContainsKey(objToRegisterInstructionsFor.GetType())) registeredInstructionClasses[objToRegisterInstructionsFor.GetType()] = objToRegisterInstructionsFor;
            else return false;

            return true;

            // instructions[-1](null); ; // debugging
        }

        public bool ExecuteOne()    // execute one instruction from the buffered ones 
        {
            Tuple<short, byte[]> instructionData;

            try
            {
                lock (instructionsLock) { if (instructionsToExecute.Count == 0) return false;  instructionData = instructionsToExecute.Pop(); }

                MethodInfo instruction = Server.instructions[instructionData.Item1];
                instruction.Invoke(registeredInstructionClasses[instruction.DeclaringType], new object[] { instructionData.Item2 });
                return true;
            }
            catch (KeyNotFoundException) { Logging.LogError("Instruction with id " + instructionId + " does not exist or isn't declared properly!"); }
            return false;
        }

        public void ExecuteAll()    // execute all the buffered instructions 
        {

            Tuple<short, byte[]> instructionData;

            lock (instructionsLock)
            {
                while (instructionsToExecute.Count > 0)
                {
                    instructionData = instructionsToExecute.Pop();
                    MethodInfo instruction = Server.instructions[instructionData.Item1];
                    instruction.Invoke(registeredInstructionClasses[instruction.DeclaringType], new object[] { instructionData.Item2 });
                }
            }

        }

        public void Dispose() 
        {
            Logging.Log("How's that!");
        }



        #endregion
        //--------------------------------------------------
        #region EVENTS



        public event Action onBeforeDisconnect;
        public event Action onBeforeForceDisconnect;

        public void Start()
        {

        } 

        protected override void BeforeDisconnect()
        {
        }

        protected override void BeforeForceDisconnect()
        {
            registeredInstructionClasses.Clear();
        }

        public override void Delete()
        {
            Logging.LogDeny("Client " + clientId + " on address " + tcp.Client.RemoteEndPoint + " ended it's session.");

            registeredInstructionClasses.Clear();

            tcp.Client.Dispose();

            stream.Close();
            tcp.Close();

            stream = null;
            tcp = null;

            base.Delete();

            Server.server.RemoveClient(this);
        }



        #endregion
        //--------------------------------------------------
        #region CLIENT-SENT INSTRUCTIONS
        /// instructions coming from client



        [Instruction(Instructions.HelloWorld)]
        private void instHelloWorld(byte[] data)       // testing function 
        {   
            if(data != null && data.Length > 0) Logging.LogWarning("Hello from the client-side! data: " + Bytes.ToString(data));
            else Logging.LogWarning("Hello from the client-side! No data was given...");
        }


        [Instruction(Instructions.Connect)]
        private void instConnect(byte[] data)        // connect 
        {

        }


        [Instruction(Instructions.Disconnect)]
        private void instDisconnect(byte[] data)        // disconnect 
        {
            onBeforeDisconnect?.Invoke();
            BeforeDisconnect();

            tcp.Client.Shutdown(SocketShutdown.Both);
        }


        #endregion
        //--------------------------------------------------
        #region INTERNAL VARIABLES
        /// inaccessible variables, that are responsible for internal client data storing



        private Dictionary<Type, object> registeredInstructionClasses = new Dictionary<Type, object>();        // dictionary of the registered classes which contain instruction


        // vars for supportiong instructions buffering
        private bool bufferInstructions = false;
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
            catch (ClientDisconnectedException) { Delete(); }
            catch (IOException) { Delete(); }
            catch (Exception exc)
            {
                Logging.LogError(exc);
                Disconnect();
                Delete();
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
            catch (ClientDisconnectedException) { Delete(); }
            catch (IOException) { Delete(); }
            catch (Exception exc)
            {
                Logging.LogError(exc);
                Disconnect();
                Delete();
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
                        MethodInfo instruction = Server.instructions[instructionId];
                        instruction.Invoke(registeredInstructionClasses[instruction.DeclaringType], new object[] { dataBuffer });
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
