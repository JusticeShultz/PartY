using System;
using System.Collections.Generic;
using System.Collections;
using System.Runtime.Serialization;
using UnityEngine;
using UnityEngine.UI;
using System.Net.Sockets;
using System.Net;
using System.IO;
using UnityEngine.Events;
using System.Reflection;
using System.Threading.Tasks;
using System.Text;

namespace PartY
{
    public class PartY : MonoBehaviour
    {
        #region Data
        public static string path;
        public static bool LoggedIn = false;
        public static PartY instance;

        /// <summary>
        /// IP for clients to connect to. Null if you are the server.
        /// </summary>
        public IPAddress serverIp;

        public ulong clientID = 0;
        
        /// <summary>
        /// For Clients, there is only one and it's the connection to the server.
        /// For Servers, there are many - one per connected client.
        /// </summary>
        List<PartY_ConnectedClient> clientList = new List<PartY_ConnectedClient>();

        /// <summary>
        /// Accepts new connections.  Null for clients.
        /// </summary>
        TcpListener listener;

        public InputField clientIpInput;

        public UnityEvent onSuccessfulConnection = new UnityEvent();
        public UnityEvent onFailedToConnect = new UnityEvent();
        public UnityEvent onDisconnect = new UnityEvent();

        public List<Client> clients = new List<Client>();
        public List<HostedLobby> lobbies = new List<HostedLobby>();

        private static bool ConnectionStatus = false;

        public static bool IsConnectedToServer { get { return ConnectionStatus; } private set { ConnectionStatus = value; } }
        #endregion

        #region Unity Events
        public void Awake()
        {
            //path = Application.persistentDataPath;
            

            //for(int i = 0; i < 5; i++)
            //    path = path.Remove(path.Length - 1);

            path = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);

            Debug.LogError(path);

            instance = this;
            SetServerStatus(false);
        }

        public void ConnectToServer()
        {
            instance = this;

            bool valid = false;
            IPAddress iP = new IPAddress(0);

            valid = IPAddress.TryParse(clientIpInput.text, out iP);

            if (clientIpInput.text == "localhost")
            {
                valid = IPAddress.TryParse("127.0.0.1", out iP);
            }

            serverIp = valid ? iP : null;

            // Client protocal - try connecting to the server
            if (!valid)
            {
                onFailedToConnect.Invoke();
                return;
            }

            Debug.Log("Client starting...");
            //Debug.Log("Saving to... " + Application.persistentDataPath);
            TcpClient client = new TcpClient();
            PartY_ConnectedClient connectedClient = new PartY_ConnectedClient(client);
            clientList.Add(connectedClient);
            IAsyncResult result = client.BeginConnect(serverIp, PartY_Globals.port, (ar) => connectedClient.EndConnect(ar), null);

            while (!result.IsCompleted) { }

            if (!client.Connected)
                Debug.Log("Client failed! :(");
            else
                Debug.Log("Client connected! :)");

            if (!client.Connected)
            {
                SetServerStatus(false);
                onFailedToConnect.Invoke();
            }
            else
            {
                ReadToken();

                if (clientID == 0)
                {
                    Debug.Log("Key was invalid, asking for a new one!");
                    SendTextData("NeedTokenID," + new WebClient().DownloadString("http://icanhazip.com"));
                }
                else
                {
                    //ulong id_result;
                    //bool didParse = ulong.TryParse(PlayerPrefs.GetString("ServerID"), out id_result);

                    //if(didParse)
                    //{
                    //    SendTextData("Verify," + id_result);
                    //}
                    //else
                    //{
                    //    //Else, error, external modification of user ID, ask for new one.
                    //    SendTextData("NeedTokenID");
                    //}
                    Debug.Log("Token was read, verifying with host!");
                    SendTextData("Verify," + clientID);
                }

                SetServerStatus(true);
                onSuccessfulConnection.Invoke();
            }
        }

        protected void OnApplicationQuit()
        {
            SetServerStatus(false);

            listener?.Stop();

            for (int i = 0; i < clientList.Count; i++)
            {
                clientList[i].Close();
            }
        }

        #endregion

        #region Async Events
        void OnServerConnect(IAsyncResult ar)
        {
            TcpClient tcpClient = listener.EndAcceptTcpClient(ar);
            clientList.Add(new PartY_ConnectedClient(tcpClient));
            listener.BeginAcceptTcpClient(OnServerConnect, null);
            SetServerStatus(true);
        }
        #endregion

        #region API
        public void OnDisconnect(PartY_ConnectedClient client)
        {
            SetServerStatus(false);
            listener?.Stop();
            onDisconnect.Invoke();
            clientList.Remove(client);
        }

        internal void SendTextData(string message)
        {
            BroadcastTextMessage(message);
        }

        internal static void BroadcastTextMessage(string message)
        {
            for (int i = 0; i < instance.clientList.Count; i++)
            {
                PartY_ConnectedClient client = instance.clientList[i];
                client.SendTextData(message);
            }
        }

        public static void SetServerStatus(bool status)
        {
            IsConnectedToServer = status;
        }

        public void ReadToken()
        {
            if (!File.Exists(PartY.path + "/PartY_Data/"))
            {
                // Create the file.
                using (FileStream fs = File.Create(PartY.path + "/PartY_Data/"))
                {
                }

                Debug.Log("Generated a /PartY_Data/ folder");
            }

            if (!File.Exists(PartY.path + "/PartY_Data/KeyDictionary.PartY"))
            {
                // Create the file.
                using (FileStream fs = File.Create(PartY.path + "/PartY_Data/KeyDictionary.PartY"))
                {
                    Byte[] info =
                        new UTF8Encoding(true).GetBytes("");

                    // Add some information to the file.
                    fs.Write(info, 0, info.Length);
                }

                Debug.Log("Generated a KeyDictionary.PartY file");
            }

            // Open the stream and read it back.
            using (StreamReader sr = File.OpenText(PartY.path + "/PartY_Data/KeyDictionary.PartY"))
            {
                string s = "";
                while ((s = sr.ReadLine()) != null)
                {
                    ulong input;
                    bool didParse = ulong.TryParse(s, out input);

                    if (didParse)
                        clientID = input;
                    else Debug.Log("Error parsing " + s + " inside of the existing keys dictionary");
                }
            }
        }
        #endregion
    }
}