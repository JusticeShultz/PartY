using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Net;

namespace PartY
{
    public class AutoUpdateLobby : MonoBehaviour
    {
        public float UpdateRate = 5.0f;

        void Start()
        {
            StartCoroutine(UpdateLobby());
        }

        public void ForceUpdateLobby()
        {
            _UpdateLobby();
        }

        IEnumerator UpdateLobby()
        {
            yield return new WaitForSeconds(UpdateRate);

            if(PartY.loggedIn && PartY.IsConnectedToServer)
                _UpdateLobby();

            StartCoroutine(UpdateLobby());
        }

        private void _UpdateLobby()
        {
            string externalip = new WebClient().DownloadString("http://icanhazip.com");

            if (PartY.instance.host != null)
            {
                PartY.instance.SendTextData("Heartbeat from: " + externalip);
            }

            if (LobbyHandler.myLobby != null)
            {
                if (LobbyHandler.instance.usernameField.text == LobbyHandler.myLobby.ownerUsername)
                {
                    LobbySpawner.updateHostedLobby = true;
                }
                else
                {
                    LobbySpawner.updateJoinedLobby = true;
                }
            }

            //print("Lobby updated");
        }
    }
}