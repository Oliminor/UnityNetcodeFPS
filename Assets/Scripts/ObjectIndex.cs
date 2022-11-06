using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

public class ObjectIndex : NetworkBehaviour
{
    [SerializeField] private int objectIndex;
    [SerializeField] private Transform spawner;
    [SerializeField] private LayerMask whatIsGround;

    public int GetIndex() { return objectIndex; }

    private void Start()
    {
        if (!IsOwner) return;
        gameObject.layer = LayerMask.NameToLayer("RenderOnTop");

        foreach (Transform child in transform) child.gameObject.layer = LayerMask.NameToLayer("RenderOnTop");
    }

    /// <summary>
    /// Call this when the player drops the Object (flag at the moment), spawn a spawner so other player could pick up it again
    /// </summary>
    [ServerRpc]
    public void DropObjectServerRPC()
    {
        if (!spawner) return;

        Vector3 spawnPosition = transform.position;

        if (Physics.Raycast(transform.position, Vector3.down,out RaycastHit hitInfo , 10, whatIsGround))
        {
            spawnPosition = hitInfo.point;
        }

        GameObject go = Instantiate(spawner.gameObject, spawnPosition, Quaternion.identity);
        go.GetComponent<NetworkObject>().Spawn();
        go.GetComponent<WeaponSpawner>().SetTemprarySpawnerServerRPC();
    }
}
