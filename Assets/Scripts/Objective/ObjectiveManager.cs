using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;


public enum TEAMS { RED, BLUE, GREEN, YELLOW };

[System.Serializable]
public struct TEAMDATA
{
    public Color Colour;
    public List<GameObject> Players;
    public int TeamScore;
}

public class ObjectiveManager : NetworkBehaviour
{

    public TEAMDATA[] _Teams;
    List<GameObject> _Players;

    // Start is called before the first frame update
    void Start()
    {
    }

    // Update is called once per frame
    void Update()
    {
        Debug.Log("Red team score" + _Teams[0].TeamScore);
        Debug.Log("Red team has " + _Teams[0].Players.Count);

        Debug.Log("Blue team score" + _Teams[1].TeamScore);
        Debug.Log("Blue team has " + _Teams[1].Players.Count);
    }

    public Color GetTeamColour(TEAMS Team)
    {
        return _Teams[(int)Team].Colour;
    }

    [ServerRpc]
    public void SetPlayerToTeamServerRPC(int Team, ServerRpcParams serverRpcParams = default)
    {
        var ClientID = serverRpcParams.Receive.SenderClientId;
        GameObject Player = NetworkManager.Singleton.ConnectedClients[ClientID].PlayerObject.transform.gameObject;

        //Remove player from team
        _Teams[(int)Player.GetComponent<PlayerTeamManager>().GetTeam()].Players.Remove(Player);

        //Add player to new team
        _Teams[(int)Team].Players.Add(Player);

        //TellClientsAboutTeamShiftClientRPC(Team);
    }

    [ClientRpc]
    public void TellClientsAboutTeamShiftClientRPC(int Team, ClientRpcParams clientRpcParams = default)
    {
        //GameObject Player = NetworkManager.Singleton.ConnectedClients[ClientID].PlayerObject.transform.gameObject;
        if (IsOwner) return;
        //gameObject.GetChild(1).GetComponent<Renderer>().material.color = _Teams[(int)Team].Colour;
    }
}
