using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NewBehaviourScript : MonoBehaviour
{

    [SerializeField] float _Damage;
    [SerializeField] float _FireRate;
    [SerializeField] bool _Autofire;

    private float _Cooldown;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        _Cooldown -= Time.deltaTime;
    }

    public void OnShoot()
    {
        if (_Cooldown < 0)
        {
            _Cooldown = _FireRate;
        }
    }
}
