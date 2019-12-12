using System;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Runtime.Serialization;
using UnityEngine;
using System.Net;
using System.IO;
using System.Text;

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

        public readonly TcpClient connection;

        readonly byte[] readBuffer = new byte[5000];

        public NetworkStream stream
        {
            get
            {
                if (connection.Connected)
                    return connection.GetStream();
                else return null;
            }
        }
        #endregion

        #region Init
        public PartY_ConnectedClient(TcpClient tcpClient)
        {
            this.connection = tcpClient;
            PartY.myConnection = this;
            this.connection.NoDelay = true; // Disable Nagle's cache algorithm
        }

        internal void Close()
        {
            connection.Close();
        }
        #endregion

        #region Async Events
        void OnRead(IAsyncResult ar)
        {
            if (stream == null)
            {
                PartY.instance.OnDisconnect(this);
                return;
            }

            int length = stream.EndRead(ar);

            if (length <= 0)
            { // Connection closed
                PartY.instance.OnDisconnect(this);
                return;
            }

            string newMessage = System.Text.Encoding.UTF8.GetString(readBuffer, 0, length);

            Debug.Log(newMessage);

            stream.BeginRead(readBuffer, 0, readBuffer.Length, OnRead, null);

            if (newMessage.Contains(","))
            {
                string[] parser = newMessage.Split(',');

                if (parser.Length > 0)
                {
                    if (parser[0] == "KeyGen")
                    {
                        Debug.Log("Recieved KeyGen request");

                        Debug.Log(parser[1]);

                        //Stops task?
                        //PlayerPrefs.SetString("ServerID", parser[1]);
                        WriteToken(parser[1]);

                        Debug.Log("Saved key -   " + parser[1] + "   - successfully.");

                        bool parsed = ulong.TryParse(parser[1], out userID);

                        if (parsed)
                        {
                            Debug.Log("Token was read, verifying with host!");
                            SendTextData("Verify," + userID);
                        }
                        else Debug.Log("Something wen't wrong trying to parse the ID the host sent...");
                    }
                    else if (parser[0] == "Verified")
                    {
                        Debug.Log("Logged in with ID successfully! Now fully connected.");
                        PartY.loggedIn = true;
                    }
                    else if (parser[0] == "NewLobby")
                    {
                        Debug.Log("New lobby added to the lobby pool.");
                        LobbyHandler.lobbies.Add(new LobbyHandler.Lobby(parser[1]));
                        LobbyHandler.instance.RecreateLobbyUI();
                    }
                    else if (parser[0] == "CloseLobby")
                    {
                        Debug.Log("Removed a lobby from the lobby pool.");

                        for (int i = 0; i < LobbyHandler.lobbies.Count; i++)
                        {
                            if (LobbyHandler.lobbies[i].ownerUsername == parser[1])
                            {
                                LobbyHandler.lobbies.Remove(LobbyHandler.lobbies[i]);
                                break;
                            }
                        }

                        if (LobbyHandler.myLobby.ownerUsername == parser[1])
                        {
                            LobbySpawner.hostClosed = true;
                            LobbySpawner.leaveLobby = true;
                        }

                        LobbyHandler.instance.RecreateLobbyUI();
                    }
                    else if (parser[0] == "Lobbies")
                    {
                        Debug.Log("Got all active lobbies.");

                        LobbyHandler.lobbies = new List<LobbyHandler.Lobby>();

                        for (int i = 1; i < parser.Length; i++)
                        {
                            if (parser.Length == 1) break;

                            LobbyHandler.lobbies.Add(new LobbyHandler.Lobby(parser[i]));
                        }

                        LobbyHandler.instance.RecreateLobbyUI();
                    }
                    else if (parser[0] == "LobbyJoiner")
                    {
                        Debug.Log(parser[2] + " joined " + parser[1] + "'s lobby");

                        for (int i = 0; i < LobbyHandler.lobbies.Count; i++)
                        {
                            if (LobbyHandler.lobbies[i].ownerUsername == parser[1])
                            {
                                LobbyHandler.lobbies[i].lobbySize += 1;
                                LobbyHandler.lobbies[i].clients.Add(parser[2]);

                                if (LobbyHandler.myLobby != null)
                                {
                                    if (LobbyHandler.myLobby.ownerUsername == parser[1])
                                    {
                                        LobbyHandler.myLobby.clients.Add(parser[2]);

                                        if (LobbyHandler.instance.usernameField.text == parser[1])
                                            LobbySpawner.updateHostedLobby = true;
                                        else LobbySpawner.updateJoinedLobby = true;
                                    }
                                }

                                break;
                            }
                        }

                        LobbyHandler.instance.RecreateLobbyUI();
                    }
                    else if (parser[0] == "LobbyLeaver")
                    {
                        Debug.Log(parser[2] + " left " + parser[1] + "'s lobby");

                        for (int i = 0; i < LobbyHandler.lobbies.Count; i++)
                        {
                            if (LobbyHandler.lobbies[i].ownerUsername == parser[1])
                            {
                                LobbyHandler.lobbies[i].lobbySize -= 1;

                                for (int z = 0; z < LobbyHandler.lobbies[i].clients.Count; z++)
                                {
                                    if (LobbyHandler.lobbies[i].clients[z] == parser[2])
                                    {
                                        LobbyHandler.lobbies[i].clients.Remove(LobbyHandler.lobbies[i].clients[z]);
                                    }
                                }
                            }

                            if (LobbyHandler.myLobby != null)
                            {
                                if (LobbyHandler.myLobby.ownerUsername == parser[1])
                                {
                                    for (int z = 0; z < LobbyHandler.myLobby.clients.Count; z++)
                                    {
                                        if (LobbyHandler.myLobby.clients[z] == parser[2])
                                        {
                                            LobbyHandler.myLobby.clients.Remove(LobbyHandler.myLobby.clients[z]);
                                        }
                                    }

                                    if (LobbyHandler.instance.usernameField.text == parser[1])
                                        LobbySpawner.updateHostedLobby = true;
                                    else LobbySpawner.updateJoinedLobby = true;
                                }
                            }
                        }

                        LobbyHandler.instance.RecreateLobbyUI();
                    }
                    else if (parser[0] == "InvalidLobby")
                    {
                        Debug.LogWarning("Invalid lobby!");
                        LobbyHandler.joining = false;
                        LobbySpawner.lobbyFull = true;
                    }
                    else if (parser[0] == "FailedToLeave")
                    {
                        Debug.LogWarning("Failed to leave lobby!");
                    }
                    else if (parser[0] == "LobbyJoined")
                    {
                        Debug.Log("Successfully joined the lobby!");
                        LobbyHandler.instance.JoinedLobby();
                        LobbySpawner.updateJoinedLobby = true;
                        LobbyHandler.joining = false;
                    }
                    else if (parser[0] == "LobbyLeft")
                    {
                        Debug.Log("Successfully left the lobby!");
                        LobbyHandler.instance.LeftLobby();
                    }
                    else if (parser[0] == "LobbyInfo")
                    {
                        if (LobbyHandler.myLobby != null)
                        {
                            Debug.Log("Got info about current joined lobby");

                            //Flush out old client data.
                            LobbyHandler.myLobby.clients = new List<string>();

                            for (int i = 1; i < parser.Length; i++)
                            {
                                LobbyHandler.myLobby.clients.Add(parser[i]);
                            }

                            LobbyHandler.instance.RecreateLobbyUI();
                        }
                        else
                        {
                            Debug.LogWarning("Tried to get info about current joined lobby, but you weren't detected in a lobby!");
                        }
                    }
                    else if (parser[0] == "LobbyStarted")
                    {
                        bool myLobby = false;

                        if (LobbyHandler.myLobby != null)
                        {
                            if (LobbyHandler.myLobby.ownerUsername == parser[1])
                            {
                                myLobby = true;

                                //Mark lobby as in progress.

                                //Start up lobby
                                LobbySpawner.startGame = true;
                            }
                        }

                        if (!myLobby)
                        {
                            //Mark lobby as in progress.
                        }
                    }
                    else if (parser[0] == "GamePayload")
                    {
                        try
                        {
                            if(parser.Length > 0)
                            //Pick out data and give it to target transforms.
                            if (parser[1] != LobbyHandler.instance.usernameField.text)
                            {
                                for (int i = 0; i < GameHandler.instance.otherPlayers.Count; i++)
                                {
                                    if (GameHandler.instance.otherPlayers[i].cachedName == parser[1])
                                    {
                                        float posX = 0f;
                                        float posY = 0f;
                                        float posZ = 0f;
                                        float rotX = 0f;
                                        float rotY = 0f;
                                        float rotZ = 0f;
                                        float scaleX = 0f;
                                        float scaleY = 0f;
                                        float scaleZ = 0f;

                                        bool posXParsed = float.TryParse(parser[2], out posX);
                                        bool posYParsed = float.TryParse(parser[3], out posY);
                                        bool posZParsed = float.TryParse(parser[4], out posZ);

                                        bool rotXParsed = float.TryParse(parser[5], out rotX);
                                        bool rotYParsed = float.TryParse(parser[6], out rotY);
                                        bool rotZParsed = float.TryParse(parser[7], out rotZ);

                                        bool scaleXParsed = float.TryParse(parser[8], out scaleX);
                                        bool scaleYParsed = float.TryParse(parser[9], out scaleY);
                                        bool scaleZParsed = float.TryParse(parser[10], out scaleZ);

                                        if (posXParsed && posYParsed && posZParsed)
                                            GameHandler.instance.otherPlayersTargets[i].position = new Vector3(posX, posY, posZ);

                                        if (rotXParsed && rotYParsed && rotZParsed)
                                            GameHandler.instance.otherPlayersTargets[i].rotation = new Vector3(rotX, rotY, rotZ);

                                        if (scaleXParsed && scaleYParsed && scaleZParsed)
                                            GameHandler.instance.otherPlayersTargets[i].scale = new Vector3(scaleX, scaleY, scaleZ);
                                    }
                                }
                            }

                            if(parser.Length > 11)
                            if (parser[11] != LobbyHandler.instance.usernameField.text)
                            {
                                for (int i = 0; i < GameHandler.instance.otherPlayers.Count; i++)
                                {
                                    if (GameHandler.instance.otherPlayers[i].cachedName == parser[11])
                                    {
                                        float posX = 0f;
                                        float posY = 0f;
                                        float posZ = 0f;
                                        
                                        float rotX = 0f;
                                        float rotY = 0f;
                                        float rotZ = 0f;
                                        
                                        float scaleX = 0f;
                                        float scaleY = 0f;
                                        float scaleZ = 0f;
                                        
                                        bool posXParsed = float.TryParse(parser[12], out posX);
                                        bool posYParsed = float.TryParse(parser[13], out posY);
                                        bool posZParsed = float.TryParse(parser[14], out posZ);
                                        
                                        bool rotXParsed = float.TryParse(parser[15], out rotX);
                                        bool rotYParsed = float.TryParse(parser[16], out rotY);
                                        bool rotZParsed = float.TryParse(parser[17], out rotZ);
                                        
                                        bool scaleXParsed = float.TryParse(parser[18], out scaleX);
                                        bool scaleYParsed = float.TryParse(parser[19], out scaleY);
                                        bool scaleZParsed = float.TryParse(parser[20], out scaleZ);
                                        
                                        if (posXParsed && posYParsed && posZParsed)
                                            GameHandler.instance.otherPlayersTargets[i].position = new Vector3(posX, posY, posZ);
                                        
                                        if (rotXParsed && rotYParsed && rotZParsed)
                                            GameHandler.instance.otherPlayersTargets[i].rotation = new Vector3(rotX, rotY, rotZ);

                                        if (scaleXParsed && scaleYParsed && scaleZParsed)
                                            GameHandler.instance.otherPlayersTargets[i].scale = new Vector3(scaleX, scaleY, scaleZ);
                                    }
                                }
                            }

                            if(parser.Length > 20)
                            if (parser[21] != LobbyHandler.instance.usernameField.text)
                            {
                                for (int i = 0; i < GameHandler.instance.otherPlayers.Count; i++)
                                {
                                    if (GameHandler.instance.otherPlayers[i].cachedName == parser[21])
                                    {
                                        float posX = 0f;
                                        float posY = 0f;
                                        float posZ = 0f;
                                        float rotX = 0f;
                                        float rotY = 0f;
                                        float rotZ = 0f;
                                        float scaleX = 0f;
                                        float scaleY = 0f;
                                        float scaleZ = 0f;

                                        bool posXParsed = float.TryParse(parser[22], out posX);
                                        bool posYParsed = float.TryParse(parser[23], out posY);
                                        bool posZParsed = float.TryParse(parser[24], out posZ);

                                        bool rotXParsed = float.TryParse(parser[25], out rotX);
                                        bool rotYParsed = float.TryParse(parser[26], out rotY);
                                        bool rotZParsed = float.TryParse(parser[27], out rotZ);

                                        bool scaleXParsed = float.TryParse(parser[28], out scaleX);
                                        bool scaleYParsed = float.TryParse(parser[29], out scaleY);
                                        bool scaleZParsed = float.TryParse(parser[30], out scaleZ);

                                        if (posXParsed && posYParsed && posZParsed)
                                            GameHandler.instance.otherPlayersTargets[i].position = new Vector3(posX, posY, posZ);

                                        if (rotXParsed && rotYParsed && rotZParsed)
                                            GameHandler.instance.otherPlayersTargets[i].rotation = new Vector3(rotX, rotY, rotZ);

                                        if (scaleXParsed && scaleYParsed && scaleZParsed)
                                            GameHandler.instance.otherPlayersTargets[i].scale = new Vector3(scaleX, scaleY, scaleZ);
                                    }
                                }
                            }

                            if(parser.Length > 30)
                            if (parser[31] != LobbyHandler.instance.usernameField.text)
                            {
                                for (int i = 0; i < GameHandler.instance.otherPlayers.Count; i++)
                                {
                                    if (GameHandler.instance.otherPlayers[i].cachedName == parser[31])
                                    {
                                        float posX = 0f;
                                        float posY = 0f;
                                        float posZ = 0f;
                                        float rotX = 0f;
                                        float rotY = 0f;
                                        float rotZ = 0f;
                                        float scaleX = 0f;
                                        float scaleY = 0f;
                                        float scaleZ = 0f;

                                        bool posXParsed = float.TryParse(parser[32], out posX);
                                        bool posYParsed = float.TryParse(parser[33], out posY);
                                        bool posZParsed = float.TryParse(parser[34], out posZ);

                                        bool rotXParsed = float.TryParse(parser[35], out rotX);
                                        bool rotYParsed = float.TryParse(parser[36], out rotY);
                                        bool rotZParsed = float.TryParse(parser[37], out rotZ);

                                        bool scaleXParsed = float.TryParse(parser[38], out scaleX);
                                        bool scaleYParsed = float.TryParse(parser[39], out scaleY);
                                        bool scaleZParsed = float.TryParse(parser[40], out scaleZ);

                                        if (posXParsed && posYParsed && posZParsed)
                                            GameHandler.instance.otherPlayersTargets[i].position = new Vector3(posX, posY, posZ);

                                        if (rotXParsed && rotYParsed && rotZParsed)
                                            GameHandler.instance.otherPlayersTargets[i].rotation = new Vector3(rotX, rotY, rotZ);

                                        if (scaleXParsed && scaleYParsed && scaleZParsed)
                                            GameHandler.instance.otherPlayersTargets[i].scale = new Vector3(scaleX, scaleY, scaleZ);
                                    }
                                }

                            }
                        } catch(Exception a)
                        {
                            Debug.LogError("Game data payload failed with the exception: " + a.Message);

                            return;
                        }

                        Console.WriteLine("Parsed!");
                    }
                }
            }
            else
            {
                if (newMessage == "DuplicateUsername")
                {
                    Debug.Log("Using a duplicate username, aborting lobby creation...");

                    LobbyHandler.instance._Menu.SetActive(true);
                    LobbyHandler.instance._HostedLobby.SetActive(false);
                    LobbyHandler.instance._JoinedLobby.SetActive(false);
                    LobbyHandler.instance.duplicateUsername.SetActive(false);
                }
            }

            //PartY.messageToDisplay += newMessage + Environment.NewLine;

            //stream.BeginRead(readBuffer, 0, readBuffer.Length, OnRead, null);
        }

        internal void EndConnect(IAsyncResult ar)
        {
            connection.EndConnect(ar);

            stream.BeginRead(readBuffer, 0, readBuffer.Length, OnRead, null);
        }
        #endregion

        #region API
        internal void SendTextData(string @message)
        {
            SendPayload(System.Text.Encoding.UTF8.GetBytes(@message));
        }

        internal void SendStringData(string variable, string @string)
        {
            SendPayload(System.Text.Encoding.UTF8.GetBytes(variable + "," + @string));
        }

        internal void SendVector3Data(string variable, Vector3 @vector3)
        {
            SendPayload(System.Text.Encoding.UTF8.GetBytes(variable + "," + @vector3.x + "," + @vector3.y + "," + @vector3.z));
        }

        internal void SendVector2Data(string variable, Vector2 @vector2)
        {
            SendPayload(System.Text.Encoding.UTF8.GetBytes(variable + "," + @vector2.x + "," + @vector2.y));
        }

        internal void SendIntegerData(string variable, int @integer)
        {
            SendPayload(System.Text.Encoding.UTF8.GetBytes(variable + "," + @integer));
        }

        internal void SendFloatData(string variable, float @floatingPointNumber)
        {
            SendPayload(System.Text.Encoding.UTF8.GetBytes(variable + "," + @floatingPointNumber));
        }

        internal void SendUintData(string variable, uint @unsignedInteger)
        {
            SendPayload(System.Text.Encoding.UTF8.GetBytes(variable + "," + @unsignedInteger));
        }

        internal void SendLongData(string variable, float @floatingPointNumber)
        {
            SendPayload(System.Text.Encoding.UTF8.GetBytes(variable + "," + @floatingPointNumber));
        }

        internal void SendData(string variable, Type @data)
        {
            SendPayload(System.Text.Encoding.UTF8.GetBytes(variable + "," + @data));
        }

        internal void SendPayload(byte[] buffer)
        {
            if (stream == null)
            {
                PartY.instance.OnDisconnect(this);
                return;
            }

            stream.Write(buffer, 0, buffer.Length);
        }

        public void ReadToken()
        {
            if (!File.Exists(PartY.path + "/KeyDictionary" + PartY.instance.clientVariant.text + ".PartY"))
            {
                // Create the file.
                using (FileStream fs = File.Create(PartY.path + "/KeyDictionary" + PartY.instance.clientVariant.text + ".PartY"))
                {
                    Byte[] info =
                        new UTF8Encoding(true).GetBytes("");

                    // Add some information to the file.
                    fs.Write(info, 0, info.Length);
                }

                Debug.Log("Generated a KeyDictionary.PartY file");
            }

            // Open the stream and read it back.
            using (StreamReader sr = File.OpenText(PartY.path + "/KeyDictionary" + PartY.instance.clientVariant.text + ".PartY"))
            {
                string s = "";
                while ((s = sr.ReadLine()) != null)
                {
                    ulong input;
                    bool didParse = ulong.TryParse(s, out input);

                    if (didParse)
                        userID = input;
                    else Debug.Log("Error parsing " + s + " inside of the existing keys dictionary");
                }
            }
        }

        public void WriteToken(string token)
        {
            if (!File.Exists(PartY.path + "/KeyDictionary" + PartY.instance.clientVariant.text + ".PartY"))
            {
                // Create the file.
                using (FileStream fs = File.Create(PartY.path + "/KeyDictionary" + PartY.instance.clientVariant.text + ".PartY"))
                {
                    Byte[] info =
                        new UTF8Encoding(true).GetBytes("");

                    // Add some information to the file.
                    fs.Write(info, 0, info.Length);
                }

                Console.WriteLine("Generated a KeyDictionary.PartY file, didn't successfully write any data.");
            }

            // Open the stream and read it back.

            File.WriteAllText(PartY.path + "/KeyDictionary" + PartY.instance.clientVariant.text + ".PartY", token);
        }
        #endregion
    }
}