using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

public class WeaponSpawner : NetworkBehaviour
{
    [SerializeField] GameObject weapon;
    [SerializeField] NetworkVariable<int> index = new NetworkVariable<int>(0, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner);

    public void SetIndex(int _index) { index.Value = _index; }

    [ClientRpc]
    public void SpawnWeaponClientRPC()
    {
        GameObject go = Instantiate(this.gameObject, transform.position, Quaternion.identity);
        go.GetComponent<WeaponSpawner>().SetIndex(weapon.GetComponent<WeaponManager>().GetIndex());
        go.GetComponent<NetworkObject>().Spawn();

        Destroy(gameObject);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (index.Value == 0)
        {
            Destroy(gameObject);
            return;
        }

        if (other.tag == "Player")
        {
            Debug.Log("Picked up weapon index: " + index.Value + " Clinet ID " + other.GetComponent<PlayerMovement>().OwnerClientId);
            other.GetComponent<PlayerMovement>().GetWeaponInventory().AddWeapon(index.Value);
        }
    }
}
