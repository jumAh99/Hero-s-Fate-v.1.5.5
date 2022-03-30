using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine.SceneManagement;

public class MultiplayerUI : MonoBehaviour
{
    PhotonView view;
    public GameObject chatSystem;
    bool setActive = true;
    public void OnLeave()
    {
        SceneManager.LoadScene("ConnectingScene");
        PhotonNetwork.Disconnect();
        FindObjectOfType<AudioManager>().Play("Button_Pressed_Sound");
        FindObjectOfType<AudioManager>().StopPlaying("Battle_Sound");
        FindObjectOfType<AudioManager>().Play("MainMenu_Sound");
        Debug.LogError("Music stopped.");
    }

    public void ChatToggle()
    {
        chatSystem.SetActive(setActive);
        setActive = !setActive;
    }
    public void OnExit()
    {
        PhotonNetwork.LoadLevel("LobbyScene");
    }
}
