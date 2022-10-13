using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

public class ProjectileManager : NetworkBehaviour
{

    private float _Damage;
    private float _Speed;

    private Rigidbody _RigidBody;

    // Start is called before the first frame update
    void Start()
    {
        _Speed = 3;
        _Damage = 10;
        _RigidBody = GetComponent<Rigidbody>();
        _RigidBody.velocity = transform.forward * _Speed;
    }

    public void SetProperties(float Damage, float Speed)
    {
        _Speed = Speed;
        _Damage = Damage;

        _RigidBody.velocity = transform.forward * _Speed;
    }

    // Update is called once per frame
    void Update()
    {

    }

    void OnTriggerEnter(Collider Object) 
    {
        if (Object.gameObject.tag == "Player")
        {
            Object.GetComponent<HealthManager>().ChangeHealth(-_Damage);
            Destroy(gameObject);
            GetComponent<NetworkObject>().Despawn();
        }
        
    }

}
