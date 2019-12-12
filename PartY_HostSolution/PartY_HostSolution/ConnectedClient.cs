using System;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Runtime.Serialization;
using System.Net;

namespace PartY
{
    public class PartY_ConnectedClient
    {
        #region Data
        /// <summary>
        /// For Clients, the connection to the server.
        /// For Servers, the connection to a client.
        /// </summary>

        ulong userID;
        
        readonly TcpClient connection;

        readonly byte[] readBuffer = new byte[5000];

        NetworkStream stream
        {
            get
            {
                try
                {
                    return connection.GetStream();
                }
                catch (System.InvalidOperationException)
                {
                    return null;
                }
            }
        }
        #endregion

        #region Init
        public PartY_ConnectedClient(TcpClient tcpClient)
        {
            this.connection = tcpClient;
            this.connection.NoDelay = true; // Disable Nagle's cache algorithm

            try
            {
                stream.BeginRead(readBuffer, 0, readBuffer.Length, OnRead, null);
            }
            catch (Exception ex)
            {
                if (ex is System.IO.IOException || ex is System.NullReferenceException)
                    return;
            }
        }

        internal void Close()
        {
            connection.Close();
        }
        #endregion

        #region Async Events
        void OnRead(IAsyncResult ar)
        {
            int length = 0;

            try
            {
                length = stream.EndRead(ar);
            }
            catch (Exception ex)
            {
                if (ex is System.IO.IOException || ex is System.NullReferenceException)
                    return;
            }

            if (length <= 0)
            { // Connection closed
                Program.instance.OnDisconnect(this);
                return;
            }

            string newMessage = System.Text.Encoding.UTF8.GetString(readBuffer, 0, length);

            try
            {
                stream.BeginRead(readBuffer, 0, readBuffer.Length, OnRead, null);
            }
            catch (Exception ex)
            {
                if (ex is System.IO.IOException || ex is System.NullReferenceException)
                    return;
            }

            //Console.WriteLine(newMessage);

            if (newMessage.Contains(","))
            {
                string[] parser = newMessage.Split(",");

                if (parser.Length > 0)
                {
                    if (parser[0] == "NeedTokenID")
                    {
                        SendAPIKey();
                    }
                    else if (parser[0] == "Verify")
                    {
                        ulong key;
                        bool parsed = ulong.TryParse(parser[1], out key);

                        if (!parsed)
                        {
                            Console.WriteLine("Error parsing key, could not verify");
                        }

                        bool FoundKey = false;

                        for (int i = 0; i < Program.existingKeys.Count; i++)
                        {
                            if (Program.existingKeys[i] == key)
                            {
                                FoundKey = true;
                                byte[] buffer = System.Text.Encoding.UTF8.GetBytes("Verified,Welcome");
                                stream.Write(buffer, 0, buffer.Length);
                                Console.WriteLine("Verified user " + key + ". Sent welcome! Handshake complete :)");
                                userID = key;
                                break;
                            }
                        }

                        if (!FoundKey)
                        {
                            Console.WriteLine("Invalid key, resending validation");
                            SendAPIKey();
                        }
                    }
                    else if (parser[0] == "CrLobby")
                    {
                        Program.instance.ReadTokens();
                        Console.WriteLine(parser[1] + " initialized a new lobby.");

                        PartY_ConnectedClient clientConnection = null;

                        ulong key;
                        bool parsed = ulong.TryParse(parser[2], out key);

                        if (parsed)
                        {
                            for (int i = 0; i < Program.instance.clientList.Count; i++)
                            {
                                if (Program.instance.clientList[i].userID == key)
                                {
                                    clientConnection = Program.instance.clientList[i];
                                    break;
                                }
                            }
                        }

                        if (clientConnection == null)
                        {
                            return;
                        }

                        for (int i = 0; i < Program.instance.lobbyList.Count; i++)
                        {
                            if (Program.instance.lobbyList[i].host.username == parser[1])
                            {
                                byte[] buffer = System.Text.Encoding.UTF8.GetBytes("DuplicateUsername");
                                stream.Write(buffer, 0, buffer.Length);

                                return;
                            }
                        }

                        //Tell everyone there is a PartY! :D
                        for (int i = 0; i < Program.instance.clientList.Count; i++)
                        {
                            Program.instance.clientList[i].Send("NewLobby," + parser[1]);
                        }

                        Program.instance.lobbyList.Add(new Lobby(new User(clientConnection, parser[1])));
                        Console.WriteLine(parser[1] + " created a lobby.");
                    }
                    else if (parser[0] == "ClLobby")
                    {
                        Console.WriteLine(parser[1] + " attempted to close their lobby.");

                        PartY_ConnectedClient clientConnection = null;

                        ulong key;
                        bool parsed = ulong.TryParse(parser[2], out key);

                        if (parsed)
                        {
                            for (int i = 0; i < Program.instance.clientList.Count; i++)
                            {
                                if (Program.instance.clientList[i].userID == key)
                                {
                                    clientConnection = Program.instance.clientList[i];
                                    break;
                                }
                            }
                        }

                        if (clientConnection == null)
                        {
                            return;
                        }

                        //Tell everyone the PartY is over :(
                        for (int i = 0; i < Program.instance.clientList.Count; i++)
                        {
                            Program.instance.clientList[i].Send("CloseLobby," + parser[1]);
                        }

                        for (int i = 0; i < Program.instance.lobbyList.Count; i++)
                        {
                            if (Program.instance.lobbyList[i].host.username == parser[1])
                            {
                                Program.instance.lobbyList.Remove(Program.instance.lobbyList[i]);
                                Console.WriteLine(parser[1] + " closed their lobby.");
                                break;
                            }
                        }
                    }
                    else if (parser[0] == "JoinLobby")
                    {
                        Console.WriteLine(parser[2] + " attempted to join " + parser[1] + "'s lobby");

                        PartY_ConnectedClient clientConnection = null;

                        ulong key;
                        bool parsed = ulong.TryParse(parser[3], out key);

                        if (parsed)
                        {
                            for (int i = 0; i < Program.instance.clientList.Count; i++)
                            {
                                if (Program.instance.clientList[i].userID == key)
                                {
                                    clientConnection = Program.instance.clientList[i];
                                    break;
                                }
                            }
                        }

                        if (clientConnection == null)
                        {
                            return;
                        }

                        bool joined = false;

                        for (int i = 0; i < Program.instance.lobbyList.Count; i++)
                        {
                            if (parser[1] == Program.instance.lobbyList[i].host.username)
                            {
                                if (Program.instance.lobbyList[i].usersConnected.Count < 3)
                                {
                                    bool duplicateUsername = false;

                                    for(int z = 0; z < Program.instance.lobbyList[i].usersConnected.Count; z++)
                                        if(Program.instance.lobbyList[i].usersConnected[z].username == parser[2])
                                            duplicateUsername = true;

                                    if (!duplicateUsername)
                                    {
                                        Program.instance.lobbyList[i].usersConnected.Add(new User(clientConnection, parser[2]));
                                        joined = true;
                                    }
                                    else joined = false;
                                }

                                break;
                            }
                        }

                        if (!joined)
                        {
                            byte[] bufferaa = System.Text.Encoding.UTF8.GetBytes("InvalidLobby,SadTimes");
                            stream.Write(bufferaa, 0, bufferaa.Length);

                            Console.WriteLine(parser[2] + " failed to join " + parser[1] + "'s lobby");
                        }
                        else
                        {
                            byte[] buffera = System.Text.Encoding.UTF8.GetBytes("LobbyJoined,HappyTimes");
                            stream.Write(buffera, 0, buffera.Length);

                            for (int i = 0; i < Program.instance.clientList.Count; i++)
                            {
                                Program.instance.clientList[i].Send("LobbyJoiner," + parser[1] + "," + parser[2]);
                            }

                            Console.WriteLine(parser[2] + " successfully joined " + parser[1] + "'s lobby");
                        }
                    }
                    else if (parser[0] == "LeaveLobby")
                    {
                        Console.WriteLine(parser[2] + " attempted to leave " + parser[1] + "'s lobby");

                        PartY_ConnectedClient clientConnection = null;

                        ulong key;
                        bool parsed = ulong.TryParse(parser[3], out key);

                        if (parsed)
                        {
                            for (int i = 0; i < Program.instance.clientList.Count; i++)
                            {
                                if (Program.instance.clientList[i].userID == key)
                                {
                                    clientConnection = Program.instance.clientList[i];
                                    break;
                                }
                            }
                        }

                        if (clientConnection == null)
                        {
                            return;
                        }

                        bool left = false;

                        for (int i = 0; i < Program.instance.lobbyList.Count; i++)
                        {
                            if (left) break;

                            for (int a = 0; a < Program.instance.lobbyList[i].usersConnected.Count; a++)
                            {
                                if (Program.instance.lobbyList[i].usersConnected[a].username == parser[2])
                                {
                                    Program.instance.lobbyList[i].usersConnected.Remove(Program.instance.lobbyList[i].usersConnected[a]);
                                    left = true;
                                    break;
                                }
                            }
                        }

                        if (!left)
                        {
                            //There's no escape off the PartY train >:)
                            byte[] buffer = System.Text.Encoding.UTF8.GetBytes("FailedToLeave,UhOh");
                            stream.Write(buffer, 0, buffer.Length);
                        }
                        else
                        {
                            byte[] buffer = System.Text.Encoding.UTF8.GetBytes("LobbyLeft,SadTimes");
                            stream.Write(buffer, 0, buffer.Length);

                            for (int i = 0; i < Program.instance.clientList.Count; i++)
                            {
                                Program.instance.clientList[i].Send("LobbyLeaver," + parser[1] + "," + parser[2]);
                            }

                            Console.WriteLine(parser[2] + " successfully left " + parser[1] + "'s lobby");
                        }
                    }
                    else if (parser[0] == "GetLobbyData")
                    {
                        string joiners = "";

                        for (int i = 0; i < Program.instance.lobbyList.Count; i++)
                        {
                            if (Program.instance.lobbyList[i].host.username == parser[1])
                            {
                                for (int z = 0; z < Program.instance.lobbyList[i].usersConnected.Count; z++)
                                {
                                    if (z == Program.instance.lobbyList[i].usersConnected.Count - 1)
                                        joiners += Program.instance.lobbyList[i].usersConnected[z].username;
                                    else joiners += Program.instance.lobbyList[i].usersConnected[z].username + ",";
                                }

                                break;
                            }
                        }

                        byte[] buffer = System.Text.Encoding.UTF8.GetBytes("LobbyInfo," + joiners);
                        stream.Write(buffer, 0, buffer.Length);
                    }
                    else if (parser[0] == "StartLobby")
                    {
                        for (int i = 0; i < Program.instance.clientList.Count; i++)
                        {
                            Program.instance.clientList[i].Send("LobbyStarted," + parser[1]);
                        }
                    }
                    else if (parser[0] == "MovementPayload")
                    {
                        #region Unpack payload and update the lobby user
                        float xPos = 0;
                        float yPos = 0;
                        float zPos = 0;

                        float xRot = 0;
                        float yRot = 0;
                        float zRot = 0;

                        float xScale = 0;
                        float yScale = 0;
                        float zScale = 0;

                        bool xPosRes = false;
                        bool yPosRes = false;
                        bool zPosRes = false;

                        xPosRes = float.TryParse(parser[2], out xPos);
                        yPosRes = float.TryParse(parser[3], out yPos);
                        zPosRes = float.TryParse(parser[4], out zPos);

                        bool xRotRes = false;
                        bool yRotRes = false;
                        bool zRotRes = false;

                        xRotRes = float.TryParse(parser[5], out xRot);
                        yRotRes = float.TryParse(parser[6], out yRot);
                        zRotRes = float.TryParse(parser[7], out zRot);

                        bool xScaleRes = false;
                        bool yScaleRes = false;
                        bool zScaleRes = false;

                        xScaleRes = float.TryParse(parser[8], out xScale);
                        yScaleRes = float.TryParse(parser[9], out yScale);
                        zScaleRes = float.TryParse(parser[10], out zScale);

                        int lobbyIndex = -1;

                        for (int i = 0; i < Program.instance.lobbyList.Count; i++)
                        {
                            if (lobbyIndex != -1) break;

                            if (Program.instance.lobbyList[i].host.username == parser[1])
                            {
                                //Hosts data
                                if (parser[11] == Program.instance.lobbyList[i].host.username)
                                {
                                    if (xPosRes && yPosRes && zPosRes)
                                        Program.instance.lobbyList[i].host.transform.position = new PartY_HostSolution.Types.Vector3(xPos, yPos, zPos);

                                    if (xRotRes && yRotRes && zRotRes)
                                        Program.instance.lobbyList[i].host.transform.rotation = new PartY_HostSolution.Types.Vector3(xRot, yRot, zRot);

                                    if (xScaleRes && yScaleRes && zScaleRes)
                                        Program.instance.lobbyList[i].host.transform.scale = new PartY_HostSolution.Types.Vector3(xScale, yScale, zScale);

                                    lobbyIndex = i;
                                    break;
                                }
                                else
                                {
                                    for (int z = 0; z < Program.instance.lobbyList[i].usersConnected.Count; z++)
                                    {
                                        if (parser[11] == Program.instance.lobbyList[i].usersConnected[z].username)
                                        {
                                            if (xPosRes && yPosRes && zPosRes)
                                                Program.instance.lobbyList[i].usersConnected[z].transform.position = new PartY_HostSolution.Types.Vector3(xPos, yPos, zPos);

                                            if (xRotRes && yRotRes && zRotRes)
                                                Program.instance.lobbyList[i].usersConnected[z].transform.rotation = new PartY_HostSolution.Types.Vector3(xRot, yRot, zRot);

                                            if (xScaleRes && yScaleRes && zScaleRes)
                                                Program.instance.lobbyList[i].usersConnected[z].transform.scale = new PartY_HostSolution.Types.Vector3(xScale, yScale, zScale);

                                            lobbyIndex = i;
                                            break;
                                        }
                                    }
                                }
                            }
                        }
                        #endregion

                        try
                        {
                            #region Repack the lobby into a payload

                            //Inject the host data.
                            string payload = "GamePayload," + Program.instance.lobbyList[lobbyIndex].host.username + "," + 
                                Program.instance.lobbyList[lobbyIndex].host.transform.position.x + "," + //pos x
                                 Program.instance.lobbyList[lobbyIndex].host.transform.position.y + "," + //pos y 
                                  Program.instance.lobbyList[lobbyIndex].host.transform.position.z + "," + //pos z
                                   Program.instance.lobbyList[lobbyIndex].host.transform.rotation.x + "," + //rot x
                                    Program.instance.lobbyList[lobbyIndex].host.transform.rotation.y + "," + //rot y 
                                     Program.instance.lobbyList[lobbyIndex].host.transform.rotation.z + "," + //rot z
                                      Program.instance.lobbyList[lobbyIndex].host.transform.scale.x    + "," + //scale x
                                       Program.instance.lobbyList[lobbyIndex].host.transform.scale.y    + "," + //scale y 
                                        Program.instance.lobbyList[lobbyIndex].host.transform.scale.z;           //scale z

                            //Tag on connected users.
                            for (int i = 0; i < Program.instance.lobbyList[lobbyIndex].usersConnected.Count; i++)
                            {
                                payload += "," + Program.instance.lobbyList[lobbyIndex].usersConnected[i].username + ",";

                                payload += Program.instance.lobbyList[lobbyIndex].usersConnected[i].transform.position.x + ",";
                                payload += Program.instance.lobbyList[lobbyIndex].usersConnected[i].transform.position.y + ",";
                                payload += Program.instance.lobbyList[lobbyIndex].usersConnected[i].transform.position.z + ",";

                                payload += Program.instance.lobbyList[lobbyIndex].usersConnected[i].transform.rotation.x + ",";
                                payload += Program.instance.lobbyList[lobbyIndex].usersConnected[i].transform.rotation.y + ",";
                                payload += Program.instance.lobbyList[lobbyIndex].usersConnected[i].transform.rotation.z + ",";

                                payload += Program.instance.lobbyList[lobbyIndex].usersConnected[i].transform.scale.x + ",";
                                payload += Program.instance.lobbyList[lobbyIndex].usersConnected[i].transform.scale.y + ",";
                                payload += Program.instance.lobbyList[lobbyIndex].usersConnected[i].transform.scale.z;
                            }

                            //Write out payload into a buffer.
                            byte[] buffer = System.Text.Encoding.UTF8.GetBytes(payload);
                            //Stream out the payload.
                            stream.Write(buffer, 0, buffer.Length);
                            #endregion

                        }
                        catch (Exception a)
                        {
                            return;
                        }
                    }
                }
            }
            else
            {
                Console.WriteLine(DateTime.Now + ")> " + newMessage);

                //Program.BroadcastChatMessage(newMessage);

                if (Program.instance.lobbyList.Count > 0 && newMessage.Contains("Heartbeat"))
                {
                    string lobbies = "";

                    for (int i = 0; i < Program.instance.lobbyList.Count; i++)
                    {
                        if (i == Program.instance.lobbyList.Count - 1)
                        {
                            lobbies += Program.instance.lobbyList[i].host.username;
                        }
                        else lobbies += Program.instance.lobbyList[i].host.username + ",";
                    }

                    byte[] buffer = System.Text.Encoding.UTF8.GetBytes("Lobbies," + lobbies);
                    stream.Write(buffer, 0, buffer.Length);
                }
                else Program.BroadcastChatMessage(newMessage);
            }
        }

