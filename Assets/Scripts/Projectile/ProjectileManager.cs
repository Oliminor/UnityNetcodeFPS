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
        _Speed = 250;
        _Damage = 33.3f;
        _RigidBody = GetComponent<Rigidbody>();
        _RigidBody.velocity = transform.forward * _Speed;
        Destroy(gameObject, 100);
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
        }
        GetComponent<NetworkObject>().Despawn();
        Destroy(gameObject);
    }

}
