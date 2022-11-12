using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerRaycast : MonoBehaviour
{
    PlayerMovement playerMovement;
    Ray ray;
    private RaycastHit hit;

    // Start is called before the first frame update
    private void Awake()
    {
        playerMovement=gameObject.GetComponent<PlayerMovement>();
    }
   

    // Update is called once per frame
    void Update()
    {
        ray = playerMovement.GetPlayerCamera().GetComponent<Camera>().ViewportPointToRay(new Vector3(0.5F, 0.5F, 0));
        if (Physics.Raycast(ray, out hit))
        {
            if(hit.transform.tag=="CameraButton")
            {
                if(Input.GetKeyDown(KeyCode.E))
                {
                    hit.transform.gameObject.GetComponent<CameraSnapshot>().VirtualPhoto();
                }
            }
        }
    }
}
