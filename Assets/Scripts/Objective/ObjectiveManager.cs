using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Collections;
using Unity.Netcode;
using UnityEngine.SceneManagement;
using TMPro;


public enum TEAMS { RED, BLUE, GREEN, YELLOW };

public enum MODES { DEATHMATCH, KINGOFTHEHILL, INFECTION, ONEFLAG};

[System.Serializable]
public struct TEAMDATA : INetworkSerializable, System.IEquatable<TEAMDATA> {

    public Color Colour;
    public int TeamScore;
    public int TeamCount;
    public FixedString128Bytes TeamName;

    public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
    {
        serializer.SerializeValue(ref Colour);
        serializer.SerializeValue(ref TeamScore);
        serializer.SerializeValue(ref TeamCount);
        serializer.SerializeValue(ref TeamName);

    }

    public bool Equals(TEAMDATA other)
    {
        return false;
    }
}

public class ObjectiveManager : NetworkBehaviour
{

    [SerializeField] public TEAMDATA[] _Teams;

    //[SerializeField] private NetworkVariable<MODES> _CurrentMode = new NetworkVariable<MODES>(MODES.KINGOFTHEHILL, NetworkVariableReadPermission.Owner, NetworkVariableWritePermission.Server);
    [SerializeField] private MODES _CurrentMode;
    private float _TimeLimit = 0;
    private int _MaxScore;
    private int[] _TeamWeapons;
    private int[] _TeamMaxHealth;
    private NetworkVariable<FixedString512Bytes> _Desc = new NetworkVariable<FixedString512Bytes>();

    [SerializeField] List<TextMeshProUGUI> _ScoreText;
    [SerializeField] GameObject _PlayerForAI;

    [SerializeField] private GameObject _Spawners;
    [SerializeField] private GameObject _KingOfTheHill;
    [SerializeField] private List<GameObject> _HillLocations;
    [SerializeField] private GameObject _FlagSpawn;
    [SerializeField] private GameObject _FlagDest;

    //[SerializeField] private NetworkVariable<int> _KingOfTheHillActive = new NetworkVariable<int>(0, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);

    private bool _GameInProgress;
    private GameObject _SceneManager;
    private GameObject _Scoreboard;

    //public static ObjectiveManager instance;

    public GameObject TeamScoreText;

    private List<GameObject> _Players;
    private List<GameObject> _Bots;

    private NetworkVariable<float> _Time = new NetworkVariable<float>(0, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);
    private float _TimeSinceMove = 0;

    public TextMeshProUGUI text; //TESTING SYNC CAUSE MY UNITY EDITOR IS TRASH- OLLIE

    // Start is called before the first frame update
    private void Awake()
    {
        _TeamWeapons = new int[4];
        _TeamMaxHealth = new int[4];
        NetworkManager.SceneManager.OnLoadEventCompleted += SceneManagement_OnLoadEventCompleted;

        //NetworkManager.SceneManager.OnUnload += SceneManagement_OnUnload;
    }
    void Start()
    {
        _Bots = new List<GameObject> { };
       // instance = this;
        _GameInProgress = false;
        _TeamMaxHealth = new int[4];
        _SceneManager = GameObject.Find("SceneManager");
        if (!IsServer) return;
        StartNewGameServerRPC(_SceneManager.GetComponent<GameModeManager>().GetCurrentMode());
    }

    private void SceneManagement_OnLoadEventCompleted(string sceneName, LoadSceneMode loadSceneMode, List<ulong> clientsCompleted, List<ulong> clientsTimedOut)
    {
        if (IsServer)
        {
            if(sceneName==ProjectNetworkSceneManager.sceneNames[2]&&clientsCompleted.Count==ProjectNetworkSceneManager.singleton.playersConnected.Value)
            {
                _SceneManager = GameObject.Find("SceneManager");
                
                Debug.Log("BOOOOOOOOIIIIIIIII IT WORKED"+ clientsCompleted.Count);
                
            }
           
        }  
    }

    //private void SceneManagement_OnUnload(ulong clientId, string sceneName, AsyncOperation asyncOperation)
    //{
    //    if(IsServer)
    //    {
    //        if (_GameInProgress)
    //        {
    //            EndGame();
    //        }
    //    }
        
    //}

    // Update is called once per frame
    void Update()
    {
        
        //Debug.Log("Game in Progress" + _GameInProgress);
        if (!_GameInProgress) return;
        if (!IsServer) return;

        foreach(TEAMDATA TeamData in _Teams)
        {
            //Debug.Log("Max score" + _MaxScore);
            
            if (TeamData.TeamScore >= _MaxScore && IsServer)
            {
                EndGame();
            }
            
        }

        if (_Time.Value > _TimeLimit)
        {
            EndGame();
        }

        switch (_CurrentMode)
        {
            case (MODES.DEATHMATCH):
                break;
            case (MODES.KINGOFTHEHILL):
                if (_TimeSinceMove <= 0)
                {
                    _TimeSinceMove = (float)(_TimeLimit * 0.2);
                    MoveHillServerRPC();
                }
                break;
            case (MODES.INFECTION):
                if (_Teams[1].TeamScore == NetworkManager.ConnectedClientsList.Count - 1 && _Time.Value >= 10)
                {
                    EndGame();
                }
                break;
            case (MODES.ONEFLAG):
                break;
        }

        _Time.Value += Time.deltaTime;
        _TimeSinceMove -= Time.deltaTime;
    }

