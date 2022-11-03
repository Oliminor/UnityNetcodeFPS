using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;
using Unity.Collections;
using UnityEngine.SceneManagement;

public class GLOBALVALUES : NetworkBehaviour
{
    
    public enum GameMode { TDM, KOTH };

    public MODES gameMode; 
    // Start is called before the first frame update
    void Start()
    {
        gameMode = MODES.DEATHMATCH;
        NetworkManager.SceneManager.OnLoadComplete += SceneManager_OnLoadComplete;
    }

    private void SceneManager_OnLoadComplete(ulong clientId, string sceneName, LoadSceneMode loadSceneMode)
    {
        if(IsHost)
        {
            ObjectiveManager.instance.SetMode(0);
            Debug.Log("HELLLLLLLLLLLOOOOOOOOOOOOOOOOOOOO");
        }
    }
    // Update is called once per frame
    void Update()
    {
        
    }
}
