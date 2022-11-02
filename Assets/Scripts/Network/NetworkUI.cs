using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using TMPro;

public class NetworkUI : NetworkBehaviour
{
    [SerializeField] TextMeshProUGUI textInputForPlayerName;
    [SerializeField] TextMeshProUGUI _IPInput;
    [SerializeField] TextMeshProUGUI _PortInput;
    public TMP_InputField inputText;
    public TextMeshProUGUI chatText;
    public GameObject chatUI;
    public static NetworkUI instance;

    public string GetPlayerNameFromInput() { return textInputForPlayerName.text; }

    private GameObject _ObjectiveManager;
    private GameObject _NetworkManager;

    void Start()
    {
        _ObjectiveManager = GameObject.Find("ObjectiveManager");
        _NetworkManager = GameObject.Find("NetworkManager");
        instance = this;
        DontDestroyOnLoad(this);
       // Application.targetFrameRate = 120;
    }

    void Update()
    {
        _NetworkManager.GetComponent<UnityTransport>().ConnectionData.Address = "127.0.0.1";//_IPInput.text;
    }

    public void StartHost()
    {
        //_ObjectiveManager.GetComponent<MenuManager>().SetMenuState(MENUSTATES.HOSTSETUP);
        NetworkManager.Singleton.StartHost();
        NetworkManager.Singleton.SceneManager.OnSceneEvent += ProjectNetworkSceneManager.singleton.SceneManager_OnSceneEvent;
    }

    public void StartClient()
    {
        _ObjectiveManager.GetComponent<MenuManager>().SetMenuState(MENUSTATES.CLIENTSETUP);
        NetworkManager.Singleton.StartClient();
        NetworkManager.Singleton.SceneManager.OnSceneEvent += ProjectNetworkSceneManager.singleton.SceneManager_OnSceneEvent;
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
