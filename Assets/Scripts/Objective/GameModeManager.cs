using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

public class GameModeManager : MonoBehaviour
{

    public struct GameModeData : INetworkSerializable{
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

    private GameModeData _TDM = new GameModeData
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
    };

    private GameModeData _CurrentMode;

    // Start is called before the first frame update
    void Start()
    {
        _CurrentMode = _TDM;
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
