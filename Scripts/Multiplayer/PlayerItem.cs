using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using TMPro;
using UnityEngine.UI;

public class PlayerItem : MonoBehaviourPunCallbacks
{
    public TextMeshProUGUI playerName;
    public GameObject leftArrow;
    public GameObject rightArrow;
    ExitGames.Client.Photon.Hashtable playerProperties = new ExitGames.Client.Photon.Hashtable();
    public Image playerAvatar;
    public Sprite[] avatarList;
    Player player;
    public void SetPlayerInfo(Player player)
    {
        playerName.text = player.NickName;
        this.player = player;
        UpdatePlayerItem(this.player);
    }
    public void ApplyLocalChanges()
    {
        rightArrow.SetActive(true);
        leftArrow.SetActive(true);
    }
    public void OnClickLeft()
    {
        if ((int)playerProperties["playerAvatar"] == 0)
        {
            playerProperties["playerAvatar"] = avatarList.Length - 1;
        }
        else
        {
            playerProperties["playerAvatar"] = (int)playerProperties["playerAvatar"] - 1;
        }
        PhotonNetwork.SetPlayerCustomProperties(playerProperties);
        FindObjectOfType<AudioManager>().Play("Button_Pressed_Sound");
    }
    public void OnClickRight()
    {
        if ((int)playerProperties["playerAvatar"] == avatarList.Length - 1)
        {
            playerProperties["playerAvatar"] = 0;
        }
        else
        {
            playerProperties["playerAvatar"] = (int)playerProperties["playerAvatar"] + 1;
        }
        PhotonNetwork.SetPlayerCustomProperties(playerProperties);
        FindObjectOfType<AudioManager>().Play("Button_Pressed_Sound");
    }
    public override void OnPlayerPropertiesUpdate(Player player, ExitGames.Client.Photon.Hashtable playerProperties)
    {
        if (this.player == player)
        {
            UpdatePlayerItem(player);
        }
    }
    void UpdatePlayerItem(Player player)
    {
        if (player.CustomProperties.ContainsKey("playerAvatar"))
        {
            //change the sprite real time in the local machine of the player
            playerAvatar.sprite = avatarList[(int)player.CustomProperties["playerAvatar"]];
            //logically store the player avatar in the photon network
            playerProperties["playerAvatar"] = (int)player.CustomProperties["playerAvatar"];
        }
        else
        {
            playerProperties["playerAvatar"] = 0;
        }
    }
}
