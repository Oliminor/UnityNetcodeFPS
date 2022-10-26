using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

public class ServerManager : NetworkBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        NetworkManager.Singleton.ConnectionApprovalCallback = ApprovalCheck;
    }

    // Update is called once per frame
    void Update()
    {

    }

    private void ApprovalCheck(NetworkManager.ConnectionApprovalRequest Request, NetworkManager.ConnectionApprovalResponse Response)
    {
        //Response.transform.GetComponent<PlayerTeamManager>().SetTeam(GetComponent<ObjectiveManager>().GetTeamPlayer(0));
    }
}