    [ServerRpc]
    void AssignTeamsServerRPC()
    {
        int TooInfect = Random.Range(0, NetworkManager.Singleton.ConnectedClients.Count);
        if (!IsServer) return;
        //for(int i = 0; i < NetworkManager.Singleton.ConnectedClients.Count; i++)
        int i = 0;
        foreach(NetworkClient PlayerClient in NetworkManager.ConnectedClientsList)
        {
            NetworkObject Player = PlayerClient.PlayerObject;
            
            switch (_CurrentMode) 
            {
                case (MODES.DEATHMATCH):
                    SetPlayerToTeamServerRPC((i + 1) % 2, PlayerClient.ClientId);
                    break;
                case (MODES.KINGOFTHEHILL):
                    SetPlayerToTeamServerRPC((i + 1) % 2, PlayerClient.ClientId);
                    break;
                case (MODES.INFECTION):
                    SetPlayerToTeamServerRPC(0, PlayerClient.ClientId);
                    if(TooInfect == i)
                    {
                        SetPlayerToTeamServerRPC(1, PlayerClient.ClientId);
                    }
                    break;
                case (MODES.ONEFLAG):
                    SetPlayerToTeamServerRPC((i + 1) % 2, PlayerClient.ClientId);
                    break;
            }
            Player.GetComponent<PlayerStats>().ResetStatsServerRPC();
            i++;
        }
    }

    public void EndGame()
    {

        _GameInProgress = false;

        Debug.Log("GameOver");
        _SceneManager.GetComponent<GameModeManager>()._InLobby.Value = true;

        _SceneManager.GetComponent<ProjectNetworkSceneManager>().ExitGameMode();
    }

    [ServerRpc]
    public void StartNewGameServerRPC(GameModeData ModeData)
    {
        _SceneManager.GetComponent<GameModeManager>()._InLobby.Value = false;
        SetGameModeSettings(ModeData);
        AssignTeamsServerRPC();
        StartNewGameClientRPC(ModeData);
        StartNewGameServerRPC();
        
    }

    public void SetGameModeSettings(GameModeData ModeData)
    {
        _CurrentMode = ModeData.Mode;
        _MaxScore = ModeData.ScoreLimit;
        _TimeLimit = ModeData.TimeLimit;
        _TeamWeapons[0] = ModeData.Team1Weapons;
        _TeamWeapons[1] = ModeData.Team2Weapons;
        _TeamMaxHealth[0] = ModeData.Team1MaxHealth;
        _TeamMaxHealth[1] = ModeData.Team2MaxHealth;
        if (IsServer)
        {
            switch (_CurrentMode)
            {
                case (MODES.DEATHMATCH):
                    _Desc.Value = "Kill The Enemy Team To Win";
                    break;
                case (MODES.KINGOFTHEHILL):
                    _Desc.Value = "Capture And Hold The Hill To Win";
                    break;
                case (MODES.INFECTION):
                    _Desc.Value = "Red Team Surive For 120 Seconds\nBlue Team Must Kill And Convert Red Team To Win";
                    break;
                case (MODES.ONEFLAG):
                    _Desc.Value = "Red Team Must Defend The Flag(Yellow Square)\nBlue Team Must Take The Flag Too The Red Square";
                    break;
            }
            Debug.Log("Objective: " + _Desc.Value);
        }
        
        if (!ModeData.HasWeapons)
        {
            _Spawners.transform.position = new Vector3(0, -1000, 0);
        }
    }

    [ClientRpc]
    void StartNewGameClientRPC(GameModeData ModeData)
    {
        _Teams[0].TeamName = "Red";
        _Teams[1].TeamName = "Blue";
        SetGameModeSettings(ModeData);
        _GameInProgress = true;
        //GetComponent<MenuManager>().SetMenuState(MENUSTATES.INGAME);
        NetworkManager.LocalClient.PlayerObject.GetComponent<PlayerStats>().ResetStatsServerRPC();
        _KingOfTheHill.SetActive(false);
        //_FlagSpawn.SetActive(false);
        _FlagDest.SetActive(false);
        switch (_CurrentMode) 
        {
            case (MODES.DEATHMATCH):
                
                Debug.Log("Starting deathmatch");
                break;
            case (MODES.KINGOFTHEHILL):
                _KingOfTheHill.SetActive(true);
                Debug.Log("Starting King of the hill");
                break;
            case (MODES.INFECTION):
                Debug.Log("Starting Infection");
                break;
            case (MODES.ONEFLAG):
              //  _FlagSpawn.SetActive(true);
                _FlagDest.SetActive(true);
                break;
        }
        NetworkManager.LocalClient.PlayerObject.GetComponent<HealthManager>().NewGameClientRPC();
        NetworkManager.LocalClient.PlayerObject.GetComponent<HealthManager>().Respawn(false);
    }

