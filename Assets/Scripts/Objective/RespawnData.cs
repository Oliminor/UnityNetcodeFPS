using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;
using UnityEngine.SceneManagement;

public class RespawnData : MonoBehaviour
{

    [SerializeField] TEAMS _Team;
    private GameObject _ObjectiveManager;

    // Start is called before the first frame update
    void Start()
    {
    }

    void Awake()
    {
        AddSpawn();
    }

    void AddSpawn()
    {
        _ObjectiveManager = GameObject.Find("ObjectiveManager");
        _ObjectiveManager.GetComponent<RespawnManager>().AddRespawnPoint(gameObject);
        gameObject.transform.GetComponent<Renderer>().material.color = _ObjectiveManager.GetComponent<ObjectiveManager>().GetTeamColour(_Team);
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    
}
