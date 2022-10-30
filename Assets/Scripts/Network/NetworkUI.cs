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

    private GameObject _ObjectiveManager;

    void Start()
    {
        _ObjectiveManager = GameObject.Find("ObjectiveManager");
        instance = this;
        Application.targetFrameRate = 120;
    }

    public void StartHost()
    {
        _ObjectiveManager.GetComponent<MenuManager>().SetMenuState(MENUSTATES.HOSTSETUP);
        NetworkManager.Singleton.StartHost();
    }

    public void StartClient()
    {
        _ObjectiveManager.GetComponent<MenuManager>().SetMenuState(MENUSTATES.CLIENTSETUP);
        NetworkManager.Singleton.StartClient();
    }

    public void StartServer()
    {
        _ObjectiveManager.GetComponent<MenuManager>().SetMenuState(MENUSTATES.HOSTSETUP);
        NetworkManager.Singleton.StartServer();
    }

    public void Shutdown()
    {
        NetworkManager.Singleton.Shutdown();
    }
}
