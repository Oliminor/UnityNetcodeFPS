using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

public class HealthManager : NetworkBehaviour
{

    //private float _HealthCur;
    [SerializeField] NetworkVariable<int> _HealthCur = new NetworkVariable<int>(-1);
    [SerializeField] int _HealthMax;
    [SerializeField] int _HealthRegen;
    [SerializeField] float _HealthCooldown;

    public override void OnNetworkSpawn()
    {
        _HealthCur.OnValueChanged += SetHealthClientRPC;
    }

    /// <summary>
    /// Set the player healt on the server side
    /// </summary>
    [ServerRpc(RequireOwnership = false)]
    private void SetHealthServerRPC(int health)
    {
        _HealthCur.Value = health;
    }

    /// <summary>
    /// if the player health less than 0, do the thingy
    /// </summary>
    [ClientRpc]
    private void SetHealthClientRPC(int prevHealth, int newHealth)
    {
        if (newHealth <= 0)
        {
            transform.position = new Vector3(0, 10, 0);
            if (IsOwner) SetHealthServerRPC(_HealthMax);
        }
    }

    /// <summary>
    /// Damage the player
    /// </summary>
    public void DamagePlayer(int damage)
    {
        int health = _HealthCur.Value;

        Debug.Log("got hit " + damage);

        health -= damage;

        SetHealthServerRPC(health);
    }
}
