using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;
using Unity.Netcode.Components;

public class MeleeWeaponManager : NetworkBehaviour
{
    [SerializeField] CapsuleCollider capsuleCol;
    [SerializeField] Transform playerHitEffect;
    [SerializeField] int damage;
    [SerializeField] float meleeRate;

    private Animator anim;
    private PlayerMovement player;
    private float coolDownTimer;

    private void Awake()
    {
        capsuleCol.enabled = false;
    }

    private void OnEnable()
    {
        if (CrossHairManagement.instance && IsOwner)
        {
            player = transform.root.GetComponent<PlayerMovement>();
            anim = player.GetWeaponInventory().GetAnimator();
            anim.SetTrigger("meleeActivated");
            CrossHairManagement.instance.ActivateCrossHair(false);
        }
            
    }

    private void OnDisable()
    {
        if (CrossHairManagement.instance && IsOwner)
        {
            CrossHairManagement.instance.ActivateCrossHair(true);
        }
    }

    void Update()
    {
        if (!IsOwner) return;

        MeleeAttack();
        HUD.instance.SetPlayerAmmoTextHUD(0, 0);

        anim.SetFloat("speed", player.GetAnimSpeed());
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

        if (Input.GetMouseButtonDown(0) && coolDownTimer <= 0 && !player.IsRunning())
        {
            coolDownTimer = meleeRate;

            anim.SetTrigger("meleeAttack");

            if (IsClient) MeleeAttackServerRPC();
        }
    }

    /// <summary>
    /// Collider active time (1 sec)
    /// </summary>
    IEnumerator ActivateMeleeCollider()
    {
        yield return new WaitForSeconds(0.1f);
        capsuleCol.enabled = true;
        yield return new WaitForSeconds(0.3f);
        capsuleCol.enabled = false;
    }

    /// <summary>
    /// Collider stuff (bloody effect, attacks the player if the weapon inside)
    /// </summary>
    private void OnTriggerEnter(Collider other)
    { 
        if (other.transform.gameObject.tag == "Player")
        {
            PlayerMovement player = transform.root.GetComponent<PlayerMovement>();
            if (other.GetComponent<PlayerMovement>() == player) return;
            other.transform.gameObject.GetComponent<HealthManager>().DamagePlayer(damage, player.gameObject);
            GameObject playerHitParticle = Instantiate(playerHitEffect.gameObject, other.transform.position, Quaternion.identity);
            if (IsServer) playerHitParticle.GetComponent<NetworkObject>().Spawn();
            Destroy(playerHitParticle, 1);
            capsuleCol.enabled = false;
        }
    }
}
