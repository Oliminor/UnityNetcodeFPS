using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

public class MeleeWeaponManager : NetworkBehaviour
{
    [SerializeField] CapsuleCollider capsuleCol;
    [SerializeField] Transform playerHitEffect;
    [SerializeField] int damage;
    [SerializeField] float meleeRate;

    private float coolDownTimer;

    private void Awake()
    {
        capsuleCol.enabled = false;
    }

    void Update()
    {
        if (!IsOwner) return;

        MeleeAttack();
    }

    [ServerRpc]
    private void MeleeAttackServerRPC()
    {
        MeleeAttackClientRPC();
    }

    [ClientRpc]
    private void MeleeAttackClientRPC()
    {
        StartCoroutine(ActivateMeleeCollider());
    }

    /// <summary>
    /// Calling the Melee Attack functions and RPCs
    /// </summary>
    private void MeleeAttack()
    {
        coolDownTimer -= Time.deltaTime;

        if (Input.GetMouseButton(0) && coolDownTimer <= 0)
        {
            coolDownTimer = meleeRate;

            if (IsClient) MeleeAttackServerRPC();

            if (IsServer) MeleeAttackClientRPC();
        }
    }

    /// <summary>
    /// Collider active time (1 sec)
    /// </summary>
    IEnumerator ActivateMeleeCollider()
    {
        capsuleCol.enabled = true;
        yield return new WaitForSeconds(1);
        capsuleCol.enabled = false;
    }

    /// <summary>
    /// Collider stuff (bloody effect, attacks the player if the weapon inside)
    /// </summary>
    private void OnTriggerEnter(Collider other)
    { 
        if (other.transform.gameObject.tag == "Player")
        {
            if (!IsServer) return;
            PlayerMovement player = transform.root.GetComponent<PlayerMovement>();
            other.transform.gameObject.GetComponent<HealthManager>().DamagePlayer(damage, player.gameObject);
            GameObject playerHitParticle = Instantiate(playerHitEffect.gameObject, other.transform.position, Quaternion.identity);
            if (IsServer) playerHitParticle.GetComponent<NetworkObject>().Spawn();
            Destroy(playerHitParticle, 1);
            capsuleCol.enabled = false;
        }
    }
}
