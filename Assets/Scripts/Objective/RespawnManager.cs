using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

public class RespawnManager : NetworkBehaviour
{

    [SerializeField] private List<GameObject> _AllRespawnPoints;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    void Awake()
    {
        _AllRespawnPoints = new List<GameObject> { };
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void AddRespawnPoint(GameObject RespawnPoint)
    {
        _AllRespawnPoints.Add(RespawnPoint);
    }

    [ServerRpc(RequireOwnership = false)]
    public void GetRespawnPointServerRPC(ServerRpcParams serverRpcParams = default)
    {
        var ClientId = serverRpcParams.Receive.SenderClientId;

        ClientRpcParams clientRpcParams = new ClientRpcParams
        {
            Send = new ClientRpcSendParams
            {
                TargetClientIds = new ulong[] { ClientId }
            }
        };

        GameObject RespawnPoint = _AllRespawnPoints[Random.Range(0, _AllRespawnPoints.Count)];
        NetworkObject Player = NetworkManager.ConnectedClients[ClientId].PlayerObject;
        for (int i = 0; i < 50; i++)
        {
            if (RespawnPoint.GetComponent<RespawnData>()._Team == Player.GetComponent<PlayerTeamManager>()._Team.Value)
            {
                Player.GetComponent<HealthManager>().SetPositionClientRPC(RespawnPoint.transform.position, clientRpcParams);
                return;
            }
            RespawnPoint = _AllRespawnPoints[Random.Range(0, _AllRespawnPoints.Count)];
        }
        Player.GetComponent<HealthManager>().SetPositionClientRPC(RespawnPoint.transform.position, clientRpcParams);
        return;
    }

    public void RemoveSpawnPoint()
    {
        _AllRespawnPoints.Clear();
    }
}
