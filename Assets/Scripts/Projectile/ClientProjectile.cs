using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ClientProjectile : MonoBehaviour
{
    [SerializeField] LayerMask whatIsSolid;
    [SerializeField] Transform groundHitEffect;
    [SerializeField] Transform playerHitEffect;

    private float _Speed;
    private float _Life;

    private Vector3 previousPosition;

    public void SetProperties(float Speed, float Life)
    {
        _Speed = Speed;
        _Life = Life;
    }

    void Start()
    {
        previousPosition = transform.position;
        Destroy(gameObject, _Life);
    }

    void Update()
    {
        transform.Translate(transform.forward * _Speed * Time.deltaTime, Space.World);

        CheckBetweenTwoPositions();
    }

    private void CheckBetweenTwoPositions()
    {
        float distance = Vector3.Distance(previousPosition, transform.position);
        RaycastHit hit;
        if (Physics.Raycast(previousPosition, transform.forward, out hit, distance, whatIsSolid))
        {
            if (hit.transform.gameObject.tag == "Player")
            {
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
