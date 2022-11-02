using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RespawnData : MonoBehaviour
{

    [SerializeField] TEAMS _Team;
    private GameObject _ObjectiveManager;

    // Start is called before the first frame update
    void Start()
    {
        _ObjectiveManager = GameObject.Find("ObjectiveManager");
        _ObjectiveManager.GetComponent<RespawnManager>().AddRespawnPoint(gameObject);
        gameObject.transform.GetComponent<Renderer>().material.color = _ObjectiveManager.GetComponent<ObjectiveManager>().GetTeamColour(_Team);
    }

    void Awake()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
