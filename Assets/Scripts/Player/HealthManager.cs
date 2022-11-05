using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

public class HealthManager : NetworkBehaviour
{

    //private float _HealthCur;
    [SerializeField] NetworkVariable<int> _HealthCur = new NetworkVariable<int>(-1);
    [SerializeField] NetworkVariable<Vector3> sourcePlayer = new NetworkVariable<Vector3>();
    [SerializeField] int _HealthMax;
    [SerializeField] int _HealthRegen;
    [SerializeField] float _HealthCooldown;

    private GameObject _KilledBy;
    private bool _CanRespawn;
    private GameObject _ObjectiveManager;
    private PlayerMovement player;

    public float GetPercentHealth() { return (float)_HealthCur.Value / _HealthMax * 100.0f; }

    void Start()
    {
        player = GetComponent<PlayerMovement>();
        _ObjectiveManager = GameObject.Find("ObjectiveManager");
        _KilledBy = gameObject;
    }

    void Awake()
    {
        _ObjectiveManager = GameObject.Find("ObjectiveManager");
    }

    public override void OnNetworkSpawn()
    {
        _HealthCur.OnValueChanged += SetHealthClientRPC;
        sourcePlayer.OnValueChanged += SetSourcePositionClientRPC;
        _ObjectiveManager = GameObject.Find("ObjectiveManager");
    }

    [ServerRpc(RequireOwnership = false)]
    private void SetSourcePositionServerRPC(Vector3 _sourcePlayer)
    {
        sourcePlayer.Value = _sourcePlayer;
    }

    [ClientRpc]
    private void SetSourcePositionClientRPC(Vector3 _prePos, Vector3 _newPos)
    {
        if (IsOwner) DamageIndicatorManagerUI.instance.InstantiateDamageIndicator(_newPos);
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
            if (IsOwner)
            {
                Respawn(true);
                SetHealthServerRPC(_HealthMax);
            }
        }
    }

    public void Respawn(bool AwardPoint)
    {
        GetComponent<Rigidbody>().velocity = Vector3.zero;
        GetComponent<Rigidbody>().angularVelocity = Vector3.zero;
        _ObjectiveManager = GameObject.Find("ObjectiveManager");
        SetHealthServerRPC(_HealthMax);
        player.GetWeaponInventory().DropEveryWeapons();
        player.GetWeaponInventory().ResetInventory();
        transform.position = _ObjectiveManager.GetComponent<RespawnManager>().GetRespawnPoint().transform.position;
            if (_ObjectiveManager.GetComponent<ObjectiveManager>().GetMode() == MODES.DEATHMATCH && _KilledBy.GetComponent<PlayerTeamManager>().GetTeam() != GetComponent<PlayerTeamManager>().GetTeam() && AwardPoint)
            {
            _ObjectiveManager.GetComponent<ObjectiveManager>().AddScoreToTeamServerRPC(1, (int)_KilledBy.GetComponent<PlayerTeamManager>().GetTeam());
            }
    }

    /// <summary>
    /// Damage the player
    /// </summary>
    public void DamagePlayer(float damage, GameObject Source)
    {
        int health = _HealthCur.Value;

        //Debug.Log("got hit " + damage);

        health -= (int)damage;

        _KilledBy = Source;
        //Debug.Log(_KilledBy);

        SetHealthServerRPC(health);
        SetSourcePositionServerRPC(Source.transform.position);

        if (!IsServer) return;

        _HealthCur.Value = health;
        SetHealthClientRPC(0, health);
        SetSourcePositionClientRPC(Vector3.zero, Source.transform.position);
    }
}
