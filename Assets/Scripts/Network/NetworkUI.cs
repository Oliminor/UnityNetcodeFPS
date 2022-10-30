using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;
using TMPro;

public class NetworkUI : NetworkBehaviour
{
    [SerializeField] TextMeshProUGUI textInputForPlayerName;
    public static NetworkUI instance;

    public string GetPlayerNameFromInput() { return textInputForPlayerName.text; }

    void Start()
    {
        instance = this;
        Application.targetFrameRate = 120;
    }

    public void StartHost()
    {
        NetworkManager.Singleton.StartHost();
    }

    public void StartClient()
    {
        NetworkManager.Singleton.StartClient();
    }

    public void StartServer()
    {
        NetworkManager.Singleton.StartServer();
    }

    public void Shutdown()
    {
        NetworkManager.Singleton.Shutdown();
    }
}
