﻿using System.Collections;
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

            _UpdateLobby();

            StartCoroutine(UpdateLobby());
        }

        private void _UpdateLobby()
        {
            string externalip = new WebClient().DownloadString("http://icanhazip.com");

            PartY.instance.Send("Heartbeat from: " + externalip);

            print("Lobby updated");
        }
    }
}