using System;
using System.Collections.Generic;
using System.Text;
using System.Net;

namespace PartY
{
    class Matchmaking
    {
        public static List<MatchmakingLobby> openLobbies = new List<MatchmakingLobby>();

        public class UserProfile
        {
            public ulong id;
            public string username;
            public IPAddress address;

            public UserProfile()
            {

            }

            public UserProfile(ulong signInID, string usersname)
            {
                id = signInID;
                username = usersname;
            }

            public UserProfile(ulong signInID, string usersname, IPAddress ipaddress)
            {
                id = signInID;
                username = usersname;
                address = ipaddress;
            }
        }

        public class MatchmakingLobby
        {
            public List<UserProfile> usersInLobby = new List<UserProfile>();
        }
    }
}