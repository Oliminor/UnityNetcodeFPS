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
    public bool HasWeapons;
    public int Team1Weapons;
    public int Team2Weapons;
    public int Team1MaxHealth;
    public int Team2MaxHealth;

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
    }
}

public class GameModeManager : NetworkBehaviour
{

    [SerializeField] private List<GameModeData> _ModeList;
    [SerializeField] private GameObject _Scoreboard;
    public NetworkVariable<bool> _InLobby = new NetworkVariable<bool>(true);

    [SerializeField] public NetworkVariable<GameModeData> _CurrentMode = new NetworkVariable<GameModeData>(new GameModeData { }, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);
    [SerializeField] public NetworkVariable<int> _CurrentModeInt = new NetworkVariable<int>(0, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);

    // Start is called before the first frame update
    void Start()
    {
        //_ModeList.Add(_TDM);
        //_ModeList.Add(_KOTH);
        if (!IsServer) return;
        _CurrentMode.Value=_ModeList[0];
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKey(KeyCode.Tab) || _InLobby.Value)
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
