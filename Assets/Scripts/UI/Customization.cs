using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

public class Customization : NetworkBehaviour
{
    [SerializeField] List<GameObject> headPieceList;
    [SerializeField] List<GameObject> facePieceList;
    [SerializeField] Transform weaponCamera;
    [SerializeField] Transform customizationCamera;
    
    private int currentHeadPiece;
    private int currentFacePiece;
    private bool isCustomizationIsOn = false;

    NetworkVariable<int> headPieceIndex = new NetworkVariable<int>(-1);
    NetworkVariable<int> facePieceIndex = new NetworkVariable<int>(-1);

    public bool GetIsCustomizationIsOn() { return isCustomizationIsOn; }

    public override void OnNetworkSpawn()
    {
        // Calls ActivateWeaponClientRPC, when the serverIndex changes
        headPieceIndex.OnValueChanged += ActivateHeadPieceCustomizationClientRPC;
        facePieceIndex.OnValueChanged += ActivateFacePieceCustomizationClientRPC;
      
        if (headPieceIndex.Value != -1) ActivateHeadPieceCustomization(headPieceIndex.Value);
        if (facePieceIndex.Value != -1) ActivateFacePieceCustomization(facePieceIndex.Value);
    }

    private void Start()
    {
        customizationCamera.gameObject.SetActive(false);
        if (IsOwner)
        {
            ActivateFacePieceCustomizationServerRPC(0);
            ActivateHeadPieceCustomizationServerRPC(0);
        }
    }

    private void Update()
    {
        if (!IsOwner) return;
        if (ChatManager.singleton.GetIsChatActive()) return;
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
    private void ActivateHeadPieceCustomizationClientRPC(int _PrevIndex, int _NewIndex)
    {
        ActivateHeadPieceCustomization(_NewIndex);
    }
    // and the Server side
    [ServerRpc]
    public void ActivateHeadPieceCustomizationServerRPC(int _index)
    {
        headPieceIndex.Value = _index;
    }

    /// <summary>
    /// Activate the selected cosmetic item
    /// </summary>
    private void ActivateHeadPieceCustomization(int _index)
    {
        DisableAllHeadPieceCustomization();
        headPieceList[_index].gameObject.SetActive(true);
    }

    // same us above, but Client side
    [ClientRpc]
    private void ActivateFacePieceCustomizationClientRPC(int _PrevIndex, int _NewIndex)
    {
        ActivateFacePieceCustomization(_NewIndex);
    }
    // and the Server side
    [ServerRpc]
    public void ActivateFacePieceCustomizationServerRPC(int _index)
    {
        facePieceIndex.Value = _index;
    }

    /// <summary>
    /// Activate the selected cosmetic item
    /// </summary>
    private void ActivateFacePieceCustomization(int _index)
    {
        DisableAllFacePieceCustomization();
        facePieceList[_index].gameObject.SetActive(true);
    }

    /// <summary>
    /// Disable all item (to prevent double item bug)
    /// </summary>
    private void DisableAllHeadPieceCustomization()
    {
        foreach (GameObject item in headPieceList)
        {
            item.SetActive(false);
        }            
    }

    private void DisableAllFacePieceCustomization()
    {
        foreach (GameObject item in facePieceList)
        {
            item.SetActive(false);
        }
    }

    /// <summary>
    /// Next button function for the UI
    /// </summary>
    public void NextHeadItem()
    {
        currentHeadPiece++;

        if (currentHeadPiece >= headPieceList.Count) currentHeadPiece = 0;

        ActivateHeadPieceCustomizationServerRPC(currentHeadPiece);
    }

    /// <summary>
    /// Previous button function for the UI
    /// </summary>
    public void PreviousHeadItem()
    {
        currentHeadPiece--;

        if (currentHeadPiece < 0) currentHeadPiece = headPieceList.Count - 1;

        ActivateHeadPieceCustomizationServerRPC(currentHeadPiece);
    }

    public void NextFaceItem()
    {
        currentFacePiece++;

        if (currentFacePiece >= facePieceList.Count) currentFacePiece = 0;

        ActivateFacePieceCustomizationServerRPC(currentFacePiece);
    }

    public void PreviousFaceItem()
    {
        currentFacePiece--;

        if (currentFacePiece < 0) currentFacePiece = facePieceList.Count - 1;

        ActivateFacePieceCustomizationServerRPC(currentFacePiece);
    }

}
