using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class LobbyManagement : NetworkBehaviour
{
    int minPlayersRequired = 0;//change this to have a more players needed to start game
    bool isMinimumPlayerReqMet;
    bool areAllPlayersReady;

    bool areYouReadyButtonToggle = false;

    private Dictionary<ulong, bool> connectedClients;
    bool allPlayersJoinedLobby;
    int readyClients;
    int allClients;
    public TMP_Text readyCountText;
    public TMP_Text playersConnectedText;

    public Button readyUpButton;
    public TMP_Text playButtonText;
    public GameObject readyIcon;

    private void Awake()
    {
        readyCountText = GameObject.Find("PlayersLoadedScene").GetComponent<TMP_Text>();
        readyIcon = GameObject.Find("ReadyIcon");
        //readyIcon.GetComponent<RawImage>().color = Color.red;
        playersConnectedText = GameObject.Find("PlayersConnected").GetComponent<TMP_Text>();
        //playButtonText = GameObject.Find("PlayButtonText").GetComponent<TMP_Text>();
        readyUpButton = GameObject.Find("ReadyUp").GetComponent<Button>();
        readyUpButton.onClick.AddListener(() => PlayerIsNowReady());
    }
    public override void OnNetworkSpawn()
    {
        connectedClients = new Dictionary<ulong, bool>();
        areAllPlayersReady = false;
        connectedClients.Add(NetworkManager.LocalClientId, false);
        //playButtonText.color = Color.red;

        if (IsServer)
        {
            isMinimumPlayerReqMet = false;
            NetworkManager.OnClientConnectedCallback += OnClientConnectedCallback;

        }

        base.OnNetworkSpawn();
    }


    private void OnClientConnectedCallback(ulong Id)
    {
        if (IsServer)
        {
            if (!connectedClients.ContainsKey(Id))
            {
                connectedClients.Add(Id, false);
            }
            UpdateReadyClientsInts();
        }
    }

    private void UpdateReadyClientsInts()
    {
        int readyClientsTextInt = 0;
        foreach (var client in connectedClients)
        {
            if (client.Value == true)
            {
                readyClientsTextInt++;
            }
        }
        readyCountText.text = string.Empty;
        readyCountText.text = readyClientsTextInt.ToString();
        playersConnectedText.text = allClients.ToString();
        if (readyClientsTextInt == connectedClients.Count)
        {
            //playButtonText.color = Color.green;
        }
        else
        {
            //playButtonText.color = Color.red;
        }
    }

    private void CheckForAllPlayerJoinedStatus()
    {
        if (connectedClients.Count >= minPlayersRequired)
        {
            isMinimumPlayerReqMet = true;
        }
        else
        {
            isMinimumPlayerReqMet = false;
        }
        foreach (var client in connectedClients)
        {
            ClientReadyClientRpc(client.Key, client.Value);
            if (!NetworkManager.Singleton.ConnectedClients.ContainsKey(client.Key))
            {
                allPlayersJoinedLobby = false;
            }
        }
        CheckIfPlayersAreReady();
    }

    private void CheckIfPlayersAreReady()
    {
        if (isMinimumPlayerReqMet)
        {
            bool areAllPlayersReady = true;
            foreach (var client in connectedClients)
            {
                if (client.Value == false)
                {
                    areAllPlayersReady = false;
                    
                }
            }
            if (areAllPlayersReady)
            {
                NetworkManager.Singleton.OnClientConnectedCallback -= OnClientConnectedCallback;
                if(IsServer)
                {
                    NetworkManager.SceneManager.LoadScene("Ben", LoadSceneMode.Single);
                }
                
                Debug.Log("YES ALL PLAYERS ARE READY");
            }
        }
    }

    [ClientRpc]
    private void ClientReadyClientRpc(ulong Id, bool isReady)
    {
        if (!IsServer)
        {
            if (!connectedClients.ContainsKey(Id))
            {
                connectedClients.Add(Id, isReady);
            }
            else
            {
                connectedClients[Id] = isReady;
            }
            UpdateReadyClientsInts();
        }
    }

    [ServerRpc]
    private void SceneChangeServerRpc()
    {
        if (IsServer)
        {
            ProjectNetworkSceneManager.singleton.SwitchScenes();
        }
    }

    public void PlayerIsNowReady()
    {
        if (areYouReadyButtonToggle == false)
        {
            connectedClients[NetworkManager.Singleton.LocalClientId] = true;
            readyIcon.GetComponent<RawImage>().color = Color.green;
            areYouReadyButtonToggle = true;
        }
        else
        {
            connectedClients[NetworkManager.Singleton.LocalClientId] = false;
            readyIcon.GetComponent<RawImage>().color = Color.red;
            areYouReadyButtonToggle = false;
        }
        if (IsServer)
        {
            CheckForAllPlayerJoinedStatus();
        }
        else
        {
            ClientReadyUpServerRpc(NetworkManager.Singleton.LocalClientId);
        }
        UpdateReadyClientsInts();
    }

    [ServerRpc(RequireOwnership = false)]
    private void ClientReadyUpServerRpc(ulong localClientId)
    {
        if (connectedClients.ContainsKey(localClientId))
        {
            connectedClients[localClientId] = true;
            CheckForAllPlayerJoinedStatus();
            UpdateReadyClientsInts();
        }
    }
    private void Update()
    {
        //Debug.Log("all clients are ready?:" + areAllPlayersReady);
        //Debug.Log("minimum player count met?:" + isMinimumPlayerReqMet);
        //Debug.Log("number of connected clients:" + connectedClients.Count);
        //Debug.Log("number of ready clients:" + readyClients);
        //Debug.Log("number of dictionary entrys:" + connectedClients.Count);

    }
}
