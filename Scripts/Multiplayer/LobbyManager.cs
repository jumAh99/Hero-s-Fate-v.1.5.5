using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Photon.Pun;
using Photon.Realtime;
using TMPro;
using UnityEngine.SceneManagement;

public class LobbyManager : MonoBehaviourPunCallbacks
{
    public List<PlayerItem> playerList = new List<PlayerItem>();
    public PlayerItem playerPrefab;
    public Transform parentPrefab;

    //!CONNECTION VARIABLES
    public InputField createInput;
    public InputField joinInput;
    public TextMeshProUGUI titleName;
    public TextMeshProUGUI errorMessage;
    public GameObject lobbyPanel;
    public GameObject roomPanel;
    public GameObject playButton;
    /**
        * !CONNECTION FUCNTIONS 
        * !ROOM CREATION 
        * !LOBBY ACCESS*/
    void Start()
    {
        if (PhotonNetwork.NickName != "")
        {
            titleName.text = "Playing as: " + PhotonNetwork.NickName;
        }
        else
        {
            Debug.LogError("No username set.");
            errorMessage.text = "No username set!";
        }
    }
    public void OnCreate()
    {
        FindObjectOfType<AudioManager>().Play("Button_Pressed_Sound");
        if (createInput.text == "")
        {
            Debug.LogError("Value not set for room name.");
            errorMessage.text = "ERROR: Value not set for room name!";
            return;
        }
        else
        {
            //*THE ROOM CAN HOLD ONLY 2 PLAYERS
            PhotonNetwork.CreateRoom(createInput.text, new RoomOptions() { MaxPlayers = 2, BroadcastPropsChangeToAll = true });
            //activate the pannel
            ActivatePannels(true, false);
            errorMessage.text = "";
            //print room status 
            Debug.LogError("Succesfully created room: " + createInput.text);
        }
    }
    void ActivatePannels(bool lobby, bool room)
    {
        //activate the pannel
        roomPanel.SetActive(room);
        lobbyPanel.SetActive(lobby);
    }
    public void OnJoin()
    {
        FindObjectOfType<AudioManager>().Play("Button_Pressed_Sound");
        if (joinInput.text == "")
        {
            Debug.LogError("Value not set for room name.");
            errorMessage.text = "ERROR: Value not set for room name!";
            return;
        }
        else
        {
            PhotonNetwork.JoinRoom(joinInput.text);
            //activate the pannel
            ActivatePannels(true, false);
            //clear error
            errorMessage.text = "";
            Debug.LogError("Succesfully joined room name: " + joinInput.text);
        }
    }
    public override void OnJoinedRoom()
    {
        if (createInput.text != "" || joinInput.text != "")
        {
            ActivatePannels(true, false);
            errorMessage.text = "";
            UpdatePlayerList();
        }
        else
        {
            Debug.LogError("ERROR: Room name mismatch, please check your room names.");
            errorMessage.text = "ERROR: Room name mismatch, please check your room names.";
            return;
        }
    }
    public void OnLeave()
    {
        PhotonNetwork.LeaveRoom();
        FindObjectOfType<AudioManager>().Play("Button_Pressed_Sound");
    }
    public override void OnLeftRoom()
    {
        ActivatePannels(false, true);
    }
    //!UPDATE PLAYER FUNCTION
    public void UpdatePlayerList()
    {
        //delete all player item and spawn new player item 
        foreach (PlayerItem item in playerList)
        {
            Destroy(item.gameObject);
        }

        playerList.Clear();

        //CHECK WEATHER YOU ARE IN A ROOM 
        if (PhotonNetwork.CurrentRoom == null)
        {
            return;
        }
        //loop trough the current players and list them in the slots available 
        foreach (KeyValuePair<int, Player> player in PhotonNetwork.CurrentRoom.Players)
        {
            PlayerItem newPlayerItem = Instantiate(playerPrefab, parentPrefab);
            newPlayerItem.SetPlayerInfo(player.Value);

            if (player.Value == PhotonNetwork.LocalPlayer)
            {
                newPlayerItem.ApplyLocalChanges();
            }

            playerList.Add(newPlayerItem);
        }
    }
    public override void OnPlayerEnteredRoom(Player newPlayer)
    {
        UpdatePlayerList();
    }
    public override void OnPlayerLeftRoom(Player newPlayer)
    {
        UpdatePlayerList();
    }
    private void Update()
    {
        if (PhotonNetwork.IsMasterClient && PhotonNetwork.CurrentRoom.PlayerCount == 2)
        {
            playButton.SetActive(true);
        }
        else
        {
            playButton.SetActive(false);
        }
    }

    public void OnPlayButton()
    {
        //load the game scene trough the network
        PhotonNetwork.LoadLevel("GameSceneMultiplayer");
        FindObjectOfType<AudioManager>().Play("Button_Pressed_Sound");
    }

}
