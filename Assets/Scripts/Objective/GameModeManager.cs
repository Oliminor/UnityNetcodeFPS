using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

[System.Serializable]
public struct GameModeData : INetworkSerializable
{
    public MODES Mode;
    public float TimeLimit;
    public int ScoreLimit;
    public bool FFA;//This may be cut, we'll see lol
    public int Team1Weapons;
    public int Team2Weapons;
    public int RoundLimit;

    public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
    {
        serializer.SerializeValue(ref Mode);
        serializer.SerializeValue(ref TimeLimit);
        serializer.SerializeValue(ref ScoreLimit);
        serializer.SerializeValue(ref FFA);
        serializer.SerializeValue(ref Team1Weapons);
        serializer.SerializeValue(ref Team2Weapons);
        serializer.SerializeValue(ref RoundLimit);
    }
}

public class GameModeManager : MonoBehaviour
{

   /* private GameModeData _TDM = new GameModeData
    {
    Mode = MODES.DEATHMATCH,
    TimeLimit = 10 * 60,
    ScoreLimit = 10,
    FFA = false,
    Team1Weapons = 0,
    Team2Weapons = 0,
    RoundLimit = 1,
    };

    private GameModeData _KOTH = new GameModeData
    {
        Mode = MODES.KINGOFTHEHILL,
        TimeLimit = 10 * 60,
        ScoreLimit = 10,
        FFA = false,
        Team1Weapons = 0,
        Team2Weapons = 0,
        RoundLimit = 1,
    };*/

    [SerializeField] private List<GameModeData> _ModeList;

    private NetworkVariable<GameModeData> _CurrentMode = new NetworkVariable<GameModeData>(default);

    // Start is called before the first frame update
    void Start()
    {
        //_ModeList.Add(_TDM);
        //_ModeList.Add(_KOTH);
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void SetMode(int Index)
    {
        
        _CurrentMode.Value = _ModeList[Index];
        Debug.Log(_CurrentMode.Value.Mode);
    }

    public GameModeData GetCurrentMode()
    {
        return _CurrentMode.Value;
    }
}
