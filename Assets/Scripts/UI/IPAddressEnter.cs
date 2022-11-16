using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using TMPro;

public class IPAddressEnter : NetworkBehaviour
{

    [SerializeField] TMP_InputField _IPInput;
    [SerializeField] GameObject _NetworkManager;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        _NetworkManager.GetComponent<UnityTransport>().ConnectionData.Address = _IPInput.text.ToString();
    }
}
