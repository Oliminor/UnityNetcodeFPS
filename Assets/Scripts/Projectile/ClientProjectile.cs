using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ClientProjectile : MonoBehaviour
{
    private float _Speed;

    private Rigidbody _RigidBody;
    private Vector3 previousPosition;

    public void SetSpeed(float _speed) { _Speed = _speed; }

    void Start()
    {
        previousPosition = transform.position;
        _RigidBody = GetComponent<Rigidbody>();
    }

    void Update()
    {
        _RigidBody.velocity = transform.forward * _Speed * (Time.deltaTime * 100);

        CheckBetweenTwoPositions();
    }

    private void CheckBetweenTwoPositions()
    {
        if (Physics.Linecast(transform.position, previousPosition, out RaycastHit hit))
        {
            if (hit.transform.gameObject.tag == "Player")
            {
                Destroy(gameObject);
            }
        }
        previousPosition = transform.position;
    }
}
