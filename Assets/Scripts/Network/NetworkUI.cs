using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;
using TMPro;

public class NetworkUI : NetworkBehaviour
{

    [SerializeField] private GameObject chatUI;
    [SerializeField] private TextMeshProUGUI chatText;
    [SerializeField] private TMP_InputField chatInput;
    void Start()
    {
        Application.targetFrameRate = 120;
    }

    public void StartHost()
    {
        NetworkManager.Singleton.StartHost();
        SpawnWeapons();
    }

    public void StartClient()
    {
        NetworkManager.Singleton.StartClient();
    }

    public void StartServer()
    {
        NetworkManager.Singleton.StartServer();
        SpawnWeapons();
    }

    public void Shutdown()
    {
        NetworkManager.Singleton.Shutdown();
    }

    private void SpawnWeapons()
    {
        WeaponSpawner[] weaponSpawners = WeaponSpawner.FindObjectsOfType<WeaponSpawner>();

        for (int i = 0; i < weaponSpawners.Length; i++)
        {
            weaponSpawners[i].SpawnWeaponClientRPC();
        }
    }
    public GameObject GetNetworkUI()
    {
        return gameObject;
    }
    public GameObject GetChatUI()
    {
        return chatUI;
    }
    public TMP_InputField GetInputField()
    {
        return chatInput;
    }
    public TextMeshProUGUI GetChatlog()
    {
        return chatText;
    }
}
