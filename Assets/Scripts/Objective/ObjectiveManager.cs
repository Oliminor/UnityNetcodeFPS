using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Collections;
using Unity.Netcode;
using UnityEngine.SceneManagement;
using TMPro;


public enum TEAMS { RED, BLUE, GREEN, YELLOW };

public enum MODES { DEATHMATCH, KINGOFTHEHILL, INFECTION};

[System.Serializable]
public struct TEAMDATA : INetworkSerializable, System.IEquatable<TEAMDATA> {

    public Color Colour;
    public int TeamScore;
    public FixedString128Bytes TeamName;
    public int[] PlayerData;

    public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
    {
        serializer.SerializeValue(ref Colour);
        serializer.SerializeValue(ref TeamScore);
        serializer.SerializeValue(ref TeamName);
        serializer.SerializeValue(ref PlayerData);

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
    private int _MaxScore;

    [SerializeField] List<TextMeshProUGUI> _ScoreText;
    [SerializeField] GameObject _PlayerForAI;

    [SerializeField] private GameObject _KingOfTheHill;
    [SerializeField] private List<GameObject> _HillLocations;
    //[SerializeField] private NetworkVariable<int> _KingOfTheHillActive = new NetworkVariable<int>(0, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);

    private bool _GameInProgress;
    private GameObject _SceneManager;
    [SerializeField] private GameObject _Scoreboard;
    //public static ObjectiveManager instance;

    public GameObject TeamScoreText;

    private List<GameObject> _Players;
    private List<GameObject> _Bots;

    private float _TimeLimit = 0;
    private NetworkVariable<float> _Time = new NetworkVariable<float>(0, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);
    private float _TimeSinceMove = 0;

    public TextMeshProUGUI text; //TESTING SYNC CAUSE MY UNITY EDITOR IS TRASH- OLLIE

    // Start is called before the first frame update
    private void Awake()
    {
        NetworkManager.SceneManager.OnLoadEventCompleted += SceneManagement_OnLoadEventCompleted;

        //NetworkManager.SceneManager.OnUnload += SceneManagement_OnUnload;
    }
    void Start()
    {
        _Bots = new List<GameObject> { };
       // instance = this;
        _GameInProgress = false;
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
            UpdateScoreboard();
            if (TeamData.TeamScore >= _MaxScore && IsServer)
            {
                EndGame();
            }
            if (_TimeSinceMove <= 0)
            {
                _TimeSinceMove = (float)(_TimeLimit * 0.25);
                MoveHillServerRPC();
            }
            if (_Time.Value > _TimeLimit)
            {
                EndGame();
            }
        }
        _Time.Value += Time.deltaTime;
        _TimeSinceMove -= Time.deltaTime;
    }

    void UpdateScoreboard()
    {
    }

    void AssignTeams()
    {
        if (!IsServer) return;
        for(int i = 0; i < NetworkManager.Singleton.ConnectedClients.Count; i++)
        {
            NetworkObject Player = NetworkManager.Singleton.ConnectedClients[(ulong)i].PlayerObject;
            Player.GetComponent<PlayerTeamManager>().ChangeTeam((i + 1) % 2);
            int PlayerID = i;
            TEAMS PlayerTeam = Player.GetComponent<PlayerTeamManager>().GetTeam();
            int PlayerScore = 0;
            int PlayerKills = 0;
            int PlayerDeaths = 0;
            _Teams[(int)PlayerTeam].PlayerData[(i * 5) + 0] = PlayerID;
            _Teams[(int)PlayerTeam].PlayerData[(i * 5) + 1] = (int)PlayerTeam;
            _Teams[(int)PlayerTeam].PlayerData[(i * 5) + 2] = PlayerScore;
            _Teams[(int)PlayerTeam].PlayerData[(i * 5) + 3] = PlayerKills;
            _Teams[(int)PlayerTeam].PlayerData[(i * 5) + 4] = PlayerDeaths;
        }
    }

    public void EndGame()
    {
        _KingOfTheHill.SetActive(false);

        _GameInProgress = false;

        Debug.Log("GameOver");

        _SceneManager.GetComponent<ProjectNetworkSceneManager>().ExitGameMode();
    }

    [ServerRpc]
    public void StartNewGameServerRPC(GameModeData ModeData)
    {

        StartNewGameClientRPC(ModeData);
        StartNewGameServerRPC();
        
    }

    public void SetGameModeSettings(GameModeData ModeData)
    {
        _CurrentMode = ModeData.Mode;
        _MaxScore = ModeData.ScoreLimit;
        _TimeLimit = ModeData.TimeLimit;
    }

    [ClientRpc]
    void StartNewGameClientRPC(GameModeData ModeData)
    {
        _Teams[0].TeamName = "Red";
        _Teams[1].TeamName = "Blue";
        SetGameModeSettings(ModeData);
        _GameInProgress = true;
        //GetComponent<MenuManager>().SetMenuState(MENUSTATES.INGAME);
        NetworkManager.LocalClient.PlayerObject.GetComponent<HealthManager>().Respawn(false);
        switch (_CurrentMode) 
        {
            case (MODES.DEATHMATCH):
                Debug.Log("Starting deathmatch");
                break;
            case (MODES.KINGOFTHEHILL):
                Debug.Log("Starting King of the hill");
                break;
        }
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
        AssignTeams();
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

    [ServerRpc]
    public void MoveHillServerRPC()
    {

        _KingOfTheHill.transform.position = _HillLocations[Random.Range(0, _HillLocations.Count)].transform.position;
    }

    public Color GetTeamColour(TEAMS Team)
    {
        return _Teams[(int)Team].Colour;
    }

    [ServerRpc(RequireOwnership = false)]
    public void SetPlayerToTeamServerRPC(int Team, ServerRpcParams serverRpcParams = default)
    {
        var ClientID = serverRpcParams.Receive.SenderClientId;

        if (!NetworkManager.ConnectedClients.ContainsKey(ClientID)) return;

        GameObject Player = NetworkManager.Singleton.ConnectedClients[ClientID].PlayerObject.transform.gameObject;

        //Remove player from team
       // _Teams[(int)Player.GetComponent<PlayerTeamManager>().GetTeam()].Players.Remove((int)ClientID);

        //Add player to new team
        //_Teams[(int)Team].Players.Add((int)ClientID);

        Player.GetComponent<PlayerTeamManager>()._Team.Value = (TEAMS)Team;

        UpdateTeamDataClientRpc(_Teams[0], 0);
        UpdateTeamDataClientRpc(_Teams[1], 1);

    }

    [ServerRpc(RequireOwnership = false)]
    public void AddScoreToTeamServerRPC(int Score, int Team)
    {
        _Teams[Team].TeamScore += Score;
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
