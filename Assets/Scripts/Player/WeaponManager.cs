using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;
using Unity.Netcode.Components;


public class WeaponManager : NetworkBehaviour
{
    [SerializeField] private int index;
    [SerializeField] private float fireRate;

    [SerializeField] private Transform shotPoint;
    [SerializeField] private GameObject muzzleEffect;
    [SerializeField] private int objectPoolSize;
    [SerializeField] private RuntimeAnimatorController animController;
    [SerializeField] private Vector3 rotation;

    [SerializeField] private GameObject _ProjectilePrefab;
    [SerializeField] private float _ProjectileSpeed;
    [SerializeField] private float _ProjectileDamage;


    private Animator anim;
    private NetworkAnimator netAnim;
    private float fireRateCoolDown;

    private List<GameObject> objectPool = new();
    private PlayerMovement player;

    bool isWeaponPickedUp = false;

    public int GetIndex() { return index; }

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
    /// The shooting functions (ONLY THE EFFECTS)
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

    [ServerRpc]
    private void FireVoidServerRPC()
    {
        GameObject Proj = Instantiate(_ProjectilePrefab, transform.position + (transform.forward), transform.rotation);
        Proj.GetComponent<NetworkObject>().Spawn();
        Proj.GetComponent<ProjectileManager>().SetProperties(_ProjectileDamage, _ProjectileSpeed);
        FireVoidClientRPC();
    }

    [ClientRpc]
    private void FireVoidClientRPC()
    {
        if(!player.IsOwner) StartCoroutine(Fire());
    }

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
