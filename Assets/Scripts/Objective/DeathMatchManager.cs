using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

public class DeathMatchManager : NetworkBehaviour
{

    private GameObject _NetworkManager;

    // Start is called before the first frame update
    void Start()
    {
        _NetworkManager = GameObject.Find("ObjectiveManager");
        if (_NetworkManager.GetComponent<ObjectiveManager>().GetMode() == MODES.DEATHMATCH)
        {
            
        }
        else
        {
            gameObject.SetActive(false);
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (!IsServer) return;

    }
}
