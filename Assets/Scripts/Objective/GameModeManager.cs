using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Collections;
using Unity.Netcode;

[System.Serializable]
public struct GameModeData : INetworkSerializable
{
    public MODES Mode;
    public float TimeLimit;
    public int ScoreLimit;
    public bool HasWeapons;
    public int Team1Weapons;
    public int Team2Weapons;
    public int Team1MaxHealth;
    public int Team2MaxHealth;
    public FixedString128Bytes Desc;

    public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
    {
        serializer.SerializeValue(ref Mode);
        serializer.SerializeValue(ref TimeLimit);
        serializer.SerializeValue(ref ScoreLimit);
        serializer.SerializeValue(ref HasWeapons);
        serializer.SerializeValue(ref Team1Weapons);
        serializer.SerializeValue(ref Team2Weapons);
        serializer.SerializeValue(ref Team1MaxHealth);
        serializer.SerializeValue(ref Team2MaxHealth);
        serializer.SerializeValue(ref Desc);
    }
}

public class GameModeManager : NetworkBehaviour
{

    [SerializeField] private List<GameModeData> _ModeList;
    [SerializeField] private GameObject _Scoreboard;
    public NetworkVariable<bool> _InLobby = new NetworkVariable<bool>(true);
    public bool _ViewScoreboard = false;

    [SerializeField] public NetworkVariable<GameModeData> _CurrentMode = new NetworkVariable<GameModeData>(new GameModeData { }, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);
    [SerializeField] public NetworkVariable<int> _CurrentModeInt = new NetworkVariable<int>(0, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);

    // Start is called before the first frame update
    void Start()
    {
        //_ModeList.Add(_TDM);
        //_ModeList.Add(_KOTH);
        if (!IsServer) return;

        _CurrentMode.Value=_ModeList[0];

        /*SetDesc(0, "Kill The Enemy Team!\n25 Points To Win!");
        SetDesc(1, "1 V 1 Competitive!\n10 Points To Win!");
        SetDesc(2, "Control The Hill(Grey Mark)!\n60 Points To Win!");
        SetDesc(3, "Control The Hill(Grey Mark) With Snipers!\n10 Points To Win!");
        SetDesc(4, "Infection!\nBlue Convert Red\nRed Survive 120 Seconds Or Blue Kill All Red!");
        SetDesc(5, "Infection! Red Must Hide As Blue Can't Die\nRed Survive 120 Seconds Or Blue Kill All Red!");
        SetDesc(6, "One Flag!\nRed Must Defend Flag(Yellow Mark) And Blue Must bring Flag(Yellow Mark) Towards Red Mark!");
        SetDesc(7, "One Flag!\nRed Must Defend Flag(Yellow Mark) And Blue Must bring Flag(Yellow Mark) Towards Red Mark! \nBut With Increased Health And Different Loadouts!");*/
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKey(KeyCode.Tab) || _InLobby.Value || _ViewScoreboard)
        {
            _Scoreboard.SetActive(true);
            foreach(GameObject Player in GameObject.FindGameObjectsWithTag("Player"))
            {
                Player.GetComponent<PlayerStats>().UpdateScoreBoard();
            }
        }
        else
        {
            _Scoreboard.SetActive(false);
        }
    }

    public void SetDesc(int I, string String)
    {
        GameModeData Data = _ModeList[I];
        _ModeList[I] = new GameModeData
        {
            Mode = Data.Mode,
            TimeLimit = Data.TimeLimit,
            ScoreLimit = Data.ScoreLimit,
            HasWeapons = Data.HasWeapons,
            Team1Weapons = Data.Team1Weapons,
            Team2Weapons = Data.Team2Weapons,
            Team1MaxHealth = Data.Team1MaxHealth,
            Team2MaxHealth = Data.Team2MaxHealth,
            Desc = String,
        };
    }

    [ServerRpc(RequireOwnership = false)]
    public void SetModeServerRPC(int Index)
    {
        GameModeData Data = _ModeList[Index];
        _CurrentMode.Value = Data;
        _CurrentModeInt.Value = Index;
        Debug.Log(_CurrentMode.Value.Mode);
    }

    public GameModeData GetCurrentMode()
    {
        return _CurrentMode.Value;
    }
}
