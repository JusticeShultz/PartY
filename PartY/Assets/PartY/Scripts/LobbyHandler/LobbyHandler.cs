using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace PartY
{
    //[Essential script]
    /// <summary>
    /// Handles the high level side of lobbies to allow events and scripts to easily access and do certain things.
    /// </summary>
    public class LobbyHandler : MonoBehaviour
    {
        #region Data
        public class Lobby
        {
            public string ownerUsername;
            public int lobbySize;
            public List<string> clients;

            public Lobby(string owner)
            {
                ownerUsername = owner;
                lobbySize = 1;
                clients = new List<string>();
            }

            public Lobby(string owner, int _lobbySize)
            {
                ownerUsername = owner;
                lobbySize = _lobbySize;
                clients = new List<string>();
            }
        }

        public static LobbyHandler instance;

        public InputField usernameField;
        public GameObject noUsernamePopup;
        public GameObject duplicateUsername;
        public GameObject hostClosedLobbyPopup;
        public GameObject _Menu;
        public GameObject _GameHandler;
        public GameObject _HostedLobby;
        public Text _HostedLobbyTitle;
        public Text _JoinedLobbyTitle;
        public GameObject _JoinedLobby;
        public GameObject lobbyTemplate;
        public GameObject contentObj;
        public GameObject loading;
        public GameObject lobbyFull;
        public Button leaveLobbyButton;
        public RectTransform tempLobbyObj;

        public List<Text> host_joiners = new List<Text>();
        public List<Text> joiners = new List<Text>();

        public static List<Lobby> lobbies = new List<Lobby>();
        public static Lobby myLobby = null;
        public static bool joining = false;
        public static List<GameObject> lobby_UI = new List<GameObject>();
        #endregion

        #region PartY Lobby API
        private void Start()
        {
            instance = this;
        }

        public void JoinLobby(string hostUsername)
        {
            if (usernameField.text == "" || usernameField.text == null || usernameField.text.Replace(" ", "").Length == 0 || usernameField.text.Contains(","))
            {
                noUsernamePopup.SetActive(true);
                return;
            }
            else
            {
                if (usernameField.text == hostUsername)
                {
                    duplicateUsername.SetActive(true);
                    return;
                }
                else
                {
                    for (int i = 0; i < lobbies.Count; i++)
                    {
                        for (int z = 0; z < lobbies[i].clients.Count; z++)
                        {
                            if (lobbies[i].clients[z] == usernameField.text)
                            {
                                duplicateUsername.SetActive(true);
                                return;
                            }
                        }
                    }
                }
            }

            leaveLobbyButton.onClick.AddListener(delegate { LeaveLobby(hostUsername); });

            joining = true;

            //Payload: Join lobby, host name, your name, your id
            PartY.instance.host.SendTextData("JoinLobby," + hostUsername + "," + usernameField.text + "," + PartY.instance.clientID);
            myLobby = new Lobby(hostUsername, 1);
            myLobby.clients.Add(usernameField.text);
            LobbySpawner.updateJoinedLobby = true;

            LobbySpawner.showLoading = true;
        }

        public void JoinedLobby()
        {
            LobbySpawner.joinLobby = true;

            //Get peeps
            PartY.instance.host.SendTextData("NeedLobbyNames," + myLobby.ownerUsername);

            joining = false;

            LobbySpawner.hideLoading = true;
        }

        //Placeholder
        public void Matchmake()
        {
            if (usernameField.text == "" || usernameField.text == null || usernameField.text.Replace(" ", "").Length == 0 || usernameField.text.Contains(","))
            {
                noUsernamePopup.SetActive(true);
                return;
            }

            //ELO lookup, etc. > Find lobby.
            //Join.
        }

        public void CreateLobby()
        {
            if (usernameField.text == "" || usernameField.text == null || usernameField.text.Replace(" ", "").Length == 0 || usernameField.text.Contains(","))
            {
                noUsernamePopup.SetActive(true);
                return;
            }

            if (!PartY.loggedIn)
            {
                Debug.Log("Not logged in!");
                return;
            }

            for (int i = 0; i < lobbies.Count; i++)
            {
                if (usernameField.text == lobbies[i].ownerUsername)
                {
                    duplicateUsername.SetActive(true);
                    return;
                }
            }

            //Create lobby logic
            _Menu.SetActive(false);
            _HostedLobby.SetActive(true);
            _HostedLobbyTitle.text = usernameField.text + "'s Lobby";

            //Send message to server about new lobby
            //Payload: Create lobby, your name, your id
            PartY.instance.host.SendTextData("CrLobby," + usernameField.text + "," + PartY.instance.clientID);
            myLobby = new Lobby(usernameField.text, 1);
            LobbySpawner.updateHostedLobby = true;
        }
        
        public void StartLobby()
        {
            //Payload: Start lobby, your name, your id
            PartY.instance.SendTextData("StartLobby," + usernameField.text + "," + PartY.instance.clientID);
            LobbySpawner.startGame = true;
        }

        public void LeaveLobby(string hostUsername)
        {
            //Send message to server about closing the lobby (This is to kick out any peeps who joined and properly remove the host)
            //Payload: Leave lobby, host name, your name, your id
            PartY.instance.SendTextData("LeaveLobby," + hostUsername + "," + usernameField.text + "," + PartY.instance.clientID);
        }

        public void LeftLobby()
        {
            LobbySpawner.leaveLobby = true;
            myLobby = null;
            RecreateLobbyUI();
        }
        
        public void CloseLobby()
        {
            //Close both the host and joiner lobby screens and open the main menu.
            _Menu.SetActive(true);
            _HostedLobby.SetActive(false);
            _JoinedLobby.SetActive(false);

            //Send message to server about closing the lobby (This is to kick out any peeps who joined and properly remove the host)
            PartY.instance.SendTextData("ClLobby," + usernameField.text + "," + PartY.instance.clientID);

            //Remove the lobby from your side.
            for (int i = 0; i < lobbies.Count; i++)
            {
                if (lobbies[i].ownerUsername == usernameField.text)
                {
                    lobbies.Remove(lobbies[i]);
                }
            }

            //Make sure you no longer have a lobby.
            myLobby = null;

            //Mark the lobby to update itself.
            RecreateLobbyUI();
        }

        public void RecreateLobbyUI()
        {
            //Tell the handler that the lobby UI needs an update.
            LobbySpawner.needsUpdate = true;
        }

        private void OnApplicationQuit()
        {
            //If you are currently hosting. (If this is not done, users will sit in a dead lobby and a ghost lobby will be formed)
            if (_HostedLobby.activeSelf)
            {
                CloseLobby();
            }

            //If you are currently in a lobby. (If this is not done, you will leave behind a ghost in the lobby)
            if (_JoinedLobby.activeSelf)
            {
                LeaveLobby(myLobby.ownerUsername);
            }

            //If you need to do anything upon leaving an active game.
            if(_GameHandler.activeSelf)
            {

            }
        }
        #endregion
    }
}