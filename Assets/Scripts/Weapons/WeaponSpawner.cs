using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

public class WeaponSpawner : NetworkBehaviour
{
    [SerializeField] private GameObject weapon;
    [SerializeField] private Transform spawnEffect;
    [SerializeField] private MeshRenderer dissolveMat;

    [SerializeField] private NetworkVariable<int> index = new NetworkVariable<int>(0);
    [SerializeField] private NetworkVariable<bool> isWeaponPickedUp = new NetworkVariable<bool>(false);
    [SerializeField] private NetworkVariable<bool> isTemporarySpawner = new NetworkVariable<bool>(false);

    float dissolveLerp;

    public void SetIndex(int _index) { index.Value = _index; }

    private void Start()
    {
        DissolveEffect();
    }

    private void Update()
    {
        // for testing
        if (Input.GetKeyDown(KeyCode.G)) ActivateSpawnerServerRPC();

        if (!dissolveMat) return;

        dissolveLerp = Mathf.Lerp(dissolveLerp, 0, Time.deltaTime * 2);
        dissolveMat.material.SetFloat("_dissAmount", dissolveLerp);
    }

    /// <summary>
    /// Spawning effect
    /// </summary>
    private void SpawnEffect()
    {
        if (spawnEffect)
        {
            GameObject go = Instantiate(spawnEffect.gameObject, transform.position, Quaternion.identity);
            Destroy(go, 5);
        }

        DissolveEffect();
    }

    private void DissolveEffect()
    {
        if (dissolveMat) dissolveLerp = 1;
    }

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
    /// Activate the spawner on the server side
    /// </summary>
    [ServerRpc(RequireOwnership = false)]
    private void ActivateSpawnerServerRPC()
    {
        if (isTemporarySpawner.Value)
        {
            gameObject.GetComponent<NetworkObject>().Despawn();
            return;
        }

        isWeaponPickedUp.Value = false;
        ActivateSpawnerClientRPC();
    }

    /// <summary>
    /// Activate the spawner on the client side
    /// </summary>
    [ClientRpc]
    private void ActivateSpawnerClientRPC()
    {
        for (int i = 0; i < transform.childCount; i++)
        {
            transform.GetChild(i).gameObject.SetActive(true);
        }
        SpawnEffect();
    }


    /// <summary>
    /// Pick up the weapon if the Player is inside the collder and press F key
    /// </summary>
    private void OnTriggerStay(Collider other)
    {
        if (isWeaponPickedUp.Value) return;

        if (dissolveLerp > 0.1f) return;

        if (other.tag == "Player")
        {
            if (!other.GetComponent<PlayerMovement>().IsOwner) return;
            if (weapon.transform.tag == "CarriedObject")
            {
               if (Input.GetKey(KeyCode.F))
                {
                    other.GetComponent<PlayerMovement>().GetWeaponInventory().AddObject(index.Value);
                    other.GetComponent<PlayerMovement>().GetWeaponInventory().ActivatePickedUpWeapon(index.Value - 1);
                    DeActivateSpawnerServerRPC();
                }
                return;
            }
            else
            {
                if (other.GetComponent<PlayerMovement>().GetWeaponInventory().CheckIfPlayerHasThatWeapon(index.Value)) return;

                other.GetComponent<PlayerMovement>().GetWeaponInventory().AddWeapon(index.Value);
                other.GetComponent<PlayerMovement>().GetWeaponInventory().ActivatePickedUpWeapon(index.Value - 1);

                Debug.Log("Picked up weapon index: " + index.Value + " Clinet ID " + other.GetComponent<PlayerMovement>().OwnerClientId);

                DeActivateSpawnerServerRPC();
            }
        }
    }
}
