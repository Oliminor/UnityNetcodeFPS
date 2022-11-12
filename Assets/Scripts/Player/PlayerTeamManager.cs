using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using Unity.Netcode;

public class PlayerTeamManager : NetworkBehaviour
{
    [SerializeField] MeshRenderer playerModel;

    public NetworkVariable<TEAMS> _Team = new NetworkVariable<TEAMS>(0, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);
    private GameObject _NetworkManager;

    public GameObject TeamYouText;

    private 

    // Start is called before the first frame update
    void Start()
    {

    }

    void Awake()
    {
        _NetworkManager = GameObject.Find("ObjectiveManager");
    }

    // Update is called once per frame
    void Update()
    {
        _NetworkManager = GameObject.Find("ObjectiveManager");
        if (!IsOwner)
        {
            Debug.Log("Color Change called");
           if (_NetworkManager) SetTeamColor(_NetworkManager.GetComponent<ObjectiveManager>().GetTeamColour(_Team.Value));
            return;
        }
        TeamYouText = GameObject.Find("Temp");
        if (!TeamYouText) return;
        TeamYouText.transform.GetChild(2).GetComponent<TextMeshProUGUI>().text = "Score: " + _Team.Value.ToString();

        if (Input.GetKeyDown("p"))
        {
            //_NetworkManager.GetComponent<ObjectiveManager>().AddScoreToTeamServerRPC(1, 1);
            if (_Team.Value == TEAMS.RED) ChangeTeam(1);
            else ChangeTeam(0);
        }
    }

    /// <summary>
    /// Set the team color
    /// </summary>
    private void SetTeamColor(Color _color)
    {
        playerModel.materials[1].SetColor("_Color", _color);
        ChatManager.singleton.SetTeamChatColour(_color);
    }

    public void ChangeTeam(int NewTeam)
    {
        if (!IsOwner || !IsServer) return;
        _NetworkManager = GameObject.Find("ObjectiveManager");
        _NetworkManager.GetComponent<ObjectiveManager>().SetPlayerToTeamServerRPC(NewTeam);
        //_Team.Value = (TEAMS)NewTeam;
        
    }

    public TEAMS GetTeam()
    {
        return _Team.Value;
    }

}
