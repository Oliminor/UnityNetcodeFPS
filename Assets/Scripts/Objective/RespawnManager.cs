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


        Vector3 Position = _AllRespawnPoints[Random.Range(0, _AllRespawnPoints.Count)].transform.position;
        ClientRpcParams clientRpcParams = new ClientRpcParams
        {
            Send = new ClientRpcSendParams
            {
                TargetClientIds = new ulong[] { ClientId }
            }
        };
        NetworkManager.ConnectedClients[ClientId].PlayerObject.GetComponent<HealthManager>().SetPositionClientRPC(Position, clientRpcParams);
        return;
    }

    public void RemoveSpawnPoint()
    {
        _AllRespawnPoints.Clear();
    }
}
