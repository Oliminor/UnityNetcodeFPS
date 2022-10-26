using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

public class PlayerTeamManager : NetworkBehaviour
{

    public NetworkVariable<TEAMS> _Team = new NetworkVariable<TEAMS>(0, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner);
    private GameObject _NetworkManager;

    // Start is called before the first frame update
    void Start()
    {

    }

    void Awake()
    {
        _NetworkManager = GameObject.Find("ObjectiveManager");
    }

    // Update is called once per frame
    void Update()
    {
        if (!IsOwner) {
            this.gameObject.transform.GetChild(1).GetComponent<Renderer>().material.color = _NetworkManager.GetComponent<ObjectiveManager>().GetTeamColour(_Team.Value);
        }

        if (Input.GetKeyDown("tab"))
        {
            if (_Team.Value == TEAMS.RED) ChangeTeam(1);
            else ChangeTeam(0);
        }
    }

    public void ChangeTeam(int NewTeam)
    {
        if (!IsOwner) return;
        _Team.Value = (TEAMS)NewTeam;
        _NetworkManager.GetComponent<ObjectiveManager>().SetPlayerToTeamServerRPC(NewTeam);
    }

    public TEAMS GetTeam()
    {
        return _Team.Value;
    }

}
