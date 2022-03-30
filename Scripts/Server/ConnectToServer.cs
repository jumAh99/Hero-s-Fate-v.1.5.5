using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine.SceneManagement;
using TMPro;
using UnityEngine.UI;

public class ConnectToServer : MonoBehaviourPunCallbacks
{
    //set the username for te user
    public InputField usernameInput;
    public TextMeshProUGUI buttonText;
    //connect to server 
    public void OnClickConnect()
    {
        if (usernameInput.text.Length >= 1)
        {
            //store username
            PhotonNetwork.NickName = usernameInput.text;
            print("Username set to: " + usernameInput.text);

            //wait for connection
            buttonText.text = "Connecting. .";

            //sync the photon scxene so both users can see the same screen
            PhotonNetwork.AutomaticallySyncScene = true;
            //connect to the master server
            PhotonNetwork.ConnectUsingSettings();
        }
        FindObjectOfType<AudioManager>().Play("Button_Pressed_Sound");
    }
    //handle disconnections
    public override void OnDisconnected(DisconnectCause cause)
    {
        Debug.LogError("Disconnected from server, reason: " + cause);
    }
    public override void OnConnectedToMaster()
    {
        print("Successfully connected to master ;).");
        SceneManager.LoadScene("LobbyScene");
        PhotonNetwork.JoinLobby();
    }

    public void OnMainMenu()
    {
        PhotonNetwork.Disconnect();
        SceneManager.LoadScene("MainMenu");
        FindObjectOfType<AudioManager>().Play("Button_Pressed_Sound");
    }
}
