using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Networking
{
    public class ClientDisconnectedException : Exception
    {
        public ClientDisconnectedException(string message) : base(message) { }
        public ClientDisconnectedException() : base() { }
    }
}
