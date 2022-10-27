using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Collections;
using Unity.Netcode;
using TMPro;


public enum TEAMS { RED, BLUE, GREEN, YELLOW };

[System.Serializable]
public struct TEAMDATA : INetworkSerializable
{
    public FixedString128Bytes TeamName;
    public Color Colour;
    //public List<int> Players;
    public int TeamScore;

    public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
    {
        serializer.SerializeValue(ref Colour);
        serializer.SerializeValue(ref TeamScore);
        serializer.SerializeValue(ref TeamName);
    }
}

public class ObjectiveManager : NetworkBehaviour
{
    //public NetworkVariable<TEAMDATA> _Team[4] = new NetworkVariable<TEAMDATA>(NetworkVariableReadPermission.Everyone, NetwNetworkVariable<TEAMDATA>(NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.ServorkVariableWritePermission.Server);
    //public TEAMDATA[] _Teams;// = new er);

    private NetworkVariable<TEAMDATA>[4] _Teams = new NetworkVariable<TEAMDATA>[4](TEAMDATA[4] { }, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);

    public NetworkVariable<TEAMDATA> _TeamRed = new NetworkVariable<TEAMDATA>(new TEAMDATA { }, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);

    [SerializeField] List<TextMeshProUGUI> _ScoreText;

    List<GameObject> _Players;

    // Start is called before the first frame update
    void Start()
    {
    }

    // Update is called once per frame
    void Update()
    {
        int i = 0;
        string Message = "";
        foreach(TEAMDATA TeamData in _Teams)
        {
            Message = "";
            Message += string.Format("Team {0}\n", TeamData.TeamName);
            Message += string.Format("{0} Score \n", TeamData.TeamScore);
           // Message += string.Format("{0} Members \n", TeamData.Players.Count);
            _ScoreText[i].text = Message;
            i++;
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

        Debug.Log(ClientID);

        if (!NetworkManager.ConnectedClients.ContainsKey(ClientID)) return;

        GameObject Player = NetworkManager.Singleton.ConnectedClients[ClientID].PlayerObject.transform.gameObject;

        int ClientIDint = (int)ClientID;

        //Remove player from team
        //_Teams[(int)Player.GetComponent<PlayerTeamManager>().GetTeam()].Players.Remove(ClientIDint);

        //Add player to new team
       // _Teams[(int)Team].Players.Add(ClientIDint);

        Player.GetComponent<PlayerTeamManager>()._Team.Value = (TEAMS)Team;

        //UpdateTeamDataClientRpc(_Teams[0], 0);
        //UpdateTeamDataClientRpc(_Teams[1], 1);

    }

    //[ClientRpc]
    //public void UpdateTeamDataClientRpc(TEAMDATA TeamData, int i)
    //{
    //    _Teams[i] = TeamData;
    //}
}
