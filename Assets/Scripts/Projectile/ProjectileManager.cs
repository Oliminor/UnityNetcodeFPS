using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

public class ProjectileManager : NetworkBehaviour
{

    private int _Damage;
    private float _Speed;
    private float _Life;

    private Rigidbody _RigidBody;
    private Vector3 previousPosition;

    // Start is called before the first frame update
    void Start()
    {
        previousPosition = transform.position;
        SetProperties(34, 10, 3);
        _RigidBody = GetComponent<Rigidbody>();
        StartCoroutine(TimeBeforeDestroyed(_Life));
    }

    /// <summary>
    /// Set the projectile variables
    /// </summary>
    public void SetProperties(int Damage, float Speed, float Life)
    {
        _Damage = Damage;
        _Speed = Speed;
        _Life = Life;
    }

    // Update is called once per frame
    void Update()
    {
        _RigidBody.velocity = transform.forward * _Speed * (Time.deltaTime * 100);

        if (!IsOwner) return;

        CheckBetweenTwoPositions();
    }

    /// <summary>
    /// Set the time after the projectile destroyed
    /// </summary>
    IEnumerator TimeBeforeDestroyed(float _life)
    {
        yield return new WaitForSeconds(_life);
        if (IsOwner) DamagePlayerServerRPC();
    }

    /// <summary>
    /// Despawn the projectile
    /// </summary>
    [ServerRpc]
    private void DamagePlayerServerRPC()
    {
        gameObject.GetComponent<NetworkObject>().Despawn();
    }

    /// <summary>
    /// Hit player check and send the damage to the player
    /// </summary>
    private void CheckBetweenTwoPositions()
    {
        if (Physics.Linecast(transform.position, previousPosition, out RaycastHit hit))
        {
            if (hit.transform.gameObject.tag == "Player")
            {
                DamagePlayerServerRPC();
                hit.transform.gameObject.GetComponent<HealthManager>().DamagePlayer(_Damage);
            }
        }
        previousPosition = transform.position;
    }
}
