using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Networking.Server
{
    public class ServerSideObject
    {
        protected Client client;

        public ServerSideObject(Client client) 
        {
            this.client = client;

            client.onBeforeDisconnect += BeforeDisconnect;
            client.onBeforeForceDisconnect -= BeforeForceDisconnect;
        } 

        public void Delete()
        {
            client.onBeforeDisconnect -= BeforeDisconnect;
            client.onBeforeForceDisconnect -= BeforeForceDisconnect;
            client = null;
        }

        protected ServerSideObject() { }

        protected virtual void BeforeDisconnect() { }

        protected virtual void BeforeForceDisconnect() { }
    }
}
