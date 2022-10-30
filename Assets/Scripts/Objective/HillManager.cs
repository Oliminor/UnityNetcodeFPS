using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

public class HillManager : NetworkBehaviour
{

    private enum _States { EMPTY, CONTROLLED, CONTESTED };

    private _States _State;

    private GameObject _NetworkManager;
    private TEAMS _ControllingTeam;
    private List<TEAMS> _TEAMSInHill;

    private float _PointCountdown;

    // Start is called before the first frame update
    void Start()
    {
        _NetworkManager = GameObject.Find("ObjectiveManager");
        _State = _States.EMPTY;
        _TEAMSInHill = new List<TEAMS> { };
        _PointCountdown = 1;
    }

    void Awake()
    {
    }

    // Update is called once per frame
    void Update()
    {
        if (!IsServer) return;
        TEAMS TeamControlling;
        switch (_State)
        {
            case (_States.EMPTY):
                Debug.Log("Empty");
                if (_TEAMSInHill.Count > 0) _State = _States.CONTROLLED;
                break;
            case (_States.CONTROLLED):
                Debug.Log("Controlled by " + _ControllingTeam);
                if (_TEAMSInHill.Count <= 0) { 
                    _State = _States.EMPTY;
                    break;
                }
                TeamControlling = _TEAMSInHill[0];
                foreach(TEAMS Team in _TEAMSInHill)
                {
                    if (TeamControlling != Team)
                    {
                        _State = _States.CONTESTED;
                        break;
                    }
                }
                _ControllingTeam = TeamControlling;
                if (_PointCountdown <= 0)
                {
                    _PointCountdown = 1;
                    Debug.Log("Point Award");
                    _NetworkManager.GetComponent<ObjectiveManager>().AddScoreToTeamServerRPC(1, (int)_ControllingTeam);
                }
                _PointCountdown -= Time.deltaTime;
                break;
            case (_States.CONTESTED):
                Debug.Log("Hill contested");
                TeamControlling = _TEAMSInHill[0];
                foreach (TEAMS Team in _TEAMSInHill)
                {
                    if (TeamControlling != Team)
                    {
                        _State = _States.CONTESTED;
                        return;
                    }
                }
                _State = _States.CONTROLLED;
                break;
        }

    }

    void OnTriggerEnter(Collider Object)
    {
        if (!IsServer) return;
        if (!(Object.gameObject.tag == "Player")) return;
        TEAMS Team = Object.gameObject.GetComponent<PlayerTeamManager>().GetTeam();
        _TEAMSInHill.Add(Team);
    }

    void OnTriggerExit(Collider Object)
    {
        if (!IsServer) return;
        if (!(Object.gameObject.tag == "Player")) return;
        TEAMS Team = Object.gameObject.GetComponent<PlayerTeamManager>().GetTeam();
        _TEAMSInHill.Remove(Team);
    }
}
