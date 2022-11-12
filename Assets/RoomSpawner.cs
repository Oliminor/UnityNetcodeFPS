using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

public class RoomSpawner : NetworkBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        if(IsServer)
        {
            foreach (var client in NetworkManager.Singleton.ConnectedClients)
            {
                RoomSpawnClientRpc();
                
            }
        }
    }

   [ClientRpc]
   private void RoomSpawnClientRpc()
    {
        NetworkManager.Singleton.LocalClient.PlayerObject.transform.position = new Vector3(0, 0, 0);
    }
}
