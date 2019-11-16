using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class LobbyHandler : MonoBehaviour
{
    public class Lobby
    {
        public string ownerUsername;
        public int lobbySize;

        public Lobby(string owner)
        {
            ownerUsername = owner;
            lobbySize = 0;
        }

        public Lobby(string owner, int _lobbySize)
        {
            ownerUsername = owner;
            lobbySize = _lobbySize;
        }
    }

    public static LobbyHandler instance;

    public Text usernameField;
    public GameObject noUsernamePopup;
    public GameObject _Menu;
    public GameObject _HostedLobby;
    public Text _HostedLobbyTitle;
    public Text _JoinedLobbyTitle;
    public GameObject _JoinedLobby;
    public GameObject lobbyTemplate;
    public GameObject contentObj;

    public static List<Lobby> lobbies = new List<Lobby>();
    public static List<GameObject> lobby_UI = new List<GameObject>();

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

        _Menu.SetActive(false);
        _HostedLobby.SetActive(false);
        _JoinedLobby.SetActive(true);
        PartY.PartY.instance.host.SendTextData("JoinLobby," + usernameField.text + "," + PartY.PartY.instance.clientID);
    }

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

        if(!PartY.PartY.LoggedIn)
        {
            Debug.Log("Not logged in!");
            return;
        }

        //Create lobby logic
        _Menu.SetActive(false);
        _HostedLobby.SetActive(true);
        _HostedLobbyTitle.text = usernameField.text + "'s Lobby";

        //Send message to server about new lobby
        PartY.PartY.instance.host.SendTextData("CrLobby," + usernameField.text + "," + PartY.PartY.instance.clientID);
    }

    public void StartLobby()
    {
        _Menu.SetActive(false);
        _HostedLobby.SetActive(false);
        _JoinedLobby.SetActive(false);

        //Activate something?
        ///////

        //Send message to server about closing the lobby (This is to kick out any peeps who joined and properly remove the host)
        PartY.PartY.instance.SendTextData("StartLobby," + usernameField.text + "," + PartY.PartY.instance.clientID);
    }

    public void LeaveLobby(string hostUsername)
    {
        _Menu.SetActive(true);
        _HostedLobby.SetActive(false);
        _JoinedLobby.SetActive(false);

        //Send message to server about closing the lobby (This is to kick out any peeps who joined and properly remove the host)
        PartY.PartY.instance.SendTextData("LeaveLobby," + usernameField.text + "," + PartY.PartY.instance.clientID);
    }

    public void CloseLobby()
    {
        _Menu.SetActive(true);
        _HostedLobby.SetActive(false);
        _JoinedLobby.SetActive(false);

        //Send message to server about closing the lobby (This is to kick out any peeps who joined and properly remove the host)
        PartY.PartY.instance.SendTextData("ClLobby," + usernameField.text + "," + PartY.PartY.instance.clientID);
    }

    public void RecreateLobbyUI()
    {
        for (int i = 0; i < lobby_UI.Count; i++)
        {
            Destroy(lobby_UI[i]);
        }

        lobby_UI = new List<GameObject>();

        int currentY = 3930;

        for (int i = 0; i < lobbies.Count; i++)
        {
            LobbyData _lobbyTemplate = Instantiate(lobbyTemplate).GetComponent<LobbyData>();
            _lobbyTemplate.transform.parent = contentObj.transform;
            _lobbyTemplate.transform.localPosition = new Vector3(0, currentY, 0);
            _lobbyTemplate.lobbyName.text = lobbies[i].ownerUsername + "'s Lobby";
            _lobbyTemplate.lobbyCount.text = lobbies[i].lobbySize + "/4";
            _lobbyTemplate.lobbyButton.onClick.AddListener(delegate { this.JoinLobby(lobbies[i].ownerUsername); });
            currentY -= 155;
        }
    }
}