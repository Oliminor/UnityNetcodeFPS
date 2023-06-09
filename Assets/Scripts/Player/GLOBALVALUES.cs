using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;
using Unity.Collections;
using UnityEngine.SceneManagement;

public class GLOBALVALUES : NetworkBehaviour
{

    public MODES gameMode;
    private GameObject _ObjectiveManager;
    // Start is called before the first frame update
    void Start()
    {
        gameMode = MODES.KINGOFTHEHILL;
        NetworkManager.SceneManager.OnLoadComplete += SceneManager_OnLoadComplete;
        _ObjectiveManager = GameObject.Find("ObjectiveManager");
    }

    private void SceneManager_OnLoadComplete(ulong clientId, string sceneName, LoadSceneMode loadSceneMode)
    {
        if(IsServer)
        {
            if(sceneName== ProjectNetworkSceneManager.sceneNames[2])
            {
                _ObjectiveManager = GameObject.Find("ObjectiveManager");
            }
        }
    }
    // Update is called once per frame
    void Update()
    {
        
    }
}
