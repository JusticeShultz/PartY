using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class LobbyHandler : MonoBehaviour
{
    public Text usernameField;
    public GameObject noUsernamePopup;

    public void JoinLobby(int lobbyID)
    {
        if (usernameField.text == "" || usernameField.text == null || usernameField.text.Replace(" ", "").Length == 0)
        {
            noUsernamePopup.SetActive(true);
            return;
        }
    }

    public void Matchmake(int lobbyID)
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

        //Create lobby logic
        //Join empty lobby
        //Send message to server about new lobby
        //Send all connected users a message about the new lobby
    }
}
