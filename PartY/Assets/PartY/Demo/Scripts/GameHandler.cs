using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// NOTE: This class is entirely example code. It is expected of you to repurpose this system as well as the player controller for your own projects.
/// This script will handle gameplay as well as starting up the lobby to get players in the right positions and with the right references.
/// </summary>
public class GameHandler : MonoBehaviour
{
    #region Data
    public static GameHandler instance;

    public KinematicCharacterController.Examples.ExamplePlayer player;
    public List<CharacterRepresentation> players = new List<CharacterRepresentation>();

    [HideInInspector] public List<GameObject> otherPlayers = new List<GameObject>();
    [HideInInspector] public List<PartY.Player> otherPlayersTargets = new List<PartY.Player>();
    [HideInInspector] public CharacterRepresentation myPlayer = null;

    Vector3 lastPos;
    Quaternion lastRot;
    Vector3 lastScale;
    #endregion

    #region Startup
    private void OnEnable()
    {
        instance = this;

        //Go through the host/clients and find yourself, removing all player controllers in the process if they are not on your player.
        //Additionally set up the example kinematic player handler so that the user can move around.
        if (PartY.LobbyHandler.myLobby.ownerUsername == PartY.LobbyHandler.instance.usernameField.text)
        {
            myPlayer = players[0];
            players[0].gameObject.name = PartY.LobbyHandler.instance.usernameField.text;
            players[0].usernameField.GetComponent<TextMesh>().text = PartY.LobbyHandler.instance.usernameField.text;
            player.Character = players[0].playerController;

            for (int i = 0; i < PartY.LobbyHandler.myLobby.clients.Count; i++)
            {
                players[i + 1].gameObject.name = PartY.LobbyHandler.myLobby.clients[i];
                otherPlayersTargets.Add(new PartY.Player(PartY.LobbyHandler.myLobby.clients[i], players[i + 1].transform.position, players[i + 1].transform.rotation.eulerAngles, players[i + 1].transform.localScale));
                otherPlayers.Add(players[i + 1].gameObject);
                players[i + 1].usernameField.GetComponent<TextMesh>().text = PartY.LobbyHandler.myLobby.clients[i];
                players[i + 1].Cleanup();
            }
        }
        else
        {
            players[0].gameObject.name = PartY.LobbyHandler.myLobby.ownerUsername;
            otherPlayers.Add(players[0].gameObject);
            otherPlayersTargets.Add(new PartY.Player(PartY.LobbyHandler.myLobby.ownerUsername, players[0].transform.position, players[0].transform.rotation.eulerAngles, players[0].transform.localScale));
            players[0].usernameField.GetComponent<TextMesh>().text = PartY.LobbyHandler.myLobby.ownerUsername;
            players[0].Cleanup();

            for (int i = 0; i < PartY.LobbyHandler.myLobby.clients.Count; i++)
            {
                if (PartY.LobbyHandler.myLobby.clients[i] == PartY.LobbyHandler.instance.usernameField.text)
                {
                    Debug.Log("Found local player client");
                    players[i + 1].gameObject.name = PartY.LobbyHandler.instance.usernameField.text;
                    myPlayer = players[i + 1];
                    players[i + 1].usernameField.GetComponent<TextMesh>().text = PartY.LobbyHandler.instance.usernameField.text;
                    player.Character = players[i + 1].playerController;
                }
                else
                {
                    players[i + 1].gameObject.name = PartY.LobbyHandler.myLobby.clients[i];
                    otherPlayersTargets.Add(new PartY.Player(PartY.LobbyHandler.myLobby.clients[i], players[i + 1].transform.position, players[i + 1].transform.rotation.eulerAngles, players[i + 1].transform.localScale));
                    otherPlayers.Add(players[i + 1].gameObject);
                    players[i + 1].usernameField.GetComponent<TextMesh>().text = PartY.LobbyHandler.myLobby.clients[i];
                    players[i + 1].Cleanup();
                }
            }
        }
    }
    #endregion

    #region Gameplay
    void Update()
    {
        #region Payload sender [Send data up to server and ask for data back]
        //  [0]         [1]            [2]         [3]           [4]         [5]          [6]          [7]          [8]      [9]       [10]      [11]
        //[Payload] LobbyHostName, MyPositionX, MyPositionY, MyPositionZ, MyRotationX, MyRotationY, MyRotationZ, MyScaleX, MyScaleY, MyScaleZ, MyUsername
        string payload = "MovementPayload," + PartY.LobbyHandler.myLobby.ownerUsername + "," + myPlayer.gameObject.transform.position.x +
            "," + myPlayer.gameObject.transform.position.y + "," + myPlayer.gameObject.transform.position.z + "," + myPlayer.gameObject.transform.rotation.eulerAngles.x +
                "," + myPlayer.gameObject.transform.rotation.eulerAngles.y + "," + myPlayer.gameObject.transform.rotation.eulerAngles.z + "," + myPlayer.gameObject.transform.localScale.x +
                "," + myPlayer.gameObject.transform.localScale.y + "," + myPlayer.gameObject.transform.localScale.z + "," + PartY.LobbyHandler.instance.usernameField.text;

        PartY.PartY.instance.SendTextData(payload);
        #endregion

        #region Interpolation [Lag reduction]
        //Interpolate player positions, rotations and scales. [Hopefully reduces jitter due to any lag]
        for (int i = 0; i < otherPlayers.Count; i++)
        {
            otherPlayers[i].transform.position = Vector3.Lerp(otherPlayers[i].transform.position, otherPlayersTargets[i].position, 0.1f);
            otherPlayers[i].transform.rotation = Quaternion.Lerp(otherPlayers[i].transform.rotation, Quaternion.Euler(otherPlayersTargets[i].rotation), 0.1f);
            otherPlayers[i].transform.localScale = Vector3.Lerp(otherPlayers[i].transform.localScale, otherPlayersTargets[i].scale, 0.1f);
        }
        #endregion

        #region Position updates [Grabs this frames ending positions to detect if a new payload needs to be pushed up next frame]
        lastPos = myPlayer.transform.position;
        lastRot = myPlayer.transform.rotation;
        lastScale = myPlayer.transform.localScale;
        #endregion
    }
    #endregion

    #region Payload reader [Not used in demo]
    public void RecieveLocationDataPackage(PartY.Player[] playerPayload)
    {
        for(int i = 0; i < playerPayload.Length; i++)
        {
            for (int z = 0; z < otherPlayers.Count; z++)
            {
                if(otherPlayers[z].name == playerPayload[i].username)
                {
                    otherPlayersTargets[z].position = playerPayload[i].position;
                    otherPlayersTargets[z].rotation = playerPayload[i].rotation;
                    otherPlayersTargets[z].scale = playerPayload[i].scale;
                }
            }
        }
    }
    #endregion
}