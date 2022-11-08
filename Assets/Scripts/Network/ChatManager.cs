using System.Collections;
using System;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using Unity.Netcode;


public class ChatManager : NetworkBehaviour
{
    [SerializeField] public GameObject chatUI;
    [SerializeField] public string playerName;
    [SerializeField] public TextMeshProUGUI chatText;
    [SerializeField] public TMP_InputField chatInput;
    bool inputActive = false;

    private void Start()
    {
        
    }
    public override void OnNetworkSpawn()
    {
        if (IsOwner)
        {
            chatText = NetworkUI.instance.chatText;
            chatInput = NetworkUI.instance.inputText;
            chatUI = NetworkUI.instance.chatUI;
            playerName = NetworkUI.instance.GetPlayerNameFromInput();
            //chatInput.onDeselect.AddListener(Send);
        }
        base.OnNetworkSpawn();
    }

    private void Update()
    {
        if(IsOwner)
        {
            if (Input.GetKeyDown(KeyCode.Return))
            {
                if (inputActive == false)
                {
                    chatInput.ActivateInputField();
                    inputActive = true;
                }
                else
                {
                    chatInput.DeactivateInputField();
                    Send(chatInput.text);
                    inputActive = false;

                }
            }
        }
    }

    //Called from the InputField
    public void Send(string message)
    {
        if (IsClient)
        {
            SendMessageServerRpc(playerName, message);
            chatInput.text = string.Empty;
        }
    }

    [ServerRpc]
    private void SendMessageServerRpc(string playerName, string message)
    {
        HandleMessageClientRpc(playerName, message);
        Debug.Log("hello");
    }

    [ClientRpc]
    private void HandleMessageClientRpc(string playerName, string message)
    {
        UpdateChat(playerName, message);
        Debug.Log(message);
    }



    public void UpdateChat(string playerName, string message)
    {
        NetworkManager.Singleton.LocalClient.PlayerObject.GetComponent<ChatManager>().chatText.text += $"\n<color=#62BDE9FF>{playerName}</color> said: {message}";
        Debug.Log("hit");
    }

}
