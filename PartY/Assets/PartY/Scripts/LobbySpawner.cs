using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace PartY
{
    public class LobbySpawner : MonoBehaviour
    {
        #region Data
        public static bool needsUpdate = false;
        public static bool joinLobby = false;
        public static bool leaveLobby = false;
        public static bool hostClosed = false;
        public static bool updateHostedLobby = false;
        public static bool updateJoinedLobby = false;
        public static bool showLoading = false;
        public static bool hideLoading = false;

        public LobbyHandler handler;
        #endregion

        void Update()
        {
            if (needsUpdate)
            {
                needsUpdate = false;

                for (int i = 0; i < LobbyHandler.lobby_UI.Count; i++)
                {
                    Destroy(LobbyHandler.lobby_UI[i]);
                }

                LobbyHandler.lobby_UI = new List<GameObject>();

                int currentY = 3930;

                for (int i = 0; i < LobbyHandler.lobbies.Count; i++)
                {
                    //print(lobbyTemplate == null);
                    GameObject lobby = Instantiate(handler.lobbyTemplate, Vector3.zero, Quaternion.identity);
                    LobbyData _lobbyTemplate = lobby.GetComponent<LobbyData>();
                    RectTransform rectTransform = lobby.GetComponent<RectTransform>();

                    lobby.transform.name = LobbyHandler.lobbies[i].ownerUsername + "s_Lobby";
                    lobby.transform.SetParent(handler.contentObj.transform);
                    rectTransform.anchoredPosition = new Vector2(0, currentY);
                    _lobbyTemplate.lobbyName.text = LobbyHandler.lobbies[i].ownerUsername + "'s Lobby";
                    _lobbyTemplate.lobbyCount.text = LobbyHandler.lobbies[i].lobbySize + "/4";
                    string temp = LobbyHandler.lobbies[i].ownerUsername;
                    _lobbyTemplate.lobbyButton.onClick.AddListener(delegate { handler.JoinLobby(temp); });

                    currentY -= 155;

                    LobbyHandler.lobby_UI.Add(lobby);

                    print("Lobby generated " + LobbyHandler.lobbies[i].ownerUsername);
                }
            }

            if(joinLobby)
            {
                joinLobby = false;
                
                handler._HostedLobby.SetActive(false);
                handler._JoinedLobby.SetActive(true);

                hideLoading = true;
            }

            if (leaveLobby)
            {
                leaveLobby = false;

                handler._Menu.SetActive(true);
                handler._HostedLobby.SetActive(false);
                handler._JoinedLobby.SetActive(false);
                LobbyHandler.myLobby = null;
            }

            if (hostClosed)
            {
                hostClosed = false;
                handler.hostClosedLobbyPopup.SetActive(true);
            }

            if (showLoading)
            {
                handler._Menu.SetActive(false);

                showLoading = false;

                handler.loading.SetActive(true);
            }

            if (hideLoading)
            {
                hideLoading = false;

                handler.loading.SetActive(false);
            }

            if (updateHostedLobby)
            {
                updateHostedLobby = false;

                handler.host_joiners[0].text = LobbyHandler.myLobby == null ? "Empty" : LobbyHandler.myLobby.ownerUsername;

                if(LobbyHandler.myLobby != null)
                {
                    handler._HostedLobbyTitle.text = LobbyHandler.myLobby.ownerUsername + "'s Lobby";

                    PartY.instance.host.SendTextData("GetLobbyData," + LobbyHandler.myLobby.ownerUsername);
                }

                try
                {
                    handler.host_joiners[1].text = LobbyHandler.myLobby == null ? "Empty" : LobbyHandler.myLobby.clients[0];
                }
                catch (System.ArgumentOutOfRangeException)
                {
                    handler.host_joiners[1].text = "Empty";
                }

                try
                {
                    handler.host_joiners[2].text = LobbyHandler.myLobby == null ? "Empty" : LobbyHandler.myLobby.clients[1];
                }
                catch (System.ArgumentOutOfRangeException)
                {
                    handler.host_joiners[2].text = "Empty";
                }

                try
                {
                    handler.host_joiners[3].text = LobbyHandler.myLobby == null ? "Empty" : LobbyHandler.myLobby.clients[2];
                }
                catch (System.ArgumentOutOfRangeException)
                {
                    handler.host_joiners[3].text = "Empty";
                }

                #region Highlight name
                if (handler.host_joiners[0].text == handler.usernameField.text)
                    handler.host_joiners[0].color = new Color(1, 0.8265371f, 0.2971698f);
                else
                    handler.host_joiners[0].color = new Color(0.1960784f, 0.1960784f, 0.1960784f);

                if (handler.host_joiners[1].text == handler.usernameField.text)
                    handler.host_joiners[1].color = new Color(1, 0.8265371f, 0.2971698f);
                else
                    handler.host_joiners[1].color = new Color(0.1960784f, 0.1960784f, 0.1960784f);

                if (handler.host_joiners[2].text == handler.usernameField.text)
                    handler.host_joiners[2].color = new Color(1, 0.8265371f, 0.2971698f);
                else
                    handler.host_joiners[2].color = new Color(0.1960784f, 0.1960784f, 0.1960784f);

                if (handler.host_joiners[3].text == handler.usernameField.text)
                    handler.host_joiners[3].color = new Color(1, 0.8265371f, 0.2971698f);
                else
                    handler.host_joiners[3].color = new Color(0.1960784f, 0.1960784f, 0.1960784f);
                #endregion
            }

            if (updateJoinedLobby)
            {
                updateJoinedLobby = false;

                handler.joiners[0].text = LobbyHandler.myLobby == null ? "Empty" : LobbyHandler.myLobby.ownerUsername;

                if (LobbyHandler.myLobby != null)
                {
                    handler._JoinedLobbyTitle.text = LobbyHandler.myLobby.ownerUsername + "'s Lobby";

                    PartY.instance.host.SendTextData("GetLobbyData," + LobbyHandler.myLobby.ownerUsername);
                }

                try
                {
                    handler.joiners[1].text = LobbyHandler.myLobby == null ? "Empty" : LobbyHandler.myLobby.clients[0];
                }
                catch (System.ArgumentOutOfRangeException)
                {
                    handler.joiners[1].text = "Empty";
                }

                try
                {
                    handler.joiners[2].text = LobbyHandler.myLobby == null ? "Empty" : LobbyHandler.myLobby.clients[1];
                }
                catch (System.ArgumentOutOfRangeException)
                {
                    handler.joiners[2].text = "Empty";
                }

                try
                {
                    handler.joiners[3].text = LobbyHandler.myLobby == null ? "Empty" : LobbyHandler.myLobby.clients[2];
                }
                catch (System.ArgumentOutOfRangeException)
                {
                    handler.joiners[3].text = "Empty";
                }

                #region Highlight name
                if (handler.joiners[0].text == handler.usernameField.text)
                    handler.joiners[0].color = new Color(1, 0.8265371f, 0.2971698f);
                else
                    handler.joiners[0].color = new Color(0.1960784f, 0.1960784f, 0.1960784f);

                if (handler.joiners[1].text == handler.usernameField.text)
                    handler.joiners[1].color = new Color(1, 0.8265371f, 0.2971698f);
                else
                    handler.joiners[1].color = new Color(0.1960784f, 0.1960784f, 0.1960784f);

                if (handler.joiners[2].text == handler.usernameField.text)
                    handler.joiners[2].color = new Color(1, 0.8265371f, 0.2971698f);
                else
                    handler.joiners[2].color = new Color(0.1960784f, 0.1960784f, 0.1960784f);

                if (handler.joiners[3].text == handler.usernameField.text)
                    handler.joiners[3].color = new Color(1, 0.8265371f, 0.2971698f);
                else
                    handler.joiners[3].color = new Color(0.1960784f, 0.1960784f, 0.1960784f);
                #endregion
            } 
        }
    }
}