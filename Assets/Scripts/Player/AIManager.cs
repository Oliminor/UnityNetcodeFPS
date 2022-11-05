using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using Unity.Netcode;

public class AIManager : NetworkBehaviour
{

    private enum _AIState { IDLE, DEAD, WALK };

    [SerializeField] NetworkVariable<_AIState> _State = new NetworkVariable<_AIState>();
    private NavMeshAgent _Agent;
    private GameObject _Destination;

    // Start is called before the first frame update
    void Start()
    {
        _Agent = GetComponent<NavMeshAgent>();
        _Destination = GameObject.Find("KingOfTheHill");
        GetComponent<PlayerTeamManager>().ChangeTeam(1);
    }

    void Awake()
    {
        _State.Value = _AIState.WALK;
    }

    // Update is called once per frame

    void FixedUpdate()
    {
        if (!IsServer) return;

        Debug.Log(_State.Value);
        switch (_State.Value)
        {
            case (_AIState.IDLE):
                break;
            case (_AIState.DEAD):
                break;
            case (_AIState.WALK):
               // _Agent.destination = _Destination.transform.position;
                break;
        }
    }

    void Update()
    {
        
    }
}
