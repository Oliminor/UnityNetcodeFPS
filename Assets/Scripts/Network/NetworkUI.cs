using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using TMPro;
using UnityEngine.SceneManagement;

public class NetworkUI : NetworkBehaviour
{
    [SerializeField] TextMeshProUGUI textInputForPlayerName;
    [SerializeField] TextMeshProUGUI _IPInput;
    [SerializeField] TextMeshProUGUI _PortInput;
    public TMP_InputField inputText;
    public TextMeshProUGUI chatText;
    public GameObject chatUI;
    public static NetworkUI instance;
    public string playerName;
    [SerializeField] GameObject initUI;
    [SerializeField] GameObject HostUI;

    public string GetPlayerNameFromInput() { return textInputForPlayerName.text; }

    private GameObject _ObjectiveManager;
    private GameObject _NetworkManager;

    void Start()
    {
        //_ObjectiveManager = GameObject.Find("ObjectiveManager");
        HostUI.SetActive(false);
        _NetworkManager = GameObject.Find("NetworkManager");
        instance = this;
        DontDestroyOnLoad(this);
       // Application.targetFrameRate = 120;
    }

    void Update()
    {
        //_NetworkManager.GetComponent<UnityTransport>().ConnectionData.Address = "127.0.0.1";//_IPInput.text;
    }

    public void StartHost()
    {
        //_ObjectiveManager.GetComponent<MenuManager>().SetMenuState(MENUSTATES.HOSTSETUP);
        initUI.SetActive(false);
        HostUI.SetActive(true);
        NetworkManager.Singleton.StartHost();
        playerName = GetPlayerNameFromInput();
        ChatManager.singleton.playerName = playerName;
        //ObjectiveManager.instance.StartNewGame();
        NetworkManager.Singleton.SceneManager.OnSceneEvent += ProjectNetworkSceneManager.singleton.SceneManager_OnSceneEvent;
        NetworkManager.Singleton.SceneManager.OnLoadEventCompleted +=ChatManager.singleton.SceneManagement_OnLoadEventCompleted;
    }

    public void StartClient()
    {
        //_ObjectiveManager.GetComponent<MenuManager>().SetMenuState(MENUSTATES.CLIENTSETUP);
        initUI.SetActive(false);
        NetworkManager.Singleton.StartClient();
        playerName = GetPlayerNameFromInput();
        ChatManager.singleton.playerName = playerName;
        NetworkManager.Singleton.SceneManager.OnSceneEvent += ProjectNetworkSceneManager.singleton.SceneManager_OnSceneEvent;
    }

    public void StartServer()
    {
        //_ObjectiveManager.GetComponent<MenuManager>().SetMenuState(MENUSTATES.HOSTSETUP);
        initUI.SetActive(false);
        HostUI.SetActive(true);
        NetworkManager.Singleton.StartServer();
        NetworkManager.Singleton.SceneManager.OnSceneEvent += ProjectNetworkSceneManager.singleton.SceneManager_OnSceneEvent;
    }

    public void Shutdown()
    {
        initUI.SetActive(true);
        NetworkManager.Singleton.Shutdown();
        NetworkManager.SceneManager.LoadScene("Ollie", LoadSceneMode.Single);
    }
   
}
