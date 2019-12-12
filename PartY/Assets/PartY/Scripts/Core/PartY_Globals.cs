using System.Collections.Generic;
using System.Net;
using UnityEngine;

namespace PartY
{
    /// <summary>
    /// Global values used within PartY
    /// </summary>
    public static class PartY_Globals
    {
        public const int port = 27015; //The port number we will use to connect to the server.
        public static bool isServer; //If this is the server or not. [Unused, use if you migrate server into Unity]
    }

    /// <summary>
    /// The representation of a client connected to a lobby.
    /// </summary>
    public class Client
    {
        string username { get; set; } //The username of said client.
    }

    /// <summary>
    /// The representation of a player within game space.
    /// </summary>
    public class Player
    {
        public string username; //The username of the player client.
        public Vector3 position { get; set; } = Vector3.zero; //The position of the player client.
        public Vector3 rotation { get; set; } = Vector3.zero; //The rotation of the player client.
        public Vector3 scale { get; set; } = Vector3.one; //The scale of the player client.

        /// <summary>
        /// Default constructor of a player. Will generate a completely empty player.
        /// </summary>
        public Player()
        {

        }

        /// <summary>
        /// Full constructor of a player representation.
        /// </summary>
        /// <param name="uName"></param>
        /// <param name="uPos"></param>
        /// <param name="uRot"></param>
        /// <param name="uScale"></param>
        public Player(string uName, Vector3 uPos, Vector3 uRot, Vector3 uScale)
        {
            username = uName;
            position = uPos;
            rotation = uRot;
            scale = uScale;
        }
    }

    /// <summary>
    /// The representation of a hosted lobby.
    /// </summary>
    public class HostedLobby
    {
        public string hostsName { get; set; } //The name of the host.
    }
}