        internal void EndConnect(IAsyncResult ar)
        {
            connection.EndConnect(ar);

            try
            {
                stream.BeginRead(readBuffer, 0, readBuffer.Length, OnRead, null);
            }
            catch (Exception ex)
            {
                if (ex is System.IO.IOException || ex is System.NullReferenceException)
                    return;
            }
        }
        #endregion

        #region API
        internal void Send(string message)
        {
            byte[] buffer = System.Text.Encoding.UTF8.GetBytes(message);

            string val = System.Text.Encoding.UTF8.GetString(buffer);

            try
            {
                stream.Write(buffer, 0, buffer.Length);
            }
            catch (Exception ex)
            {
                if (ex is System.NullReferenceException || ex is System.IO.IOException)
                    return;
            }
        }

        public void SendAPIKey()
        {
            bool keyGenerated = false;
            ulong newKey = 0;
            Random random = new Random();

            while (!keyGenerated)
            {
                //max 9223372036854775807 - 19char
                char[] gen = new char[19];

                gen[0] = random.Next(0, 10).ToString().ToCharArray()[0];
                gen[1] = random.Next(0, 10).ToString().ToCharArray()[0];
                gen[2] = random.Next(0, 10).ToString().ToCharArray()[0];
                gen[3] = random.Next(0, 10).ToString().ToCharArray()[0];
                gen[4] = random.Next(0, 10).ToString().ToCharArray()[0];
                gen[5] = random.Next(0, 10).ToString().ToCharArray()[0];
                gen[6] = random.Next(0, 10).ToString().ToCharArray()[0];
                gen[7] = random.Next(0, 10).ToString().ToCharArray()[0];
                gen[8] = random.Next(0, 10).ToString().ToCharArray()[0];
                gen[9] = random.Next(0, 10).ToString().ToCharArray()[0];
                gen[10] = random.Next(0, 10).ToString().ToCharArray()[0];
                gen[11] = random.Next(0, 10).ToString().ToCharArray()[0];
                gen[12] = random.Next(0, 10).ToString().ToCharArray()[0];
                gen[13] = random.Next(0, 10).ToString().ToCharArray()[0];
                gen[14] = random.Next(0, 10).ToString().ToCharArray()[0];
                gen[15] = random.Next(0, 10).ToString().ToCharArray()[0];
                gen[16] = random.Next(0, 10).ToString().ToCharArray()[0];
                gen[17] = random.Next(0, 10).ToString().ToCharArray()[0];
                gen[18] = random.Next(0, 10).ToString().ToCharArray()[0];

                string _parser = "";

                for (int i = 0; i < 19; i++)
                {
                    _parser += gen[i];
                }

                Console.WriteLine("Generated new key: " + _parser);

                bool failed = false;

                failed = !ulong.TryParse(_parser, out newKey);

                if (!failed)
                {
                    bool isValid = true;

                    for (int i = 0; i < Program.existingKeys.Count; i++)
                    {
                        if (Program.existingKeys[i] == newKey)
                        {
                            isValid = false;
                            break;
                        }
                    }

                    if (isValid)
                    {
                        Console.WriteLine("Key was valid!");
                        Program.existingKeys.Add(newKey);
                        keyGenerated = true;

                        Program.instance.RewriteTokens();
                    }
                    else
                    {
                        Console.WriteLine("Key was not valid! Trying again...");
                    }
                }
                else Console.WriteLine("Key was not valid! Error parsing :(");
            }

            byte[] buffer = System.Text.Encoding.UTF8.GetBytes("KeyGen," + newKey.ToString());
            stream.Write(buffer, 0, buffer.Length);

            Console.WriteLine("Key sent to requester, safe travels fren");
        }
        #endregion
    }
}