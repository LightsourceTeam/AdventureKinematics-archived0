using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Networking.Server
{
    public class ServerSideObject
    {
        //--------------------------------------------------
        #region VARIABLES



        protected Client client;



        #endregion
        //--------------------------------------------------
        #region INTERNAL AND INTERACTION METHODS



        protected ServerSideObject() { }

        protected void Initialize(Client client)
        {
            this.client = client;

            client.onBeforeDisconnect += BeforeDisconnect;
            client.onBeforeForceDisconnect += BeforeForceDisconnect;
            client.onStart += Start;
        }

        public T Instantiate<T>() where T : ServerSideObject, new() // 
        {
            T newInstance = new T();
            newInstance.Initialize(client);

            return newInstance;
        }

        public virtual void Delete()
        {
            client = null;

            client.onBeforeDisconnect -= BeforeDisconnect;
            client.onBeforeForceDisconnect -= BeforeForceDisconnect;
            client.onStart -= Start; 
        }



        #endregion
        //--------------------------------------------------
        #region EVENTS



        protected virtual void Start() { }

        protected virtual void BeforeDisconnect() { }

        protected virtual void BeforeForceDisconnect() { }



        #endregion
        //--------------------------------------------------
    }
}
