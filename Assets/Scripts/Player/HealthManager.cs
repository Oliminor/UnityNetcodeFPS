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

    private void Update()
    {
        if (IsOwner)
        {
            HUD.instance.SetHUDPercent(GetPercentHealth());
            HUD.instance.SetPlayerHealthTextHUD(_HealthCur.Value, _HealthMax);

            if (transform.position.y < -50) Respawn(false);
        }
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

    [ServerRpc(RequireOwnership = false)]
    private void DamagePlayerServerRPC(int health)
    {
        _HealthCur.Value -= health;
        if (_HealthCur.Value < 0) _HealthCur.Value = 0;
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
            }
        }
    }

    public void Respawn(bool AwardPoint)
    {
        StartCoroutine(InterpolateSwitch());
        GetComponent<Rigidbody>().velocity = Vector3.zero;
        GetComponent<Rigidbody>().angularVelocity = Vector3.zero;
        Debug.Log("HElo there");
        _ObjectiveManager = GameObject.Find("ObjectiveManager");
        SetHealthServerRPC(_HealthMax);
        player.GetWeaponInventory().DropEveryWeapons();
        
        Debug.Log("HGEOIFHAFH DHOHAWIDHAW ");
        _ObjectiveManager.GetComponent<RespawnManager>().GetRespawnPointServerRPC();
        if (_ObjectiveManager.GetComponent<ObjectiveManager>().GetMode() == MODES.DEATHMATCH && _KilledBy.GetComponent<PlayerTeamManager>().GetTeam() != GetComponent<PlayerTeamManager>().GetTeam() && AwardPoint)
        {
            _ObjectiveManager.GetComponent<ObjectiveManager>().AddScoreToTeamServerRPC(1, (int)_KilledBy.GetComponent<PlayerTeamManager>().GetTeam());
        }
        if (_ObjectiveManager.GetComponent<ObjectiveManager>().GetMode() == MODES.INFECTION && AwardPoint)
        {
            if (_KilledBy.GetComponent<PlayerTeamManager>().GetTeam() == TEAMS.BLUE)
            {
                Debug.Log("Changing team here you idiot");
                //GetComponent<PlayerTeamManager>().ChangeTeam((int)TEAMS.BLUE);
                _ObjectiveManager.GetComponent<ObjectiveManager>().SetPlayerToTeamServerRPC((int)TEAMS.BLUE);
            }
            _ObjectiveManager.GetComponent<ObjectiveManager>().AddScoreToTeamServerRPC(1, (int)_KilledBy.GetComponent<PlayerTeamManager>().GetTeam());
        }
        player.GetWeaponInventory().ResetInventory();
    }

    [ClientRpc]
    public void SetPositionClientRPC(Vector3 NewPosition, ClientRpcParams clientRpcParams = default)
    {
        transform.position = NewPosition;
    }

    IEnumerator InterpolateSwitch()
    {
        GetComponent<BetterNetworkTransform>().Interpolate = false;
        yield return new WaitForSeconds(0.5f);
        GetComponent<BetterNetworkTransform>().Interpolate = true;
    }

    /// <summary>
    /// Damage the player
    /// </summary>
    public void DamagePlayer(float damage, GameObject Source)
    {
        Debug.Log("DamagePlayer " + damage);

        _KilledBy = Source;

        DamagePlayerServerRPC((int)damage);

        SetSourcePositionServerRPC(Source.transform.position);
    }
}
