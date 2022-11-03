using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using Unity.Netcode;

public class AIManager : NetworkBehaviour
{

    private enum _AIState { IDLE, ENGAGE, WALK, WEAPON, HILL };

    [SerializeField] NetworkVariable<_AIState> _State = new NetworkVariable<_AIState>();
    private NavMeshAgent _Agent;
    private Vector3 _Destination;

    private bool _InHill;

    private GameObject _Objective;
    private GameObject _Target;

    // Start is called before the first frame update
    void Start()
    {
        _Agent = GetComponent<NavMeshAgent>();
        _Objective = GameObject.Find("KingOfTheHill");
    }

    void Awake()
    {
        _Objective = GameObject.Find("KingOfTheHill");
        _State.Value = _AIState.IDLE;
    }

    // Update is called once per frame

    void FixedUpdate()
    {
        if (!IsServer) return;

        Debug.Log(_State.Value);
        switch (_State.Value)
        {
            case (_AIState.IDLE):
                //If see player

                //If Objective
                //If Deathmatch search
                _State.Value = _AIState.HILL;

                //If KOTH defend

                break;
            case (_AIState.ENGAGE):
                _State.Value = _AIState.IDLE;
                transform.LookAt(_Target.transform);
                Debug.Log("Pew pew pew");
                break;
            case (_AIState.WALK):
                //Pick random spot on map and walk there

                //if see player then engage

                //if see better weapon then pickup
                _Agent.destination = _Destination;
                break;
            case (_AIState.WEAPON):

                break;
            case (_AIState.HILL):
                Debug.Log("is in hill" + _InHill);
                //If hill empty then head to hill
                if (_Objective.GetComponent<HillManager>().GetTeamControlling() == -1)
                {
                    _Agent.destination = _Objective.transform.position;
                }
                //if in hill
                else if (_InHill)
                {
                    _Agent.destination = _Objective.transform.position;
                    Debug.Log("I OWN HILL");
                }

                //if hill is hostile then attack
                else if (_Objective.GetComponent<HillManager>().GetTeamControlling() != (int)GetComponent<PlayerTeamManager>()._Team.Value)
                {
                    _Agent.destination = _Objective.transform.position;
                    Debug.Log("Die attackers");
                }
                //if hill is friendly then defend
                else
                {
                    _Agent.destination = transform.position;
                    Debug.Log("Eh they got it");
                }
                //If in hill then stay and defend

                break;
        }
    }

    void OnTriggerEnter(Collider Object)
    {
        if (Object.gameObject == _Objective)
        {
            _InHill = true;
        }
    }

    void OnTriggerExit(Collider Object)
    {
        if (Object.gameObject == _Objective)
        {
            _InHill = false;
        }
    }

    void Update()
    {
        
    }
}
