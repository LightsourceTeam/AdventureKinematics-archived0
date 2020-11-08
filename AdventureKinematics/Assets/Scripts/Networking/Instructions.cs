using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Networking
{
    public enum Instructions
    {
        HelloWorld=-1,      // testing function

        // client instructions 

        Connect,            // connection
        Disconnect,         // disconnection
        ForceDisconnect,
        RequestUdp,         // gets called when server requests Udp
        NotifyShutdown,

        // passive-gaming instructions

        GetAccountInfo,     // information about player, his name, levels, etc.
        GetPrefs,
        GetFriends,
        GetOwnedBps,        // gets list of the owned blueprints
        GetPreferences,
        CheckForUpdates,



    }
}
