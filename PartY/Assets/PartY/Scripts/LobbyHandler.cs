using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

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
    public GameObject _HostedLobby;
    public Text _HostedLobbyTitle;
    public Text _JoinedLobbyTitle;
    public GameObject _JoinedLobby;
    public GameObject lobbyTemplate;
    public GameObject contentObj;
    public GameObject loading;
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
        if (usernameField.text == "" || usernameField.text == null || usernameField.text.Replace(" ", "").Length == 0)
        {
            noUsernamePopup.SetActive(true);
            return;
        }
        else
        {
            if(usernameField.text == hostUsername)
            {
                duplicateUsername.SetActive(true);
                return;
            }
            else
            {
                for(int i = 0; i < lobbies.Count; i++)
                {
                    for(int z = 0; z < lobbies[i].clients.Count; z++)
                    {
                        if (lobbies[i].clients[z] == usernameField.text)
                        {
                            //lobbies[i].clients.Remove(lobbies[i].clients[z]);
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
        PartY.PartY.instance.host.SendTextData("JoinLobby," + hostUsername + "," + usernameField.text + "," + PartY.PartY.instance.clientID);
        myLobby = new Lobby(hostUsername, 1);
        myLobby.clients.Add(usernameField.text);
        PartY.LobbySpawner.updateJoinedLobby = true;

        PartY.LobbySpawner.showLoading = true;
    }
    
    public void JoinedLobby()
    {
        PartY.LobbySpawner.joinLobby = true;

        //Get peeps
        PartY.PartY.instance.host.SendTextData("NeedLobbyNames," + myLobby.ownerUsername);

        joining = false;

        PartY.LobbySpawner.hideLoading = true;
    }

    //Placeholder
    public void Matchmake()
    {
        if (usernameField.text == "" || usernameField.text == null || usernameField.text.Replace(" ", "").Length == 0)
        {
            noUsernamePopup.SetActive(true);
            return;
        }

        //ELO lookup, etc. > Find lobby.
        //Join.
    }

    public void CreateLobby()
    {
        if (usernameField.text == "" || usernameField.text == null || usernameField.text.Replace(" ", "").Length == 0)
        {
            noUsernamePopup.SetActive(true);
            return;
        }

        if(!PartY.PartY.loggedIn)
        {
            Debug.Log("Not logged in!");
            return;
        }

        for(int i = 0; i < lobbies.Count; i++)
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
        PartY.PartY.instance.host.SendTextData("CrLobby," + usernameField.text + "," + PartY.PartY.instance.clientID);
        myLobby = new Lobby(usernameField.text, 1);
        PartY.LobbySpawner.updateHostedLobby = true;
    }

    //Not done
    public void StartLobby()
    {
        //Payload: Start lobby, your name, your name, your id
        PartY.PartY.instance.SendTextData("StartLobby," + usernameField.text + "," + PartY.PartY.instance.clientID);
    }

    //Not done
    public void DoStartLobby()
    {
        _Menu.SetActive(false);
        _HostedLobby.SetActive(false);
        _JoinedLobby.SetActive(false);
    }

    public void LeaveLobby(string hostUsername)
    {
        //Send message to server about closing the lobby (This is to kick out any peeps who joined and properly remove the host)
        //Payload: Leave lobby, host name, your name, your id
        PartY.PartY.instance.SendTextData("LeaveLobby," + hostUsername + "," + usernameField.text + "," + PartY.PartY.instance.clientID);
    }

    public void LeftLobby()
    {
        PartY.LobbySpawner.leaveLobby = true;
        myLobby = null;
        RecreateLobbyUI();
    }

    private void OnApplicationQuit()
    {
        if(_HostedLobby.activeSelf)
        {
            CloseLobby();
        }

        if (_JoinedLobby.activeSelf)
        {
            LeaveLobby(myLobby.ownerUsername);
        }
    }

    public void CloseLobby()
    {
        _Menu.SetActive(true);
        _HostedLobby.SetActive(false);
        _JoinedLobby.SetActive(false);

        //Send message to server about closing the lobby (This is to kick out any peeps who joined and properly remove the host)
        PartY.PartY.instance.SendTextData("ClLobby," + usernameField.text + "," + PartY.PartY.instance.clientID);

        for(int i = 0; i < lobbies.Count; i++)
        {
            if(lobbies[i].ownerUsername == usernameField.text)
            {
                lobbies.Remove(lobbies[i]);
            }
        }

        myLobby = null;

        RecreateLobbyUI();
    }

    public void RecreateLobbyUI()
    {
        PartY.LobbySpawner.needsUpdate = true;
    }
    #endregion
}