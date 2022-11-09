using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class HUD : MonoBehaviour
{
    public static HUD instance;

    [SerializeField] TextMeshProUGUI healthNumberHUD;
    [SerializeField] RectTransform healthBarHUD;
    [SerializeField] RectTransform healthBarLerpHUD;

    [SerializeField] TextMeshProUGUI ammoNumberHUD;

    [SerializeField] RectTransform reloadHUD;
    [SerializeField] RectTransform reloadBarLerpHUD;

    [SerializeField] RectTransform pressButtonHUD;
    [SerializeField] TextMeshProUGUI pressButtonText;

    private float percent;
    private float currentReloadTime;
    private float maxReloadTime;
    private Vector2 reloadHUDDefaultPos;

    public void SetHUDPercent(float _percent) { percent = _percent; }
    public void SetHUDReloadTime(float _reloadTime) 
    {
        maxReloadTime = _reloadTime;
        currentReloadTime = _reloadTime;
        reloadHUD.gameObject.SetActive(true);
    }

    public void StopRealoding() { reloadHUD.gameObject.SetActive(false); }

    void Awake()
    {
        if (instance != this && instance != null)
        {
            Destroy(this.gameObject);
        }

        instance = this;
        DontDestroyOnLoad(this);
    }

    private void Start()
    {
        pressButtonHUD.gameObject.SetActive(false);
        reloadHUDDefaultPos = reloadHUD.position;
        StartCoroutine(ReloadShake());
    }

    // Update is called once per frame
    void Update()
    {
        PlayerHealthBarUpdateHUD(percent);
        ReloadBarUpdateHUD();
    }

    public void SetPlayerHealthTextHUD(int _currentHealth, int _maxHealth)
    {
        string text = _currentHealth.ToString() + "/" + _maxHealth.ToString();
        healthNumberHUD.text = text;
    }

    public void SetPlayerAmmoTextHUD(int _currentAmmo, int _maxAmmo)
    {
        string text = _currentAmmo.ToString() + "/" + _maxAmmo.ToString();

        if (_currentAmmo == 0 && _maxAmmo == 0) text = "";

        ammoNumberHUD.text = text;
    }

    private void PlayerHealthBarUpdateHUD(float _percent)
    {
        float scaleX = _percent / 100.0f;
        healthBarHUD.localScale = new Vector3(scaleX, 1, 1);

        healthBarLerpHUD.localScale = new Vector3(Mathf.Lerp(healthBarLerpHUD.localScale.x, scaleX, 0.05f), 1, 1);
    }

    private void ReloadBarUpdateHUD()
    {
        if (currentReloadTime <= 0) 
        {
            currentReloadTime = 0;
            reloadHUD.gameObject.SetActive(false);
            return;
        }

        currentReloadTime -= Time.deltaTime;

        float scaleX = currentReloadTime / maxReloadTime;

        reloadBarLerpHUD.localScale = new Vector3(1 - scaleX, 1, 1);
    }

    public void SetPickUpText(string _itemName)
    {
        pressButtonText.text = "Press <color=#FFCF07FF>F</color> to pick up the " + _itemName;
    }

    public void SetPickUpTextHUDActive(bool _bool)
    {
        pressButtonHUD.gameObject.SetActive(_bool);
    }

    IEnumerator ReloadShake()
    {
        while(true)
        {
            float shakeValueX = Random.Range(-2.0f, 2.0f);
            float shakeValueY = Random.Range(-2.0f, 2.0f);
            reloadHUD.position = new Vector2(reloadHUDDefaultPos.x + shakeValueX, reloadHUDDefaultPos.y + shakeValueY);

            yield return new WaitForSeconds(0.05f);

        }
    }
}
