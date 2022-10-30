using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum MENUSTATES { CONNECTION, HOSTSETUP, CLIENTSETUP, INGAME };

public class MenuManager : MonoBehaviour
{

    private MENUSTATES _MenuState;

    [SerializeField] private GameObject _NetworkCanvas;
    [SerializeField] private GameObject _GameSetup;
    [SerializeField] private GameObject _GameUI;

    // Start is called before the first frame update
    void Start()
    {
        _MenuState = MENUSTATES.CONNECTION;
        _NetworkCanvas.SetActive(true);
        _GameSetup.SetActive(false);
        _GameUI.SetActive(false);
    }

    // Update is called once per frame
    void CanvasUpdate()
    {
        bool NetworkCanvas = false;
        bool GameSetup = false;
        bool HostSetup = false;
        bool ClientSetup = false;
        bool GameUI = false;
        switch (_MenuState) 
        {
            case (MENUSTATES.CONNECTION):
                NetworkCanvas = true;
                break;
            case (MENUSTATES.HOSTSETUP):
                GameSetup = true;
                HostSetup = true;
                break;
            case (MENUSTATES.CLIENTSETUP):
                GameSetup = true;
                ClientSetup = true;
                break;
            case (MENUSTATES.INGAME):
                GameUI = true;
                break;
        }
        _NetworkCanvas.SetActive(NetworkCanvas);
        _GameSetup.SetActive(GameSetup);
        _GameSetup.transform.GetChild(0).gameObject.SetActive(HostSetup);
        _GameSetup.transform.GetChild(1).gameObject.SetActive(ClientSetup);
        _GameUI.SetActive(GameUI);
    }

    public void SetMenuState(MENUSTATES NewState)
    {
        _MenuState = NewState;
        CanvasUpdate();
    }
}
