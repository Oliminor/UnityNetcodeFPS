using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

public class PlayerCamera : NetworkBehaviour
{
    [SerializeField] float ySensitivity;
    [SerializeField] float xSensitivity;

    public static PlayerCamera instane;

    float xRotation;
    float yRotation;
    // Start is called before the first frame update

    private void Awake()
    {
        instane = this;
    }

    void Start()
    {
        if (!IsOwner)
        {
            gameObject.GetComponent<Camera>().enabled = false;
            gameObject.GetComponent<AudioListener>().enabled = false;
        }
        // Lock and switch off cursor
        //Cursor.lockState = CursorLockMode.Locked;
        //Cursor.visible = false;
    }

    // Update is called once per frame
    void Update()
    {
        if (!IsOwner) return;
        CameraMouseDirection();
    }

    /// <summary>
    /// Change camera direction with mouse movement
    /// </summary>
    private void CameraMouseDirection()
    {
        float yRot = Input.GetAxisRaw("Mouse X") * xSensitivity;
        float xRot = Input.GetAxisRaw("Mouse Y") * ySensitivity;

        xRotation += xRot;
        yRotation += yRot;

        xRotation = Mathf.Clamp(xRotation, -90f, 90f);

        transform.rotation = Quaternion.Euler(xRotation, yRotation, 0);
    }
}
