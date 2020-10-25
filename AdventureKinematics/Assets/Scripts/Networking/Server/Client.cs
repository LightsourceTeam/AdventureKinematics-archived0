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
    public class Client : IDisposable
    {
        //--------------------------------------------------
        #region VARIABLES
        /// public variables declaration



        // client identifier at server
        public int id;
        public bool isRunning { get; private set; } = false;
        public SafeParamLock usageLock = new SafeParamLock();

        public TcpClient tcp;
        public NetworkStream stream;

        public int clientId;



        #endregion
        //--------------------------------------------------
        #region INTERACTION METHODS
        /// these methods will be executed only here



        public void Connect(TcpClient tcp, int id)        // start server-side client management 
        {
            isRunning = true;

            // get tcp network stream
            this.tcp = tcp;
            clientId = id;
            stream = tcp.GetStream();


            // initialize client-sent methods
            RegisterInstructions(this);

            // signalize an event before client starts
            Start();
            onStart?.Invoke();

            // begin data reading
            stream.BeginRead(headerBuffer, 0, 6, OnHeaderReceive, null);
        }

        public void Disconnect()              // safely disconnect client
        {
            OnDisconnect();
            onDisconnect?.Invoke();


            tcp.Client.Shutdown(SocketShutdown.Both);
        }

        public void Send(Instructions instructionId, byte[] data = null, bool waitTillEnd = false)
        {
            try
            {
                byte[] dataToSend = Bytes.Combine(Bytes.ToBytes((short)instructionId), Bytes.ToBytes(data.Length), data);

                if (waitTillEnd) stream.Write(dataToSend, 0, dataToSend.Length);
                else stream.WriteAsync(dataToSend, 0, dataToSend.Length);
            }
            catch(IOException) { }
        }

        public static void RegisterInstructions(object objToRegisterInstructionsFor)    // bind instructions received from system channel to the instructions methods 
        {
            /*
            super puper large line which:
                1. gets non-public methods from *Client*, which are marked with *ServerSentAttribute*
                2. gets delegates to the gotten functions from their *MethodInfo*'s
                3. stores them to *sentInstructions*-dictionary, where keys are Ids of *ServerSentAttribute*, and values are delegates themselves
            */

            instructions = (from method in objToRegisterInstructionsFor.GetType().GetMethods(BindingFlags.NonPublic | BindingFlags.Instance)
                            where method.GetCustomAttribute(typeof(InstructionAttribute)) != null
                            select method.CreateDelegate(typeof(Action<byte[]>), objToRegisterInstructionsFor) as Action<byte[]>).ToDictionary(x => (x.GetMethodInfo().GetCustomAttribute(typeof(InstructionAttribute)) as InstructionAttribute).id);
            
            // instructions[-1](null); ; // debugging
        }

        public void ExecuteOne() 
        {
            KeyValuePair<short, byte[]> instruction;

            lock (instructionsLock) { if (instructionsToExecute.Count > 0) instruction = instructionsToExecute.Pop(); }

            instructions[instruction.Key]?.Invoke(instruction.Value);
        }

        public void ExecuteAll() 
        {

            KeyValuePair<short, byte[]> instruction;

            lock (instructionsLock)
            {
                while (instructionsToExecute.Count > 0)
                {
                    instruction = instructionsToExecute.Pop();
                    instructions[instruction.Key]?.Invoke(instruction.Value);
                }
            }

        }

        public void Dispose() 
        {
        }



        #endregion
        //--------------------------------------------------
        #region EVENTS


        public event Action onStart;
        public event Action onDisconnect;

        public void Start()
        {

        } 

        public void OnDisconnect()
        {

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
            OnDisconnect();
            onDisconnect?.Invoke();

            lock (usageLock) isRunning = false;
        }



        #endregion
        //--------------------------------------------------
        #region INTERNAL VARIABLES
        /// inaccessible variables, that are responsible for internal client data storing



        private static Dictionary<short, Action<byte[]>> instructions;        // dictionary of instructions


        // vars for supportiong instructions buffering
        private bool bufferInstructions = false;
        private Stack<KeyValuePair<short, byte[]>> instructionsToExecute = new Stack<KeyValuePair<short, byte[]>>();
        private object instructionsLock = new object();



        #endregion
        //--------------------------------------------------
        #region INTERNAL METHODS
        /// inaccessible methods, that are responsible for internal client functioning



        private void EndSession()
        {
            Logging.LogDeny("Client " + clientId + " on address " + tcp.Client.RemoteEndPoint + " ended it's session.");

            stream.Close();
            tcp.Close();

            instructions.Clear();
            instructionsToExecute.Clear();

            Server.server.RemoveClient(this);
        }

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
            catch (ClientDisconnectedException) { EndSession(); }
            catch (IOException) { EndSession(); }
            catch (Exception exc)
            {
                Logging.LogError(exc);
                Disconnect();
                EndSession();
            }
        }
        byte[] headerBuffer = new byte[6];
        short instructionId = -1;
        int dataSize = 0;


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
            catch (ClientDisconnectedException) { EndSession(); }
            catch (IOException) { EndSession(); }
            catch (Exception exc)
            {
                Logging.LogError(exc);
                Disconnect();
                EndSession();
            }
        }
        byte[] dataBuffer = null;
        int readBytesCount = 0;

        private void HandleInstruction()
        {
            lock (instructionsLock)
            {
                if (bufferInstructions)
                {
                    instructionsToExecute.Push(new KeyValuePair<short, byte[]>(instructionId, dataBuffer));
                }
                else 
                {
                    instructions[instructionId](dataBuffer);
                }
            }
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
