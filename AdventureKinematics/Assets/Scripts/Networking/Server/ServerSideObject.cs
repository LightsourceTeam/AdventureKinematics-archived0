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
