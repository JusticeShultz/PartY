using System;
using System.Collections.Generic;
using System.Collections;
using System.Runtime.Serialization;
using System.Net.Sockets;
using System.Net;
using System.IO;
using System.Reflection;
using System.Threading.Tasks;
using System.IO;
using System.Text;
using static PartY_HostSolution.Types;

namespace PartY
{
    public struct Transform
    {
        public Vector3 position { get; set; }
        public Vector3 rotation { get; set; }
        public Vector3 scale { get; set; }

        public Transform(int garbage) { position = Vector3.zero; rotation = Vector3.zero; scale = Vector3.zero; }

        public Transform(Vector3 pos, Vector3 rot, Vector3 sca)
        {
            position = pos;
            rotation = rot;
            scale = sca;
        }

        public void Set(Transform incoming)
        {
            position = incoming.position;
            rotation = incoming.rotation;
            scale = incoming.scale;
        }
    }

    public class User
    {
        public PartY_ConnectedClient clientConnection;
        public string username;

        public Transform transform;

        public User(PartY_ConnectedClient cConnection, string userName)
        {
            clientConnection = cConnection;
            username = userName;
            transform = new Transform();
        }
    }

    public class Lobby
    {
        public User host;
        public List<User> usersConnected = new List<User>();

        public Lobby()
        {

        }

        public Lobby(User _host)
        {
            host = _host;
        }

        public void StartLobby()
        {
            host.transform.position = new Vector3(0, 1.318f, 10);

            if(usersConnected.Count > 0)
                usersConnected[0].transform.position = new Vector3(10, 1.318f, 0);

            if (usersConnected.Count > 1)
                usersConnected[1].transform.position = new Vector3(-10, 1.318f, 0);

            if (usersConnected.Count > 2)
                usersConnected[2].transform.position = new Vector3(0, 1.318f, -10);
        }

        public void UpdateLobby(Transform _host)
        {
            host.transform.Set(_host);
        }

        public void UpdateLobby(Transform _host, Transform _user1)
        {
            host.transform.Set(_host);
            usersConnected[0].transform.Set(_user1);
        }

        public void UpdateLobby(Transform _host, Transform _user1, Transform _user2)
        {
            host.transform.Set(_host);
            usersConnected[0].transform.Set(_user1);
            usersConnected[1].transform.Set(_user2);
        }

        public void UpdateLobby(Transform _host, Transform _user1, Transform _user2, Transform _user3)
        {
            host.transform.Set(_host);
            usersConnected[0].transform.Set(_user1);
            usersConnected[1].transform.Set(_user2);
            usersConnected[2].transform.Set(_user3);
        }
    }

    class Program
    {
        #region Data
        public static List<ulong> existingKeys = new List<ulong>();

        public const int port = 27015;

        public static Program instance;

        public IPAddress serverIp;

        public List<PartY_ConnectedClient> clientList = new List<PartY_ConnectedClient>();

        public List<Lobby> lobbyList = new List<Lobby>();

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
            #region Key auth

            ReadTokens();

            #endregion

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
            try
            {
                TcpClient tcpClient = listener.EndAcceptTcpClient(ar);
                tcpClient.GetStream();
                clientList.Add(new PartY_ConnectedClient(tcpClient));

                listener.BeginAcceptTcpClient(OnServerConnect, null);
            }
            catch (System.ObjectDisposedException)
            {
                return;
            }
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

        public void ReadTokens()
        {
            if (!File.Exists("KeyDictionary.PartY"))
            {
                // Create the file.
                using (FileStream fs = File.Create("KeyDictionary.PartY"))
                {
                    Byte[] info =
                        new UTF8Encoding(true).GetBytes("");

                    // Add some information to the file.
                    fs.Write(info, 0, info.Length);
                }

                Console.WriteLine("Generated a KeyDictionary.PartY file");
            }

            // Open the stream and read it back.
            using (StreamReader sr = File.OpenText("KeyDictionary.PartY"))
            {
                string s = "";

                existingKeys = new List<ulong>();

                while ((s = sr.ReadLine()) != null)
                {
                    ulong input;
                    bool didParse = ulong.TryParse(s, out input);

                    if (didParse)
                        existingKeys.Add(input);
                    else Console.WriteLine("Error parsing " + s + " inside of the existing keys dictionary");
                }
            }
        }

        public void RewriteTokens()
        {
            if (!File.Exists("KeyDictionary.PartY"))
            {
                // Create the file.
                using (FileStream fs = File.Create("KeyDictionary.PartY"))
                {
                    Byte[] info =
                        new UTF8Encoding(true).GetBytes("");

                    // Add some information to the file.
                    fs.Write(info, 0, info.Length);
                }

                Console.WriteLine("Generated a KeyDictionary.PartY file, didn't successfully write any data.");
            }

            // Open the stream and read it back.

            string contents = "";

            for(int i = 0; i < existingKeys.Count; i++)
            {
                contents += existingKeys[i].ToString() + Environment.NewLine;
            }

            File.WriteAllText("KeyDictionary.PartY", contents);
        }
        #endregion
    }
}

#region Default gateway IP
//Default gateway: 127.0.0.1
#endregion