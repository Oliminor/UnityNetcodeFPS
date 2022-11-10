using System.Collections;
using System;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using Unity.Netcode;
using UnityEngine.SceneManagement;


public class ChatManager : NetworkBehaviour
{
    [SerializeField] public GameObject chatUI;
    [SerializeField] public string playerName;
    [SerializeField] public TextMeshProUGUI chatText;
    [SerializeField] public TMP_InputField chatInput;
    bool inputActive = false;
    public static ChatManager singleton;
    private Color teamColour;
    private string teamColorFormat;
    GameObject objectiveManager;

    private void Awake()
    {
        
        teamColour = Color.gray;
        teamColorFormat = ColorUtility.ToHtmlStringRGB(teamColour);
    }
    


    private void SceneManagement_OnLoadComplete(ulong clientId, string sceneName, LoadSceneMode loadSceneMode)
    {
        if(sceneName=="Ben")
        {
            objectiveManager = GameObject.Find("ObjectiveManager");
            
        }
        
    }

    public void SceneManagement_OnLoadEventCompleted(string sceneName, LoadSceneMode loadSceneMode, List<ulong> clientsCompleted, List<ulong> clientsTimedOut)
    {
        if(sceneName=="Ben")
        {

            objectiveManager = GameObject.Find("ObjectiveManager");
            var team = objectiveManager.GetComponent<PlayerTeamManager>()._Team.Value;
            teamColour=objectiveManager.GetComponent<ObjectiveManager>().GetTeamColour(team);
            teamColorFormat= ColorUtility.ToHtmlStringRGB(teamColour);

        }        
        
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
            SendMessageServerRpc(playerName, message, teamColorFormat);
            chatInput.text = string.Empty;
        }
    }

    [ServerRpc(RequireOwnership =false)]
    private void SendMessageServerRpc(string playerName, string message, string teamColor)
    {
        HandleMessageClientRpc(playerName, message, teamColor);
        Debug.Log("hello");
    }

    [ClientRpc]
    private void HandleMessageClientRpc(string playerName, string message, string teamColor)
    {
        UpdateChat(playerName, message, teamColor);
        Debug.Log(message);
    }

    [ServerRpc]
    private void ChangeOwnershipServerRpc(ulong clientID)
    {
            gameObject.GetComponent<NetworkObject>().ChangeOwnership(clientID);
        
    }

    public void UpdateChat(string playerName, string message, string teamColor)
    {
        GetComponent<ChatManager>().chatText.text += $"\n<color={teamColor}>{playerName}</color> said: {message}";
        Debug.Log("hit");
    }

}
