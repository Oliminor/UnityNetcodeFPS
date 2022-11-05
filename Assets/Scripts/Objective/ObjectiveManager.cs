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

    [SerializeField] private NetworkVariable<MODES> _CurrentMode = new NetworkVariable<MODES>(MODES.KINGOFTHEHILL);
    private int _MaxScore;

    [SerializeField] List<TextMeshProUGUI> _ScoreText;
    [SerializeField] GameObject _PlayerForAI;

    [SerializeField] private GameObject _KingOfTheHill;
    private bool _GameInProgress;
    private GameObject _SceneManager;
    //public static ObjectiveManager instance;

    private List<GameObject> _Players;
    private List<GameObject> _Bots;

    public TextMeshProUGUI text; //TESTING SYNC CAUSE MY UNITY EDITOR IS TRASH- OLLIE

    // Start is called before the first frame update
    private void Awake()
    {
        NetworkManager.SceneManager.OnLoadEventCompleted += SceneManagement_OnLoadEventCompleted;
        _SceneManager = GameObject.Find("SceneManager");
        //NetworkManager.SceneManager.OnUnload += SceneManagement_OnUnload;
    }
    void Start()
    {
        _Bots = new List<GameObject> { };
       // instance = this;
        _GameInProgress = false;
        _SceneManager = GameObject.Find("SceneManager");
    }

    private void SceneManagement_OnLoadEventCompleted(string sceneName, LoadSceneMode loadSceneMode, List<ulong> clientsCompleted, List<ulong> clientsTimedOut)
    {
        if (IsServer)
        {
            if(sceneName==ProjectNetworkSceneManager.sceneNames[2]&&clientsCompleted.Count==ProjectNetworkSceneManager.singleton.playersConnected.Value)
            {
                _SceneManager = GameObject.Find("SceneManager");
                StartNewGame(_SceneManager.GetComponent<GameModeManager>().GetCurrentMode());
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
        if (!_GameInProgress) return;
        if (!IsServer) return;
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
            Debug.Log("Max score" + _MaxScore);
            if (TeamData.TeamScore >= _MaxScore)
            {
                EndGame();
            }
        }

    }

    public void EndGame()
    {
        _KingOfTheHill.SetActive(false);
        _GameInProgress = false;

        Debug.Log("GameOver");

        _SceneManager.GetComponent<ProjectNetworkSceneManager>().ExitGameMode();
    }

    public void StartNewGame(GameModeData ModeData)
    {
        _GameInProgress = true;
        SetGameModeSettings(ModeData);
        StartNewGameServerRPC();
        StartNewGameClientRPC();
    }

    public void SetGameModeSettings(GameModeData ModeData)
    {
        _CurrentMode.Value = ModeData.Mode;
        _MaxScore = ModeData.ScoreLimit;
    }

    [ClientRpc]
    void StartNewGameClientRPC()
    {
        _GameInProgress = true;
        _KingOfTheHill.SetActive(false);
        //GetComponent<MenuManager>().SetMenuState(MENUSTATES.INGAME);
        NetworkManager.LocalClient.PlayerObject.GetComponent<HealthManager>().Respawn(false);
        switch (_CurrentMode.Value) 
        {
            case (MODES.DEATHMATCH):
                Debug.Log("Starting deathmatch");
                break;
            case (MODES.KINGOFTHEHILL):
                _KingOfTheHill.SetActive(true);
                Debug.Log("Starting King of the hill");
                break;
        }
    }

    [ServerRpc]
    void StartNewGameServerRPC()
    {
        _GameInProgress = true;
        //ClearAIServerRPC();
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
        //_CurrentMode.Value = (MODES)GameMode;
    }
}
