using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

//formed with the use of the netcode bitesize project examples


public class LobbyManagement : NetworkBehaviour
{
    int minPlayersRequired = 0;//change this to have a more players needed to start game
    bool isMinimumPlayerReqMet;
    bool areYouReadyButtonToggle = false;
    private Dictionary<ulong, bool> connectedClients;
    int allClients;
    public TMP_Text readyCountText;
    public TMP_Text playersConnectedText;

    public Button readyUpButton;
    public TMP_Text playButtonText;
    public GameObject readyIcon;

    private void Awake() //just references
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
        connectedClients = new Dictionary<ulong, bool>(); // dictionary of clients and their ready value (true= client ready, false=client not ready) 

        connectedClients.Add(NetworkManager.LocalClientId, false);//first onnetworkspawn is the time the host joins, so add the host to the dictionary 
        //playButtonText.color = Color.red;

        if (IsServer)
        {
            isMinimumPlayerReqMet = false;
            NetworkManager.OnClientConnectedCallback += OnClientConnectedCallback; //subscribe to connection events (importance clear later)
            NetworkManager.OnClientDisconnectCallback += OnClientDisonnectCallback;// ^^

            foreach (var client in NetworkManager.Singleton.ConnectedClientsIds) //iterate through the list of connected clients
            {                                                                   // if the client isnt already an entry in the dict, add them
                if (!connectedClients.ContainsKey(client))                     //  of course, upon join they have not readied, so initialise the dict value as false
                {                                                             //   while no clients joined on first spawn, this is important once a game has been played and the players return the the room where this script exists- there will be no connection callbacks so the dictionary needs to be formed with the list of already connected clients
                    connectedClients.Add(client, false);
                }
                UpdateReadyClientsInts();  // update the UI values
            }


        }

        base.OnNetworkSpawn();
    }

    private void OnClientDisonnectCallback(ulong Id) // the disconnect event subbed to earlier, upon a client disconnecting, if the dict contains a key that is their specfic ID, remove that entry
    {
        if (IsServer)
        {
            if (connectedClients.ContainsKey(Id))
            {
                connectedClients.Remove(Id);
            }
            UpdateReadyClientsInts(); //update UI values
        }
    }

    private void OnClientConnectedCallback(ulong Id) //similar to the above just inverse, upon new client joining, if they are not in the dict, add them
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
        foreach (var client in connectedClients) //iterate through dictionary entries and increment int by 1 for each true value
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

    private void CheckForAllPlayerJoinedStatus() //min player requirement currently set to 0 so beginning of function is practically pointless, but the idea is if it were 4, and there was only 3 players, the value would be false and the function called at the end of this does nothings
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
                if (client.Value == false) //if any value is false then all players arent ready, so set the bool to false
                {
                    areAllPlayersReady = false;
                    
                }
            }
            if (areAllPlayersReady) // if there is no false value, then continue to unsubscribe to the callback and load the game scene 
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
            if (!connectedClients.ContainsKey(Id)) //safety check, add if client doesnt exist as entry 
            {
                connectedClients.Add(Id, isReady);
            }
            else
            {
                connectedClients[Id] = isReady; //else, set the value
            }
            UpdateReadyClientsInts();
        }
    }


    public void PlayerIsNowReady()
    {
        if (areYouReadyButtonToggle == false) //beginning is just visuals, toggle the ui red or green dependant on your ready status, as well as toggling the dictionary values 
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
            CheckForAllPlayerJoinedStatus(); //check to see if players ready 
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

}

