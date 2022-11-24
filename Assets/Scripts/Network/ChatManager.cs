using System.Collections;
using System;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using Unity.Netcode;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using UnityEngine.SceneManagement;


public class ChatManager : NetworkBehaviour
{
    [SerializeField] public GameObject chatUI;
    [SerializeField] public string playerName;
    [SerializeField] public TextMeshProUGUI chatText;
    [SerializeField] public TMP_InputField chatInput;
    [SerializeField] private Scrollbar scrollBar;
    bool inputActive = false;
    [SerializeField] private GameObject inputFieldObject;
    public static ChatManager singleton;
    private Color teamColour;
    private string teamColorFormat;
    

    private void Awake()
    {
        
        teamColour = Color.gray;
        teamColorFormat = ColorUtility.ToHtmlStringRGB(teamColour);
        inputFieldObject.SetActive(false);

    }
    


    private void Start()
    {
        singleton = this;
        DontDestroyOnLoad(this);
 
        
        
    }
    //public override void OnNetworkSpawn()
    //{
    //    //if (IsClient&&!IsHost)
    //    //{
    //    //    ChangeOwnershipServerRpc(NetworkManager.Singleton.LocalClientId);
    //    //}

    //    if (IsClient)
    //    {
    //        playerName = NetworkUI.instance.playerName;
    //        //chatInput.onDeselect.AddListener(Send);
    //    }
    //    base.OnNetworkSpawn();
    //}

    private void Update()
    {
       
        if (IsClient)
        {
            if (Input.GetKeyDown(KeyCode.Return))
            {
                
                if (inputActive == false)
                {
                    inputFieldObject.SetActive(true);
                    chatInput.ActivateInputField();
                    //chatInput.Select();
                    inputActive = true;
                }
                else
                {
                    inputFieldObject.SetActive(false);
                    Send(chatInput.text);
                    chatInput.text = string.Empty;
                    inputActive = false;

                }
            }
        }
    }

    //send message on pressing enter
    public void Send(string message)
    {
        if (IsClient)
        {
            SendMessageServerRpc(playerName, message, teamColorFormat);
           
        }
    }

    [ServerRpc(RequireOwnership =false)]
    private void SendMessageServerRpc(string playerName, string message, string teamColor)//pass message along with senders name and team colour to server
    {
        HandleMessageClientRpc(playerName, message, teamColor);
        Debug.Log("hello");
    }

    [ClientRpc]
    private void HandleMessageClientRpc(string playerName, string message, string teamColor)// ^^ but from server to clients
    {
        UpdateChat(playerName, message, teamColor);
        Debug.Log(message);
    }


    public void UpdateChat(string playerName, string message, string teamColor) //update the chat log with message and some formatting
    {
        GetComponent<ChatManager>().chatText.text += $"\n<color=#{teamColor}>{playerName}</color> said: {message}";
        Invoke("SetChatToBottom", 0.1f);
        Debug.Log("hit");
    }
    
    public void SetTeamChatColour(Color color)
    {
        teamColour = color;
        teamColorFormat = ColorUtility.ToHtmlStringRGB(teamColour);

    }

    private void SetChatToBottom()
    {
        if (scrollBar.value != 0)
        {
            scrollBar.value = 0;
        }
    }
    public bool GetIsChatActive() 
    { 
        return inputActive; 
    }
}
