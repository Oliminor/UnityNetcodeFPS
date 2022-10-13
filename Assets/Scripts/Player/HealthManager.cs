using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

public class HealthManager : NetworkBehaviour
{

    //private float _HealthCur;
    [SerializeField] NetworkVariable<float> _HealthCur = new NetworkVariable<float>(1);
    [SerializeField] float _HealthMax;
    [SerializeField] float _HealthRegen;
    [SerializeField] float _HealthCooldown;

    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {

    }

    public void ChangeHealth(float Delta)
    {
        _HealthCur.Value += Delta;
        if (_HealthCur.Value < 0)
        {
            transform.position = new Vector3(0, 10, 0);
            _HealthCur.Value = _HealthMax;
        }
        else if (_HealthCur.Value > _HealthMax)
        {
            _HealthCur.Value = _HealthMax;
        }
    }
}
