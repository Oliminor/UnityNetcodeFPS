using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;
using Unity.Netcode.Components;


public class WeaponManager : NetworkBehaviour
{
    [SerializeField] private float fireRate;

    [SerializeField] private Transform shotPoint;
    [SerializeField] private Transform projectile;
    [SerializeField] private GameObject muzzleEffect;
    [SerializeField] private int objectPoolSize;
    [SerializeField] private RuntimeAnimatorController animController;
    [SerializeField] private Vector3 rotation;

    [SerializeField] private float _Damage;
    [SerializeField] private float _Velocity;


    private Animator anim;
    private NetworkAnimator netAnim;
    private float fireRateCoolDown;

    private List<GameObject> objectPool = new();
    private PlayerMovement player;

    bool isWeaponPickedUp = false;

    // Start is called before the first frame update
    void Start()
    {
        GenerateObjectPool();
        PickedUp();
    }

    // Update is called once per frame
    void Update()
    {
        if (!isWeaponPickedUp) return;

        if (!player.IsOwner) return;

        anim.SetFloat("speed", player.GetAnimSpeed());
        anim.SetBool("isAiming", player.IsAiming());

        Shooting();
    }

    /// <summary>
    /// Instantiate the object pool objects
    /// </summary>
    private void GenerateObjectPool()
    {
        for (int i = 0; i < objectPoolSize; i++)
        {
            GameObject go = Instantiate(muzzleEffect, transform);
            go.SetActive(false);
            objectPool.Add(go);
        }
    }

    /// <summary>
    /// Find a free object from the object pool
    /// </summary>
    private GameObject FreeObjectFromPool()
    {
        int index = 0;

        for (int i = 0; i < objectPoolSize; i++)
        {
            if (!objectPool[i].activeSelf)
            {
                index = i;
                break;
            }
        }

        return objectPool[index];
    }

    /// <summary>
    /// Checks if the player shoots or not
    /// </summary>
    private bool IsShooting()
    {
        bool _isShooting = false;

        if (Input.GetMouseButton(0)) _isShooting = true;

        return _isShooting;
    }

    /// <summary>
    /// The shooting functions (Effect only at the moment)
    /// </summary>
    private void Shooting()
    {
        fireRateCoolDown -= Time.deltaTime;

        if (fireRateCoolDown <= 0 && IsShooting() && (!player.IsRunning() || player.IsAiming()))
        {
            netAnim.SetTrigger("fire");
            fireRateCoolDown = fireRate;
            StartCoroutine(Fire());
            FireVoidServerRPC();
        }
    }

    /// <summary>
    /// Calls this when the player Picks up a new item (it's a mess)
    /// </summary>
    public void PickedUp()
    {
        isWeaponPickedUp = true;

        transform.localPosition = Vector3.zero;
        transform.localRotation = Quaternion.Euler(rotation);

        player = transform.root.GetComponent<PlayerMovement>();
        player.GetWeaponInventory().SetWeaponAnimatorController(animController);
        anim = player.GetWeaponInventory().GetAnimator();
        netAnim = player.GetWeaponInventory().GetComponent<NetworkAnimator>();

        for (int i = 0; i < objectPool.Count; i++) objectPool[i].SetActive(false);
    }

    /// <summary>
    ///  aim to the center of the screen/camera 
    /// </summary>
    private Vector3 FireDirection()
    {
        Ray ray = player.GetPlayerCamera().GetComponent<Camera>().ViewportPointToRay(new Vector3(0.5F, 0.5F, 0));
        Vector3 targetDirection;
        if (Physics.Raycast(ray, out RaycastHit hit)) targetDirection = hit.point;
        else targetDirection = ray.GetPoint(100);

        return targetDirection;
    }

    /// <summary>
    /// Calls the fire on the server side
    /// </summary>
    [ServerRpc]
    private void FireVoidServerRPC()
    {
        GameObject _projectile = Instantiate(projectile.gameObject, shotPoint.position, Quaternion.identity);
        _projectile.GetComponent<ProjectileManager>().SetProperties(_Damage, _Velocity, 3, transform.root.gameObject);
        _projectile.GetComponent<NetworkObject>().Spawn();
        
        _projectile.transform.LookAt(FireDirection());

        FireVoidClientRPC();
    }

    /// <summary>
    /// Calls the fire on the client side
    /// </summary>
    [ClientRpc]
    private void FireVoidClientRPC()
    {
        if(!player.IsOwner) StartCoroutine(Fire());
    }

    /// <summary>
    /// The fire function itself (Effect only at the moment)
    /// </summary>
    IEnumerator Fire()
    {
        GameObject go = FreeObjectFromPool();
        go.transform.position = shotPoint.position;
        go.transform.rotation = shotPoint.rotation;
        go.SetActive(true);
        yield return new WaitForSeconds(1);
        go.SetActive(false);
    }
}
