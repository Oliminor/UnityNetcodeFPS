using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

public class WeaponSpawner : NetworkBehaviour
{
    [SerializeField] GameObject spwnableWeapon;
    [SerializeField] GameObject weaponMesh;
    [SerializeField] Vector3 spawnRotation;
    Transform parentTransform;
    // Start is called before the first frame update


    [ServerRpc]
    public void SpawnServerRPC()
    {
        SpawnWeapon(parentTransform);
    }

    public void SpawnWeapon(Transform _parent)
    {
        weaponMesh.gameObject.SetActive(false);

        GameObject go = Instantiate(spwnableWeapon, Vector3.zero, Quaternion.identity, _parent);
        go.transform.localPosition = Vector3.zero;
        go.transform.localRotation = Quaternion.Euler(spawnRotation);
        go.GetComponent<NetworkObject>().Spawn();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.tag == "Player")
        {
            if (other.gameObject == !IsOwner) return;
            parentTransform = other.GetComponent<PlayerMovement>().GetWeaponInventory().transform.GetChild(0).transform;
            SpawnServerRPC();
            gameObject.SetActive(false);
        }
    }
}
