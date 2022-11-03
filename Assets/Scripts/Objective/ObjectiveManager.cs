using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Collections;
using Unity.Netcode;
using UnityEngine.SceneManagement;
using TMPro;


public enum TEAMS { RED, BLUE, GREEN, YELLOW };

public enum MODES { DEATHMATCH, KINGOFTHEHILL};

[System.Serializable]
public struct TEAMDATA : INetworkSerializable, System.IEquatable<TEAMDATA> {

    public Color Colour;
    public int TeamScore;
    public FixedString128Bytes TeamName;

    public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
    {
        serializer.SerializeValue(ref Colour);
        serializer.SerializeValue(ref TeamScore);
        serializer.SerializeValue(ref TeamName);

    }

    public bool Equals(TEAMDATA other)
    {
        return false;
    }
}

public class ObjectiveManager : NetworkBehaviour
{

    public TEAMDATA[] _Teams;

    [SerializeField] private NetworkVariable<MODES> _CurrentMode = new NetworkVariable<MODES>(MODES.DEATHMATCH);

    [SerializeField] List<TextMeshProUGUI> _ScoreText;
    [SerializeField] GameObject _PlayerForAI;

    private GameObject _Objective;
    private bool _GameInProgress;
    public static ObjectiveManager instance;

    private List<GameObject> _Players;
    private List<GameObject> _Bots;

    // Start is called before the first frame update
    void Start()
    {
        _Bots = new List<GameObject> { };
        //DontDestroyOnLoad(this);
        instance = this;
        _GameInProgress = false;
    }

    void Awake()
    {
        _Bots = new List<GameObject> { };
        //DontDestroyOnLoad(this);
        instance = this;
        _GameInProgress = false;
        StartNewGame();
    }

    private void SceneManager_OnSceneEvent(SceneEvent sceneEvent)
    {
        if(sceneEvent.SceneEventType==SceneEventType.LoadComplete)
        {
            GetComponent<RespawnManager>().GetRespawnPoint();
        }
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        if (!_GameInProgress) return;
        int i = 0;
        string Message = "";
        foreach(TEAMDATA TeamData in _Teams)
        {
            Message = "";
            Message += string.Format("Team {0}\n", TeamData.TeamName);
            Message += string.Format("{0} Score \n", TeamData.TeamScore);
            //Message += string.Format("{0} Members \n", TeamData.Players.Count);
            //_ScoreText[i].text = Message;
            i++;

            if (TeamData.TeamScore >= 999)
            {
                EndGame();
            }
        }
    }

    public void EndGame()
    {
        _GameInProgress = false;
        if (IsServer) GetComponent<MenuManager>().SetMenuState(MENUSTATES.HOSTSETUP);
        else GetComponent<MenuManager>().SetMenuState(MENUSTATES.CLIENTSETUP);
    }

    public void StartNewGame()
    {
        _Bots.Clear();
        _Objective = GameObject.Find("KingOfTheHill");
        StartNewGameServerRPC();
        StartNewGameClientRPC();
        
    }

    [ClientRpc]
    void StartNewGameClientRPC()
    {
        _GameInProgress = true;
        //_KingOfTheHill.SetActive(false);
        //GetComponent<MenuManager>().SetMenuState(MENUSTATES.INGAME);
        NetworkManager.LocalClient.PlayerObject.GetComponent<HealthManager>().Respawn(false);
        switch (_CurrentMode.Value) 
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
        ClearAIServerRPC();
        //SpawnAIServerRPC();
        //SpawnAIServerRPC();
        //SpawnAIServerRPC();
        //SpawnAIServerRPC();
        //SpawnAIServerRPC();
        for (int i = 0; i < _Teams.Length; i++)
        {
            Debug.Log("Setting team to 0 points" + i);
            SetScoreToTeamServerRPC(0, i);
        }
    }

    [ServerRpc]
    public void SpawnAIServerRPC()
    {
        GameObject AI = Instantiate(_PlayerForAI);
        AI.GetComponent<NetworkObject>().Spawn(true);
        AI.GetComponent<NetworkObject>().RemoveOwnership();
        AI.GetComponent<PlayerTeamManager>().ChangeTeam(Random.Range(0, 1));
        AI.GetComponent<HealthManager>().Respawn(false);
        _Bots.Add(AI);
    }

    [ServerRpc]
    public void ClearAIServerRPC()
    {
        foreach(GameObject Bot in _Bots)
        {
            Bot.GetComponent<NetworkObject>().Despawn(false);
            Destroy(Bot);
        }
        _Bots.Clear();
    }

    public int GetTeamInControl()
    {
        switch (_CurrentMode.Value)
        {
            case (MODES.DEATHMATCH):
                return -1;
                break;
            case (MODES.KINGOFTHEHILL):
                return _Objective.GetComponent<HillManager>().GetTeamControlling();
                break;
        }
        return 0;
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
        return _CurrentMode.Value;
    }

    public void SetMode(int GameMode)
    {
        _CurrentMode.Value = (MODES)GameMode;
    }
    public void StartGameOnTransition()
    {
        Invoke(nameof(StartNewGame), 0.2f);
    }
}
