using System.Collections.Generic;
using System.Net;

namespace PartY
{
    public static class PartY_Globals
    {
        public const int port = 27015;
        public static bool isServer;
    }

    public class Client
    {
        string username { get; set; }
    }

    public class HostedLobby
    {
        public string hostsName { get; set; }
    }
}