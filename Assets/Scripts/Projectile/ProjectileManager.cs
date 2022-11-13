using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

public class ProjectileManager : NetworkBehaviour
{
    [SerializeField] LayerMask whatIsSolid;
    [SerializeField] Transform groundHitEffect;
    [SerializeField] Transform playerHitEffect;
    private float _Damage;
    private float _Speed;
    private float _Life;
    private GameObject _Owner;

    private Rigidbody _RigidBody;
    private Vector3 previousPosition;

    // Start is called before the first frame update
    void Start()
    {
        previousPosition = transform.position;
        _RigidBody = GetComponent<Rigidbody>();
        Destroy(gameObject, _Life);
        //StartCoroutine(TimeBeforeDestroyed(_Life));
    }

    /// <summary>
    /// Set the projectile variables
    /// </summary>
    public void SetProperties(float Damage, float Speed, float Life, GameObject Owner)
    {
        _Damage = Damage;
        _Speed = Speed;
        _Life = Life;
        _Owner = Owner;
    }

    // Update is called once per frame
    void Update()
    {
        transform.Translate(transform.forward * _Speed * Time.deltaTime, Space.World);

        //if (!IsOwner) return;

        CheckBetweenTwoPositions();
    }

    /// <summary>
    /// Hit player check and send the damage to the player
    /// </summary>
    private void CheckBetweenTwoPositions()
    {
        float distance = Vector3.Distance(previousPosition, transform.position);
        RaycastHit hit;

        if (Physics.Raycast(previousPosition, transform.forward, out hit, distance, whatIsSolid))
        {
            if (hit.transform.gameObject.tag == "Player")
            {
                hit.transform.gameObject.GetComponent<HealthManager>().DamagePlayer(_Damage, _Owner);
                GameObject playerHitParticle = Instantiate(playerHitEffect.gameObject, hit.point, Quaternion.identity);
                Destroy(playerHitParticle, 0.5f);
                Destroy(gameObject);
                return;
            }
            GameObject groundHitParticle = Instantiate(groundHitEffect.gameObject, hit.point, Quaternion.identity);
            Destroy(groundHitParticle, 0.5f);
            Destroy(gameObject);
        }
        previousPosition = transform.position;
    }
}
