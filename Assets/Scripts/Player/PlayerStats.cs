using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using Unity.Netcode;
[System.Serializable]
public struct PlayerStatData : INetworkSerializable
{

    public int Kills;
    public int Deaths;
    public int Score;

    public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
    {
        if (serializer.IsReader)
        {
            serializer.GetFastBufferReader().ReadValueSafe(out Kills);
            serializer.GetFastBufferReader().ReadValueSafe(out Deaths);
            serializer.GetFastBufferReader().ReadValueSafe(out Score);
        }
        else
        {
            serializer.GetFastBufferWriter().WriteValueSafe(Kills);
            serializer.GetFastBufferWriter().WriteValueSafe(Deaths);
            serializer.GetFastBufferWriter().WriteValueSafe(Score);
        }
    }

}

public class PlayerStats : NetworkBehaviour
{
    [SerializeField] private GameObject _ScoreBoardStats;
    private GameObject _ScoreBoard;
    private bool _SearchForScoreBoard = true;

    private NetworkVariable<PlayerStatData> _Stats = new NetworkVariable<PlayerStatData>(default, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);

    // Start is called before the first frame update
    void Awake()
    {
        _ScoreBoardStats = Instantiate(_ScoreBoardStats);
        _ScoreBoardStats.transform.localScale = new Vector3(1, 1, 1);
        UpdateScoreBoard();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void UpdateScoreBoard()
    {
        _ScoreBoard = GameObject.Find("ScoreBoard");
        _ScoreBoardStats.transform.SetParent(_ScoreBoard.transform.GetChild((int)GetComponent<PlayerTeamManager>().GetTeam()).GetChild(2));
        _ScoreBoardStats.transform.GetChild(0).GetComponent<TextMeshProUGUI>().text = GetComponent<NamePlateManager>().GetPlayerName().ToString();
        _ScoreBoardStats.transform.GetChild(1).GetComponent<TextMeshProUGUI>().text = _Stats.Value.Score.ToString();
        _ScoreBoardStats.transform.GetChild(2).GetComponent<TextMeshProUGUI>().text = _Stats.Value.Kills.ToString();
        _ScoreBoardStats.transform.GetChild(3).GetComponent<TextMeshProUGUI>().text = _Stats.Value.Deaths.ToString();
    }

    [ServerRpc(RequireOwnership = false)]
    public void ResetStatsServerRPC()
    {
        _Stats.Value = new PlayerStatData { };
    }

    [ServerRpc]
    public void AddScoreServerRPC(int Score)
    {
        _Stats.Value = new PlayerStatData
        {
            Score = _Stats.Value.Score + Score,
            Kills = _Stats.Value.Kills,
            Deaths = _Stats.Value.Deaths
        };
    }

    [ServerRpc(RequireOwnership = false)]
    public void AddKillsServerRPC(int Kills)
    {
        _Stats.Value = new PlayerStatData
        {
            Score = _Stats.Value.Score,
            Kills = _Stats.Value.Kills + Kills,
            Deaths = _Stats.Value.Deaths
        };
    }

    [ServerRpc(RequireOwnership = false)]
    public void AddDeathsServerRPC(int Deaths)
    {
        _Stats.Value = new PlayerStatData
        {
            Score = _Stats.Value.Score,
            Kills = _Stats.Value.Kills,
            Deaths = _Stats.Value.Deaths + Deaths
        };
    }
}