    [ServerRpc]
    void StartNewGameServerRPC()
    {
        _GameInProgress = true;
        _KingOfTheHill.GetComponent<HillManager>().StartGame();
        if (_CurrentMode == MODES.KINGOFTHEHILL)
        {
            _KingOfTheHill.SetActive(true);
            _KingOfTheHill.transform.position = _HillLocations[0].transform.position;

        }
        
        for (int i = 0; i < _Teams.Length; i++)
        {
            Debug.Log("Setting team to 0 points" + i);
            SetScoreToTeamServerRPC(0, i);
        }
        _TimeSinceMove = 5;
        _Time.Value = 0;
    }

    [ServerRpc]
    public void SpawnAIServerRPC()
    {
        GameObject AI = Instantiate(_PlayerForAI);
        AI.GetComponent<NetworkObject>().Spawn(true);
        AI.GetComponent<NetworkObject>().RemoveOwnership();
        _Bots.Add(AI);
    }

    [ServerRpc]
    public void ClearAIServerRPC()
    {
        foreach (GameObject Bot in _Bots)
        {
            Bot.GetComponent<NetworkObject>().Despawn();
            Destroy(Bot);
        }
        _Bots.Clear();
    }

    public string GetDescription()
    {
        FixedString512Bytes BString = _Desc.Value;
        string String = BString.ConvertToString();
        return String;
    }

    [ServerRpc]
    public void MoveHillServerRPC()
    {

        _KingOfTheHill.transform.position = _HillLocations[Random.Range(0, _HillLocations.Count)].transform.position;
    }

    [ServerRpc(RequireOwnership = false)]
    public void FlagCapturedServerRPC()
    {
        Debug.Log("FLAG RETRIED");
        AddScoreToTeamServerRPC(1, 1);
        _FlagSpawn.GetComponent<WeaponSpawner>().ActivateSpawnerServerRPC();
    }

    public Color GetTeamColour(TEAMS Team)
    {
        return _Teams[(int)Team].Colour;
    }

    [ServerRpc(RequireOwnership = false)]
    public void SetPlayerToTeamServerRPC(int Team, ServerRpcParams serverRpcParams = default)
    {
        var ClientID = serverRpcParams.Receive.SenderClientId;
        SetPlayerToTeamServerRPC(Team, ClientID);
        

    }
    [ServerRpc]
    public void SetPlayerToTeamServerRPC(int Team, ulong PlayerID)
    {
        if (!NetworkManager.ConnectedClients.ContainsKey(PlayerID)) return;

        GameObject Player = NetworkManager.Singleton.ConnectedClients[PlayerID].PlayerObject.transform.gameObject;

        Debug.Log("The weapon that Team " + Team + " Should spawn with is " + _TeamWeapons[Team] + " The ClientID is: " + PlayerID);

        Player.transform.GetChild(2).transform.GetChild(0).GetComponent<WeaponInventory>().ChangeDefaultWeaponClientRPC(_TeamWeapons[Team]);
        Player.GetComponent<HealthManager>().SetMaxHealthClientRPC(_TeamMaxHealth[Team]);
        Debug.Log("Change default weapon: " + _TeamWeapons[Team]);
        _Teams[Team].TeamCount++;
        Player.GetComponent<PlayerTeamManager>()._Team.Value = (TEAMS)Team;

        UpdateTeamDataClientRpc(_Teams[0], 0);
        UpdateTeamDataClientRpc(_Teams[1], 1);
    }

    [ServerRpc(RequireOwnership = false)]
    public void AddScoreToTeamServerRPC(int Score, int Team, ServerRpcParams serverRpcParams = default)
    {
        _Teams[Team].TeamScore += Score;
       // NetworkManager.ConnectedClients[serverRpcParams.Receive.SenderClientId].PlayerObject.GetComponent<PlayerStats>().AddScoreServerRPC(1);
        UpdateTeamDataClientRpc(_Teams[Team], Team);
    }

    [ServerRpc(RequireOwnership = false)]
    public void SetScoreToTeamServerRPC(int Score, int Team)
    {
        _Teams[Team].TeamScore = Score;
        UpdateTeamDataClientRpc(_Teams[Team], Team);
    }

    [ClientRpc]
    public void UpdateTeamDataClientRpc(TEAMDATA TeamData, int i)
    {
       _Teams[i] = TeamData;
    }

    public MODES GetMode()
    {
        return _CurrentMode;
    }

    public void SetMode(int GameMode)
    {
        //_CurrentMode = (MODES)GameMode;
    }
}
