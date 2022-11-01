using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;
using TMPro;

public class NetworkUI : NetworkBehaviour
{
    [SerializeField] TextMeshProUGUI textInputForPlayerName;
    public static NetworkUI instance;
    [SerializeField] public GameObject chatUI;
    [SerializeField] public TextMeshProUGUI chatText;
    [SerializeField] public TMP_InputField inputText;

    public string GetPlayerNameFromInput() { return textInputForPlayerName.text; }

    void Start()
    {
        instance = this;
        
        Application.targetFrameRate = 120;
        DontDestroyOnLoad(gameObject);
        chatUI.SetActive(false);
    }

    public void StartHost()
    {
        bool success = false;
        success = NetworkManager.Singleton.StartHost();
        if (success)
        {
            NetworkManager.Singleton.SceneManager.OnSceneEvent += ProjectNetworkSceneManager.singleton.SceneManager_OnSceneEvent;//subscribe to SceneEvents
            chatUI.SetActive(true);
        }
    }

    public void StartClient()
    {
        bool success = false;
        success = NetworkManager.Singleton.StartClient();
        if (success)
        {
            NetworkManager.Singleton.SceneManager.OnSceneEvent += ProjectNetworkSceneManager.singleton.SceneManager_OnSceneEvent; //subscribe to SceneEvents
            chatUI.SetActive(true);
        }
    }
    public void StartGame()
    {
        NetworkManager.SceneManager.LoadScene("Test", UnityEngine.SceneManagement.LoadSceneMode.Single);
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
