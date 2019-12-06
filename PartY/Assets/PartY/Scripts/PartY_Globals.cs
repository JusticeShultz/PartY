using System.Collections.Generic;
using System.Net;
using UnityEngine;

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

    public class Player
    {
        public string username;
        public Vector3 position { get; set; } = Vector3.zero;
        public Vector3 rotation { get; set; } = Vector3.zero;
        public Vector3 scale { get; set; } = Vector3.one;

        public Player()
        {

        }

        public Player(string uName, Vector3 uPos, Vector3 uRot, Vector3 uScale)
        {
            username = uName;
            position = uPos;
            rotation = uRot;
            scale = uScale;
        }
    }

    public class HostedLobby
    {
        public string hostsName { get; set; }
    }
}