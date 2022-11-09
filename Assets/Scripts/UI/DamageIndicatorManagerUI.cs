using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class DamageIndicatorManagerUI : MonoBehaviour
{
    public static DamageIndicatorManagerUI instance;

    [SerializeField] Transform damageIndicator;
    [SerializeField] Transform bloodyScreen;

    private float lerp = 1;

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
        bloodyScreen.GetComponent<Image>().color = new Color(1, 1, 1, 0);
    }

    private void Update()
    {
        BloodyScreenLerp();
    }

    /// <summary>
    /// Bloody screen around the player 
    /// </summary>
    private void BloodyScreenLerp()
    {
        lerp = Mathf.Lerp(lerp, 0, Time.deltaTime * 8);

        bloodyScreen.GetComponent<Image>().color = new Color(1, 1, 1, lerp);
    }

    /// <summary>
    /// Spawn the damage indicator (the spinning arrow next to the crosshair that indicatos the direction of the enemy)
    /// </summary>
    public void InstantiateDamageIndicator(Vector3 _enemyPos)
    {
        lerp = 1;
        GameObject go = Instantiate(damageIndicator.gameObject, transform.position, Quaternion.identity, transform);
        go.GetComponent<DamageIndicatorUI>().SetEnemyPosition(_enemyPos);
        Destroy(go, 5);
    }
}
