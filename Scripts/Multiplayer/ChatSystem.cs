using ExitGames.Client.Photon;
using System.Collections.Generic;
using System.Collections;
using Photon.Pun;
using UnityEngine;
using UnityEngine.UI;
using Photon.Realtime;
using TMPro;
using System.Text;
using System.Text.RegularExpressions;
public class ChatSystem : MonoBehaviour
{
    public TMP_InputField messageInputArea;
    public TextMeshProUGUI chatContentArea;
    private List<string> _messages = new List<string>();
    PhotonView _photon;
    private float buildDelay = 0f;
    private int maxMessages = 14;
    private void Start()
    {
        _photon = GetComponent<PhotonView>();
        //limit the number of character possible to input in the chat field
        messageInputArea.characterLimit = 22;
    }
    private void Update()
    {
        if (PhotonNetwork.InRoom)
        {
            chatContentArea.maxVisibleLines = maxMessages;
            if (_messages.Count > maxMessages)
            {
                _messages.RemoveAt(0);
            }
            if (buildDelay < Time.time)
            {
                BuildContent();
                buildDelay = Time.time + 0.25f;
            }
        }
        else if (_messages.Count > 0)
        {
            _messages.Clear();
            chatContentArea.text = "";
        }
    }
    public void SendChat(string messages)
    {
        string newMessages = PhotonNetwork.NickName + ": " + messages;
        _photon.RPC("RPC_AddMessages", RpcTarget.All, newMessages);
    }
    public void SubmitChat()
    {
        string blankCheck = messageInputArea.text;
        blankCheck = Regex.Replace(blankCheck, @"\s", "");
        if (blankCheck == "")
        {
            messageInputArea.ActivateInputField();
            messageInputArea.text = "";
            return;
        }
        SendChat(messageInputArea.text);
        messageInputArea.ActivateInputField();
        messageInputArea.text = "";
    }
    void BuildContent()
    {
        string newContent = "";
        foreach (string s in _messages)
        {
            newContent += s + "\n";
        }
        chatContentArea.text = newContent;
    }
    [PunRPC]
    void RPC_AddMessages(string messages)
    {
        _messages.Add(messages);
    }
}
