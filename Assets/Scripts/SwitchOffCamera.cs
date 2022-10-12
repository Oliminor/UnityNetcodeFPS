using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SwitchOffCamera : MonoBehaviour
{
    // Switches off the ObserverCamera, otherwise every player using that camera as default
    void Awake()
    {
        gameObject.GetComponent<Camera>().enabled = false;
    }

}
