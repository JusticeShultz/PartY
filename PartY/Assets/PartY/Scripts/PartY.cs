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

namespace PartY
{
    public class PartY : MonoBehaviour
    {
        #region Data
        public static PartY instance;

        /// <summary>
        /// IP for clients to connect to. Null if you are the server.
        /// </summary>
        public IPAddress serverIp;
        
        /// <summary>
        /// For Clients, there is only one and it's the connection to the server.
        /// For Servers, there are many - one per connected client.
        /// </summary>
        List<PartY_ConnectedClient> clientList = new List<PartY_ConnectedClient>();

        /// <summary>
        /// The string to render in Unity.
        /// </summary>
        public static string messageToDisplay;
        //public Text text;

        /// <summary>
        /// Accepts new connections.  Null for clients.
        /// </summary>
        TcpListener listener;

        public Text serverDebugLog;
        public InputField clientIpInput;

        public UnityEvent onSuccessfulConnection = new UnityEvent();
        public UnityEvent onFailedToConnect = new UnityEvent();
        public UnityEvent onDisconnect = new UnityEvent();

        public List<Client> clients = new List<Client>();
        public List<HostedLobby> lobbies = new List<HostedLobby>();

        #endregion

        #region Unity Events
        public void Awake()
        {
            instance = this;
        }

        public void ConnectToServer()
        {
            instance = this;
            
            bool valid = false;
            IPAddress iP = new IPAddress(0);

            valid = IPAddress.TryParse(clientIpInput.text, out iP);
            serverIp = valid ? iP : null;
            
            // Client protocal - try connecting to the server
            if(!valid)
            {
                onFailedToConnect.Invoke();
                return;
            }

            Debug.Log("Client starting...");
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
                onFailedToConnect.Invoke();
            else onSuccessfulConnection.Invoke();

            
        }

        protected void OnApplicationQuit()
        {
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
        }
        #endregion

        #region API
        public void OnDisconnect(PartY_ConnectedClient client)
        {
            listener?.Stop();
            onDisconnect.Invoke();
            clientList.Remove(client);
        }

        internal void Send(string message)
        {
            BroadcastChatMessage(message);
        }

        internal static void BroadcastChatMessage(string message)
        {
            for (int i = 0; i < instance.clientList.Count; i++)
            {
                PartY_ConnectedClient client = instance.clientList[i];
                client.Send(message);
            }
        }
        #endregion
    }
}