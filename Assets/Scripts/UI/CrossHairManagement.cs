using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CrossHairManagement : MonoBehaviour
{
    public static CrossHairManagement instance;

    [SerializeField] RectTransform CrossHairBase;

    private float lerpPosition;
    private float spreadValue;
    private float defaultSpreadValue;

    private Vector3 defaultPos;
    // Start is called before the first frame update
    void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(this);
        }
        else
        {
            Destroy(this.gameObject);
        }
    }

    // Update is called once per frame
    void Update()
    {
        CrossHairLerp();
    }

    /// <summary>
    /// Lerping the crosshair (works with magic)
    /// </summary>
    private void CrossHairLerp()
    {
        spreadValue = Mathf.Lerp(spreadValue, 0, Time.deltaTime * 3);
        lerpPosition = Mathf.Lerp(lerpPosition, defaultSpreadValue, Time.deltaTime * 20);

        for (int i = 0; i < CrossHairBase.childCount; i++)
        {
            Vector3 pos = CrossHairBase.GetChild(i).transform.position;
            defaultPos = new Vector3(Screen.width / 2, Screen.height / 2, 0);

            switch (i)
            {
                case 0:
                    pos = new Vector3(defaultPos.x, defaultPos.y + lerpPosition + spreadValue, defaultPos.z);
                    break;
                case 1:
                    pos = new Vector3(defaultPos.x + lerpPosition + spreadValue, defaultPos.y, defaultPos.z);
                    break;
                case 2:
                    pos = new Vector3(defaultPos.x, defaultPos.y - lerpPosition - spreadValue, defaultPos.z);
                    break;
                case 3:
                    pos = new Vector3(defaultPos.x - lerpPosition - spreadValue, defaultPos.y, defaultPos.z);
                    break;
            }

            CrossHairBase.GetChild(i).transform.position = pos;
        }
    }

    /// <summary>
    /// The additional size when the player shoots
    /// </summary>
    public void SetSpreadValue(float _spreadValue)
    {
        spreadValue = _spreadValue;
    }

    /// <summary>
    /// Default Spread is the size of the crosshair before the attack (different size while aiming, running or walking)
    /// </summary>
    public void SetDefaultSpreadValue(float _spreadDefaultValue)
    {
        defaultSpreadValue = _spreadDefaultValue;
    }

    public void ActivateCrossHair(bool _bool)
    {
        if (_bool) CrossHairBase.gameObject.SetActive(true);
        else CrossHairBase.gameObject.SetActive(false);
    }
}
