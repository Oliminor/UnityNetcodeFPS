using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;
[System.Serializable]
public struct PlayerStatData : INetworkSerializable
{

    public int Kills;
    public int Deaths;
    public int Score;

    public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
    {
        serializer.SerializeValue(ref Kills);
        serializer.SerializeValue(ref Deaths);
        serializer.SerializeValue(ref Score);
    }

    public void AddScore(int Delta)
    {
        Score += Delta;
    }

    public void AddKills(int Delta)
    {
        Kills += Delta;
    }

    public void AddDeaths(int Delta)
    {
        Deaths += Delta;
    }

    public void Reset()
    {
        Kills = 0;
        Deaths = 0;
        Score = 0;
    }

}

public class PlayerStats : NetworkBehaviour
{

    public NetworkVariable<PlayerStatData> _Stats = new NetworkVariable<PlayerStatData>(default, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);

    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
