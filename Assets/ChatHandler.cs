using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using Unity.Netcode;


public class ChatHandler: NetworkBehaviour
{
    [SerializeField] private GameObject chatUI;
    [SerializeField] private NetworkUI networkUI;
    [SerializeField] private TextMeshProUGUI chatText;
    [SerializeField] private TMP_InputField chatInput;

    private void Start()
    {
        if (IsOwner)
        {
            networkUI = NetworkUI.FindObjectOfType<NetworkUI>();
            //chatInput=networkUI.GetInputField();
            //chatUI=networkUI.GetChatUI();
            //chatText=networkUI.GetChatlog();
            //chatUI.SetActive(true);
        }
    }

    //Called from the InputField
    public void Send(string message)
    {
        if (IsClient)
        {
            if (!Input.GetKeyDown(KeyCode.Return) || string.IsNullOrWhiteSpace(message))
            {
                return;
            }

            //SendMessageServerRpc(message);
            chatText.text += $"\n{message}";
            //chatInput.text = string.Empty;
        }
        chatText.GetComponent<TMP_Text>().text += $"\n{message}";
        Debug.Log(message);
    }

    [ServerRpc]
    private void SendMessageServerRpc(string message)
    {
        HandleMessageClientRpc(message);
    }

    [ClientRpc]
    private void HandleMessageClientRpc(string message)
    {
        UpdateChat(message);
    }



    public void UpdateChat(string message)
    {
        chatText.text += $"\n{message}";
    }

}