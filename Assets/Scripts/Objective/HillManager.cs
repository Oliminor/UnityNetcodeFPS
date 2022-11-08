using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

public class HillManager : NetworkBehaviour
{

    private enum _States { EMPTY, CONTROLLED, CONTESTED };

    private _States _State;

    private GameObject _ObjectiveManager;
    private TEAMS _ControllingTeam;
    private List<TEAMS> _TEAMSInHill;
    private bool _GameActive;
    private int _TeamInControl;

    private float _PointCountdown;

    // Start is called before the first frame update
    void Start()
    {
        _ObjectiveManager = GameObject.Find("ObjectiveManager");
        _GameActive = false;
    }

    void Awake()
    {
    }

    public void StartGame()
    {
        _ObjectiveManager = GameObject.Find("ObjectiveManager");
        _TEAMSInHill = new List<TEAMS> { };
        _PointCountdown = 1;
        _State = _States.EMPTY;
    }

    // Update is called once per frame
    void Update()
    {
        if (!IsServer) return;
        HillUpdateServerRPC();

    }
    [ServerRpc]
    void HillUpdateServerRPC()
    {
        Debug.Log("Hill update");
        TEAMS TeamControlling;
        switch (_State)
        {
            case (_States.EMPTY):
                Debug.Log("Empty");
                _TeamInControl = -1;
                if (_TEAMSInHill.Count > 0) _State = _States.CONTROLLED;
                break;
            case (_States.CONTROLLED):
                Debug.Log("Controlled by " + _ControllingTeam);
                if (_TEAMSInHill.Count <= 0)
                {
                    _State = _States.EMPTY;
                    break;
                }
                TeamControlling = _TEAMSInHill[0];
                foreach (TEAMS Team in _TEAMSInHill)
                {
                    if (TeamControlling != Team)
                    {
                        _State = _States.CONTESTED;
                        break;
                    }
                }
                _TeamInControl = (int)TeamControlling;
                _ControllingTeam = TeamControlling;
                if (_PointCountdown <= 0)
                {
                    _PointCountdown = 1;
                    Debug.Log("Point Award");
                    _ObjectiveManager.GetComponent<ObjectiveManager>().AddScoreToTeamServerRPC(1, (int)_ControllingTeam);
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
                        _TeamInControl = -2;
                        return;
                    }
                }
                _State = _States.CONTROLLED;
                break;
        }
    }

    public int GetTeamControlling()
    {
        return _TeamInControl;
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
