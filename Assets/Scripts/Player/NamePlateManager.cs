using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
using UnityEngine;
using Unity.Netcode;
using TMPro;

public class NamePlateManager : NetworkBehaviour
{
    [SerializeField] TextMeshProUGUI playerNameText;
    [SerializeField] RectTransform playerHealthBar;
    [SerializeField] RectTransform LerpHealthBar;


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
        if (IsOwner)
        {
            playerNameText.gameObject.SetActive(false);
            playerHealthBar.gameObject.SetActive(false);
            LerpHealthBar.gameObject.SetActive(false);
        }
    }

    void Update()
    {
        PlayerUIOrientation();
        PlayerHealthUpdate();
    }

    /// <summary>
    ///  Updates the player health bar above the head
    /// </summary>
    private void PlayerHealthUpdate()
    {
        float scaleX = gameObject.GetComponent<HealthManager>().GetPercentHealth() / 100.0f;
        playerHealthBar.localScale = new Vector3(scaleX, 1, 1);

        LerpHealthBar.localScale = new Vector3(Mathf.Lerp(LerpHealthBar.localScale.x, scaleX, 0.05f), 1, 1);
    }

    /// <summary>
    /// Orient the player health (and name) to the main camera)
    /// </summary>
    private void PlayerUIOrientation()
    {
        if (playerNameText.text != playerName.Value.ToString()) SetPlayerName(playerName.Value.ToString());
        if (Camera.main) playerNameText.transform.parent.parent.rotation = Camera.main.transform.rotation;
    }
}
