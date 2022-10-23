using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;


public class WeaponManager : NetworkBehaviour
{
    [SerializeField] private float fireRate;

    [SerializeField] private Transform shotPoint;
    [SerializeField] private GameObject muzzleEffect;
    [SerializeField] private int objectPoolSize;
    [SerializeField] private RuntimeAnimatorController animController;

    private Animator anim;
    private float fireRateCoolDown;

    private List<GameObject> objectPool = new();
    private PlayerMovement player;

    bool isWeaponPickedUp = false;

    // Start is called before the first frame update
    void Start()
    {
        PickedUp();
        GenerateObjectPool();
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
            anim.SetTrigger("fire");
            fireRateCoolDown = fireRate;
            FireVoidServerRPC();
        }
    }

    public void PickedUp()
    {
        isWeaponPickedUp = true;

        player = transform.root.GetComponent<PlayerMovement>();
        player.GetWeaponInventory().SetWeaponAnimatorController(animController);
        anim = player.GetWeaponInventory().GetAnimator();
    }

    [ServerRpc]
    private void FireVoidServerRPC()
    {
        StartCoroutine(Fire());
        FireVoidClientRPC();
    }

    [ClientRpc]
    private void FireVoidClientRPC()
    {
        StartCoroutine(Fire());
    }

    IEnumerator Fire()
    {
        GameObject go = FreeObjectFromPool();
        go.transform.position = shotPoint.position;
        go.transform.rotation = shotPoint.rotation;
        go.SetActive(true);
        yield return new WaitForSeconds(3);
        go.SetActive(false);
    }
}
