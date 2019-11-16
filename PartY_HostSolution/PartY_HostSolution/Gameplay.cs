using System;
using System.Collections.Generic;
using System.Text;
using static PartY_HostSolution.Types;

namespace PartY
{
    class Gameplay
    {
        public class NetworkedObject
        {
            public Vector3 position;
            public Quaternion rotation;
            public Vector3 scale;
        }

        public class NetworkedPlayer: NetworkedObject
        {
            public int id;
            public float health = 100;
        }

        public class Lobby
        {
            //All dynamic objects
            public List<NetworkedObject> objects = new List<NetworkedObject>();

            //All players
            public List<NetworkedPlayer> players = new List<NetworkedPlayer>();

            public void MovePlayer(int id, Vector3 position)
            {
                for (int i = 0; i < players.Count; i++)
                {
                    if(players[i].id == id)
                    {
                        players[i].position = position;
                        break;
                    }
                }
            }

            public void RotatePlayer(int id, Quaternion rotation)
            {
                for (int i = 0; i < players.Count; i++)
                {
                    if (players[i].id == id)
                    {
                        players[i].rotation = rotation;
                        break;
                    }
                }
            }

            public void ScalePlayer(int id, Vector3 scale)
            {
                for (int i = 0; i < players.Count; i++)
                {
                    if (players[i].id == id)
                    {
                        players[i].scale = scale;
                        break;
                    }
                }
            }
        }
    }
}
