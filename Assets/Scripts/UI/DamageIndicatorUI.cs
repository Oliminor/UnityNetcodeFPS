using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class DamageIndicatorUI : MonoBehaviour
{
    private Vector3 enemyPos;
    private Vector3 cameraPos;

    private Color defaultColor;
    private float alphaLerp;

    // Start is called before the first frame update
    private void Awake()
    {
        defaultColor = GetComponent<Image>().color;
    }

    // Update is called once per frame
    void Update()
    {
        RotateIndicatorTowardsEnemy();
    }

    /// <summary>
    /// The Indicator rotates and lerp at the same time
    /// </summary>
    private void RotateIndicatorTowardsEnemy()
    {
        if (enemyPos == Vector3.zero) return;

        alphaLerp = Mathf.Lerp(alphaLerp, 0, Time.deltaTime * 10);
        GetComponent<Image>().color = new Color(defaultColor.r, defaultColor.b, defaultColor.g, alphaLerp);

        if (!Camera.main) return;

        cameraPos = Camera.main.transform.position;

        Vector3 diraction = enemyPos - cameraPos;

        Quaternion rotation = Quaternion.LookRotation(diraction);

        Vector3 cameraY = new Vector3(0, 0, Camera.main.transform.eulerAngles.y);

        transform.rotation = Quaternion.Euler(0, 0, -rotation.eulerAngles.y) * Quaternion.Euler(cameraY);
    }

    /// <summary>
    /// Sets the Indicator variables and enemy position
    /// </summary>
    public void SetEnemyPosition(Vector3 _enemyPos)
    {
        enemyPos = _enemyPos;
        alphaLerp = 1;
        GetComponent<Image>().color = new Color(defaultColor.r, defaultColor.b, defaultColor.g, 0);
    }
}
