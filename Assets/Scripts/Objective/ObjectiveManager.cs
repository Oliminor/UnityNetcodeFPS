using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Collections;
using Unity.Netcode;

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

    private GameObject _KingOfTheHill;
    private bool _GameInProgress;
    public static ObjectiveManager instance;

    List<GameObject> _Players;

    // Start is called before the first frame update
    void Start()
    {
        DontDestroyOnLoad(this);
        instance = this;
        _KingOfTheHill = GameObject.Find("KingOfTheHill");
        _GameInProgress = false;
    }

    private void SceneManager_OnSceneEvent(SceneEvent sceneEvent)
    {
        if(sceneEvent.SceneEventType==SceneEventType.LoadComplete)
        {
            GetComponent<RespawnManager>().GetRespawnPoint();
        }
    }

    // Update is called once per frame
    void Update()
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

            if (TeamData.TeamScore >= 10)
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
        StartNewGameServerRPC();
        StartNewGameClientRPC();
    }

    [ClientRpc]
    void StartNewGameClientRPC()
    {
        _GameInProgress = true;
        Debug.Log("Hdwadawdwadsi");
        //_KingOfTheHill.SetActive(false);
        //GetComponent<MenuManager>().SetMenuState(MENUSTATES.INGAME);
        NetworkManager.LocalClient.PlayerObject.GetComponent<HealthManager>().Respawn(false);
        switch (_CurrentMode.Value) 
        {
            case (MODES.DEATHMATCH):
                Debug.Log("Starting deathmatch");
                break;
            case (MODES.KINGOFTHEHILL):
                _KingOfTheHill.SetActive(true);
                _KingOfTheHill.GetComponent<HillManager>().StartGame();
                break;
        }
    }

    [ServerRpc]
    void StartNewGameServerRPC()
    {
        for (int i = 0; i < _Teams.Length; i++)
        {
            Debug.Log("Setting team to 0 points" + i);
            SetScoreToTeamServerRPC(0, i);
        }
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
