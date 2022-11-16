using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;
using TMPro;

public class HealthManager : NetworkBehaviour
{

    //private float _HealthCur;
    [SerializeField] NetworkVariable<int> _HealthCur = new NetworkVariable<int>(-1, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner);
    [SerializeField] NetworkVariable<Vector3> sourcePlayer = new NetworkVariable<Vector3>();
    [SerializeField] int _HealthMax;
    [SerializeField] int _HealthRegen;
    [SerializeField] float _HealthCooldown;

    private TextMeshProUGUI _RespawnCounter;
    private bool _Respawning;
    private float _RespawningCountDown;
    private int _KilledByTeamID;
    private int _KilledByID;
    private GameObject _ObjectiveManager;
    private PlayerMovement player;

    public float GetPercentHealth() { return (float)_HealthCur.Value / _HealthMax * 100.0f; }

    void Start()
    {
        player = GetComponent<PlayerMovement>();
        _ObjectiveManager = GameObject.Find("ObjectiveManager");
        _KilledByTeamID = 0;
        _RespawnCounter = GameObject.Find("RespawnText").GetComponent<TextMeshProUGUI>();
    }

    void Awake()
    {
        _ObjectiveManager = GameObject.Find("ObjectiveManager");
    }

    private void Update()
    {
        if (IsOwner)
        {
            if (_Respawning)
            {
                Respawn(false);
            }
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

    [ClientRpc]
    public void SetMaxHealthClientRPC(int NewMaxHealth)
    {
        _HealthMax = NewMaxHealth;
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

    [ClientRpc]
    private void SetHealthClientRPC(int health)
    {
        if (IsOwner)
        {
            _HealthCur.Value = health;
        }
        
    }

    [ServerRpc(RequireOwnership = false)]
    private void DamagePlayerServerRPC(int damage, int ID, int PlayerID)
    {
        // _HealthCur.Value -= damage;
        //  if (_HealthCur.Value < 0) _HealthCur.Value = 0;
        DamagePlayerClientRPC(damage, ID, PlayerID);
    }

    [ClientRpc]
    private void DamagePlayerClientRPC(int damage, int Team, int PlayerID)
    {
        if (IsOwner)
        {
            _KilledByTeamID = Team;
            _KilledByID = PlayerID;
            _HealthCur.Value -= damage;
            if (_HealthCur.Value < 0) _HealthCur.Value = 0;

            if (_HealthCur.Value <= 0)
            {
                Respawn(true);
            }
        }
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

    [ClientRpc]
    public void NewGameClientRPC()
    {
        _RespawningCountDown = 0;
        _Respawning = false;
    }

    public void Respawn(bool AwardPoint)
    {
        _ObjectiveManager = GameObject.Find("ObjectiveManager");
        if (!_Respawning)
        {
            player.GetWeaponInventory().DropEveryWeapons();
            player.GetWeaponInventory().ResetInventory();
            _RespawnCounter.gameObject.SetActive(true);
            _Respawning = true;
            _RespawningCountDown = 5;
            if (AwardPoint)
            {
                GetComponent<PlayerStats>().AddDeathsServerRPC(1);
                AwardKillPointServerRPC(_KilledByID);
            }
            
            if (_ObjectiveManager.GetComponent<ObjectiveManager>().GetMode() == MODES.DEATHMATCH && AwardPoint)
            {
                _ObjectiveManager.GetComponent<ObjectiveManager>().AddScoreToTeamServerRPC(1, _KilledByTeamID);
            }
            if (_ObjectiveManager.GetComponent<ObjectiveManager>().GetMode() == MODES.INFECTION)
            {
                if ((TEAMS)_KilledByTeamID == TEAMS.BLUE)
                {
                    _ObjectiveManager.GetComponent<ObjectiveManager>().SetPlayerToTeamServerRPC((int)TEAMS.BLUE);
                    AwardPointsServerRPC(_KilledByID, 10);
                }
                _ObjectiveManager.GetComponent<ObjectiveManager>().AddScoreToTeamServerRPC(1, _KilledByTeamID);
            }
            transform.position = new Vector3(0, 100, 0);
            
            return;
        }
        else if(_RespawningCountDown > 0)
        {
            _RespawnCounter.text = "Respawning in: " + (int)_RespawningCountDown;
            _RespawningCountDown -= Time.deltaTime;
            //GetComponentInChildren<WeaponInventory>().ActivateWeaponServerRPC(-1);
            return;
        }
        else
        {
            _RespawnCounter.gameObject.SetActive(false);
            StartCoroutine(InterpolateSwitch());
            GetComponent<Rigidbody>().velocity = Vector3.zero;
            GetComponent<Rigidbody>().angularVelocity = Vector3.zero;
            Debug.Log("HElo there");

            _HealthCur.Value = _HealthMax;
            player.GetWeaponInventory().DropEveryWeapons();

            Debug.Log("HGEOIFHAFH DHOHAWIDHAW ");
            _ObjectiveManager.GetComponent<RespawnManager>().GetRespawnPointServerRPC();
            
            _Respawning = false;
        }
        
    }

    [ServerRpc(RequireOwnership = false)]
    public void AwardKillPointServerRPC(int ClientID)
    {
        NetworkManager.ConnectedClients[(ulong)ClientID].PlayerObject.GetComponent<PlayerStats>().AddKillsServerRPC(1);
    }

    [ServerRpc(RequireOwnership = false)]
    public void AwardPointsServerRPC(int ClientID, int Amount)
    {
        NetworkManager.ConnectedClients[(ulong)ClientID].PlayerObject.GetComponent<PlayerStats>().AddScoreServerRPC(Amount);
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
        if (_Respawning) return;
        Debug.Log("DamagePlayer " + damage);

        Debug.Log(IsServer +" "+ IsHost +" "+ IsClient +" "+ IsOwner);

        if (IsClient && !IsServer && !IsHost) DamagePlayerServerRPC((int)damage, (int)Source.GetComponent<PlayerTeamManager>().GetTeam(), (int)Source.GetComponent<NetworkObject>().OwnerClientId);

        SetSourcePositionServerRPC(Source.transform.position);

        //DamagePlayerServerRPC((int)damage, (int)GetComponent<PlayerTeamManager>().GetTeam());

        if (IsHost && !IsOwner)
        {
            DamagePlayerClientRPC((int)damage, (int)Source.GetComponent<PlayerTeamManager>().GetTeam(), (int)Source.GetComponent<NetworkObject>().OwnerClientId);
        }
    }
}
