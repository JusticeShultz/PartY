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
                return connection.GetStream();
            }
        }
        #endregion

        #region Init
        public PartY_ConnectedClient(TcpClient tcpClient)
        {
            this.connection = tcpClient;
            this.connection.NoDelay = true; // Disable Nagle's cache algorithm
            stream.BeginRead(readBuffer, 0, readBuffer.Length, OnRead, null);
        }

        internal void Close()
        {
            connection.Close();
        }
        #endregion

        #region Async Events
        void OnRead(IAsyncResult ar)
        {
            int length = stream.EndRead(ar);
            if (length <= 0)
            { // Connection closed
                Program.instance.OnDisconnect(this);
                return;
            }

            string newMessage = System.Text.Encoding.UTF8.GetString(readBuffer, 0, length);

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

                        if(!parsed)
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

                        if(parsed)
                        {
                            for (int i = 0; i < Program.instance.clientList.Count; i++)
                            {
                                if(Program.instance.clientList[i].userID == key)
                                {
                                    clientConnection = Program.instance.clientList[i];
                                    break;
                                }
                            }
                        }

                        if (clientConnection == null) return;

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

                        if (clientConnection == null) return;

                        //Tell everyone the PartY is over :(
                        for (int i = 0; i < Program.instance.clientList.Count; i++)
                        {
                            Program.instance.clientList[i].Send("CloseLobby," + parser[1]);
                        }

                        for(int i = 0; i < Program.instance.lobbyList.Count; i++)
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
                        Console.WriteLine(parser[1] + " attempted to join lobby #" + parser[1] + ".");

                        //PartY_ConnectedClient clientConnection = null;

                        //ulong key;
                        //bool parsed = ulong.TryParse(parser[2], out key);

                        //if (parsed)
                        //{
                        //    for (int i = 0; i < Program.instance.clientList.Count; i++)
                        //    {
                        //        if (Program.instance.clientList[i].userID == key)
                        //        {
                        //            clientConnection = Program.instance.clientList[i];
                        //            break;
                        //        }
                        //    }
                        //}

                        //if (clientConnection == null) return;

                        ////Tell everyone the PartY is over :(
                        //for (int i = 0; i < Program.instance.clientList.Count; i++)
                        //{
                        //    Program.instance.clientList[i].Send("CloseLobby," + parser[1]);
                        //}

                        //for (int i = 0; i < Program.instance.lobbyList.Count; i++)
                        //{
                        //    if (Program.instance.lobbyList[i].host.username == parser[1])
                        //    {
                        //        Program.instance.lobbyList.Remove(Program.instance.lobbyList[i]);
                        //        Console.WriteLine(parser[1] + " attempted to join lobby #" + parser[1] + ".");
                        //        break;
                        //    }
                        //}
                    }
                }
            }
            else
            {
                Console.WriteLine(DateTime.Now + ")> " + newMessage);

                Program.BroadcastChatMessage(newMessage);
            }
            
            stream.BeginRead(readBuffer, 0, readBuffer.Length, OnRead, null);
        }

        internal void EndConnect(IAsyncResult ar)
        {
            connection.EndConnect(ar);

            stream.BeginRead(readBuffer, 0, readBuffer.Length, OnRead, null);
        }
        #endregion

        #region API
        internal void Send(string message)
        {
            byte[] buffer = System.Text.Encoding.UTF8.GetBytes(message);

            string val = System.Text.Encoding.UTF8.GetString(buffer);

            stream.Write(buffer, 0, buffer.Length);
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