using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraSnapshot : MonoBehaviour
{
    [SerializeField] GameObject cameraObject;
    Camera camera;
    // Start is called before the first frame update
    void Start()
    {
        camera = cameraObject.GetComponentInChildren<Camera>();
    }

    public void VirtualPhoto()
    {
        RenderTexture screenShot = new RenderTexture(Screen.width, Screen.height, 16);
        camera.targetTexture = screenShot;
        RenderTexture.active = screenShot;
        camera.Render();
        Texture2D texture = new Texture2D(Screen.width, Screen.height);
        RenderTexture.active = null;
        byte[] byteArray = texture.EncodeToPNG();
        System.IO.File.WriteAllBytes(Application.dataPath + "/GroupPhoto.png", byteArray);
    }
}
