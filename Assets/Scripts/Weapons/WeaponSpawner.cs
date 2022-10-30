using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

public class WeaponSpawner : NetworkBehaviour
{
    [SerializeField] private GameObject weapon; 

    [SerializeField] private NetworkVariable<int> index = new NetworkVariable<int>(0);
    [SerializeField] private NetworkVariable<bool> isWeaponPickedUp = new NetworkVariable<bool>(false);
    [SerializeField] private NetworkVariable<bool> isTemporarySpawner = new NetworkVariable<bool>(false);

    public void SetIndex(int _index) { index.Value = _index; }

    /// <summary>
    /// Activate when the object spawned (or the beginning of the game, thats also count spawn, even if it's already on the scene)
    /// </summary>
    public override void OnNetworkSpawn()
    {
        SetIndex(weapon.GetComponent<ObjectIndex>().GetIndex());
        if (isWeaponPickedUp.Value) DeActivateSpawnerServerRPC();
    }

    /// <summary>
    /// Set the isTemporarySpawner true, the temporary spawners are disappear after the item picked up (the permanent one stays (example the default position of the flag))
    /// </summary>
    [ServerRpc(RequireOwnership = false)]
    public void SetTemprarySpawnerServerRPC() 
    { 
        isTemporarySpawner.Value = true; 
    }

    /// <summary>
    /// Deactivate the spawner on the server side
    /// </summary>
    [ServerRpc (RequireOwnership = false)]
    private void DeActivateSpawnerServerRPC()
    {
        isWeaponPickedUp.Value = true;
        DeActivateSpawnerClientRPC();
    }

    /// <summary>
    /// Deactivate the spawner on the client side
    /// </summary>
    [ClientRpc]
    private void DeActivateSpawnerClientRPC()
    {
        if (isTemporarySpawner.Value && IsServer) gameObject.GetComponent<NetworkObject>().Despawn();
        for (int i = 0; i < transform.childCount; i++)
        {
            transform.GetChild(i).gameObject.SetActive(false);
        }
    }


    /// <summary>
    /// Pick up the weapon if the Player is inside the collder and press F key
    /// </summary>
    private void OnTriggerStay(Collider other)
    {
        if (isWeaponPickedUp.Value) return;

        if (other.tag == "Player" && Input.GetKey(KeyCode.F))
        {
            Debug.Log("Picked up weapon index: " + index.Value + " Clinet ID " + other.GetComponent<PlayerMovement>().OwnerClientId);
            other.GetComponent<PlayerMovement>().GetWeaponInventory().AddWeapon(index.Value);
            other.GetComponent<PlayerMovement>().GetWeaponInventory().ActivatePickedUpWeapon(index.Value - 1);
            DeActivateSpawnerServerRPC();
        }
    }
}
