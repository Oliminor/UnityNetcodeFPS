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

    private GameObject _KilledBy;
    private bool _CanRespawn;
    private GameObject _NetworkManager;

    void Start()
    {
        _NetworkManager = GameObject.Find("ObjectiveManager");
        _KilledBy = gameObject;
    }

    void Awake()
    {
        _NetworkManager = GameObject.Find("ObjectiveManager");
    }

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
            Respawn(true);
            if (IsOwner) SetHealthServerRPC(_HealthMax);
        }
    }

    public void Respawn(bool GivePoint)
    {
        SetHealthServerRPC(_HealthMax);
        transform.position = _NetworkManager.GetComponent<RespawnManager>().GetRespawnPoint().transform.position;//new Vector3(0, 10, 0);
            //if (_NetworkManager.GetComponent<ObjectiveManager>().GetMode() == MODES.DEATHMATCH && _KilledBy.GetComponent<PlayerTeamManager>().GetTeam() != GetComponent<PlayerTeamManager>().GetTeam() && GivePoint)
            //{
            //    _NetworkManager.GetComponent<ObjectiveManager>().AddScoreToTeamServerRPC(1, (int)_KilledBy.GetComponent<PlayerTeamManager>().GetTeam());
            //}
    }

    /// <summary>
    /// Damage the player
    /// </summary>
    public void DamagePlayer(float damage, GameObject Source)
    {
        int health = _HealthCur.Value;

        Debug.Log("got hit " + damage);

        health -= (int)damage;

        _KilledBy = Source;
        Debug.Log(_KilledBy);

        SetHealthServerRPC(health);

    }
}
