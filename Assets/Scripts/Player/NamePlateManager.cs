using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
using UnityEngine;
using Unity.Netcode;
using TMPro;

public class NamePlateManager : NetworkBehaviour
{
    [SerializeField] TextMeshProUGUI playerNameText;
    private NetworkVariable<FixedString64Bytes> playerName = new NetworkVariable<FixedString64Bytes>("" ,NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner);

    public void SetPlayerName(string name) { playerNameText.text = name; }

    private void OnValueChanged(FixedString64Bytes oldValue, FixedString64Bytes newValue) { SetPlayerName(playerName.Value.ToString()); }

    public override void OnNetworkSpawn()
    {
        playerName.OnValueChanged += OnValueChanged;
        if (IsOwner)
        {
            playerName.Value = NetworkUI.instance.GetPlayerNameFromInput();
            SetPlayerName(playerName.Value.ToString());
        }
    }

    private void Start()
    {
        if (IsOwner) playerNameText.gameObject.SetActive(false);
    }

    void Update()
    {
        if (playerNameText.text != playerName.Value.ToString()) SetPlayerName(playerName.Value.ToString());
        playerNameText.transform.parent.LookAt(Camera.main.transform, Vector3.up);
    }
}
