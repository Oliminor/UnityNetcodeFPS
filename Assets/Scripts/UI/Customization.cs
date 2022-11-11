using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

public class Customization : NetworkBehaviour
{
    [SerializeField] List<GameObject> headPieceList;
    [SerializeField] Transform weaponCamera;
    [SerializeField] Transform customizationCamera;
    
    private int currentHeadPiece;
    private bool isCustomizationIsOn = false;

    NetworkVariable<int> headPieceIndex = new NetworkVariable<int>(-1);

    public bool GetIsCustomizationIsOn() { return isCustomizationIsOn; }

    public override void OnNetworkSpawn()
    {
        // Calls ActivateWeaponClientRPC, when the serverIndex changes
        headPieceIndex.OnValueChanged += ActivateCustomizationClientRPC;
        if (headPieceIndex.Value != -1)
        {
            ActivateCustomization(headPieceIndex.Value);
        }
    }

    private void Start()
    {
        customizationCamera.gameObject.SetActive(false);
        if (IsOwner)
        {
            ActivateCustomizationServerRPC(0);
        }
    }

    private void Update()
    {
        if (!IsOwner) return;

        if (Input.GetKeyDown(KeyCode.T)) ChangeCamera();
    }

    /// <summary>
    /// Change the to the Customization camera or back to the player camera
    /// </summary>
    private void ChangeCamera()
    {
        isCustomizationIsOn = !isCustomizationIsOn;

        if (isCustomizationIsOn)
        {
            weaponCamera.gameObject.SetActive(false);
            customizationCamera.gameObject.SetActive(true);
            HUD.instance.SwitchHUDOnOff(false);
        }
        else
        {
            weaponCamera.gameObject.SetActive(true);
            customizationCamera.gameObject.SetActive(false);
            HUD.instance.SwitchHUDOnOff(true);
        }
    }

    // same us above, but Client side
    [ClientRpc]
    private void ActivateCustomizationClientRPC(int _PrevIndex, int _NewIndex)
    {
        ActivateCustomization(_NewIndex);
    }
    // and the Server side
    [ServerRpc]
    public void ActivateCustomizationServerRPC(int _index)
    {
        headPieceIndex.Value = _index;
    }

    /// <summary>
    /// Activate the selected cosmetic item
    /// </summary>
    private void ActivateCustomization(int _index)
    {
        DisableAllCustomization();
        headPieceList[_index].gameObject.SetActive(true);
    }

    /// <summary>
    /// Disable all item (to prevent double item bug)
    /// </summary>
    private void DisableAllCustomization()
    {
        foreach (GameObject item in headPieceList)
        {
            item.SetActive(false);
        }            
    }

    /// <summary>
    /// Next button function for the UI
    /// </summary>
    public void NextItem()
    {
        currentHeadPiece++;

        if (currentHeadPiece >= headPieceList.Count) currentHeadPiece = 0;

        ActivateCustomizationServerRPC(currentHeadPiece);
    }

    /// <summary>
    /// Previous button function for the UI
    /// </summary>
    public void PreviousItem()
    {
        currentHeadPiece--;

        if (currentHeadPiece < 0) currentHeadPiece = headPieceList.Count - 1;

        ActivateCustomizationServerRPC(currentHeadPiece);
    }
}
