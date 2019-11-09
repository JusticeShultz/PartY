using System;
using System.Collections.Generic;
using System.Collections;
using System.Runtime.Serialization;
using System.Net.Sockets;
using System.Net;
using System.IO;
using System.Reflection;
using System.Threading.Tasks;


namespace PartY
{
    class Program
    {
        #region Data
        public const int port = 27015;

        public static Program instance;

        public IPAddress serverIp;

        List<PartY_ConnectedClient> clientList = new List<PartY_ConnectedClient>();

        TcpListener listener;
        #endregion

        #region Main Async Startup
        static void Main(string[] args)
        {
            Main2();
        }

        static void Main2() => new Program().MainAsync().GetAwaiter().GetResult();

        private async Task MainAsync()
        {
            instance = this;
            AppDomain.CurrentDomain.ProcessExit += new EventHandler(OnApplicationQuit);

            Console.WriteLine(DateTime.Now + ")> Server starting...");
            listener = new TcpListener(localaddr: IPAddress.Any, port: port);
            listener.Start();
            listener.BeginAcceptTcpClient(OnServerConnect, null);
            Console.WriteLine(DateTime.Now + ")> Server has successfully started!");

            await Task.Delay(-1);

            while (true)
            {

            }
        }
        #endregion

        #region  Application Handler
        protected void OnApplicationQuit(object sender, EventArgs e)
        {
            Console.WriteLine(DateTime.Now + ")> Server is shutting down...");

            listener?.Stop();
            for (int i = 0; i < clientList.Count; i++)
            {
                clientList[i].Close();
            }

            Task.Delay(1000);
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
            if(client != null && clientList != null)
                clientList.Remove(client);
        }

        internal void Send(string message)
        {
            BroadcastChatMessage(message);
            Console.WriteLine(DateTime.Now + ")> " + message);
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

#region Default gateway IP
//Default gateway: 127.0.0.1
#endregion