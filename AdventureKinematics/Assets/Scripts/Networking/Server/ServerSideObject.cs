using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using static SourceExtensions.UnlockObject;

namespace Networking.Server
{
    public class ServerSideObject
    {
        //--------------------------------------------------
        #region VARIABLES



        protected Client client;

        public static Dictionary<short, MethodInfo> instructions { get; private set; }  // contains all the methods marked with Instruction attribute



        #endregion
        //--------------------------------------------------
        #region INTERNAL AND INTERACTION



        protected ServerSideObject() { }

        protected void Initialize(Client client)
        {
            this.client = client;

            client.onBeforeConnect += BeforeConenct;
            client.onAfterConnect += AfterConnect;
            client.onAfterUdpInvolved += AfterUdpInvolved;
            client.onBeforeDisconnect += BeforeDisconnect;
            client.onBeforeForceDisconnect += BeforeForceDisconnect;

            Awake();
        }

        public T Instantiate<T>() where T : ServerSideObject, new() // 
        {
            T newInstance = new T();
            newInstance.Initialize(client);

            return newInstance;
        }

        public virtual void Delete()
        {

            client.onBeforeConnect -= BeforeConenct;
            client.onAfterConnect -= AfterConnect;
            client.onAfterUdpInvolved -= AfterUdpInvolved;
            client.onBeforeDisconnect -= BeforeDisconnect;
            client.onBeforeForceDisconnect -= BeforeForceDisconnect;
        }

        public static void RegisterAllInstructions()    // makes all instructions ready for usage 
        {
            if (instructions == null)
                instructions = (from type in Assembly.GetExecutingAssembly().GetTypes()
                                from method in type.GetMethods(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance)
                                where method.GetCustomAttribute(typeof(Client.InstructionAttribute)) != null
                                select method).ToDictionary(x => (x.GetCustomAttribute(typeof(Client.InstructionAttribute)) as Client.InstructionAttribute).id);
        }



        #endregion
        //--------------------------------------------------
        #region EVENTS



        public virtual void Awake() { }

        protected virtual void BeforeConenct() { }

        protected virtual void AfterConnect() { }

        protected virtual void AfterUdpInvolved() { }

        protected virtual void BeforeDisconnect() { }

        protected virtual void BeforeForceDisconnect() { }



        #endregion
        //--------------------------------------------------
    }
}
