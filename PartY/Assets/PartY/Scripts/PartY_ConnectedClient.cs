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

                        if(parsed)
                            Debug.Log("ID established! Connection successful");
                        else Debug.Log("Something wen't wrong trying to parse the ID the host sent...");
                    }
                    else if (parser[0] == "Verified")
                    {
                        Debug.Log("Logged in with ID successfully! Now fully connected.");
                        PartY.LoggedIn = true;
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
                            if(LobbyHandler.lobbies[i].ownerUsername == parser[1])
                            {
                                LobbyHandler.lobbies.Remove(LobbyHandler.lobbies[i]);
                                break;
                            }
                        }

                        LobbyHandler.instance.RecreateLobbyUI();
                    }
                }
            }

            //PartY.messageToDisplay += newMessage + Environment.NewLine;

            stream.BeginRead(readBuffer, 0, readBuffer.Length, OnRead, null);
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
            if (!File.Exists(PartY.path + "/KeyDictionary.PartY"))
            {
                // Create the file.
                using (FileStream fs = File.Create(PartY.path + "/KeyDictionary.PartY"))
                {
                    Byte[] info =
                        new UTF8Encoding(true).GetBytes("");

                    // Add some information to the file.
                    fs.Write(info, 0, info.Length);
                }

                Debug.Log("Generated a KeyDictionary.PartY file");
            }

            // Open the stream and read it back.
            using (StreamReader sr = File.OpenText(PartY.path + "/KeyDictionary.PartY"))
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
            if (!File.Exists(PartY.path + "/KeyDictionary.PartY"))
            {
                // Create the file.
                using (FileStream fs = File.Create(PartY.path + "/KeyDictionary.PartY"))
                {
                    Byte[] info =
                        new UTF8Encoding(true).GetBytes("");

                    // Add some information to the file.
                    fs.Write(info, 0, info.Length);
                }

                Console.WriteLine("Generated a KeyDictionary.PartY file, didn't successfully write any data.");
            }

            // Open the stream and read it back.

            File.WriteAllText(PartY.path + "/KeyDictionary.PartY", token);
        }
        #endregion
    }
}