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

    [SerializeField] private float _ZoomSizeFOV;
    [SerializeField] private float _ProjectileDamage;
    [SerializeField] private float _ProjectileSpeed;
    [SerializeField] private float _ProjectileLife;
    [SerializeField] private float _ProjectileNumber;
    [SerializeField] private float _ProjectileSpread;


    private Animator anim;
    private NetworkAnimator netAnim;
    private float fireRateCoolDown;
    private float defaultFOV = 60;
    private float lerpFOV;

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

        CrossHairManagement.instance.SetDefaultSpreadValue(DefaultCrossHairSize());

        CameraZoom();

        anim.SetFloat("speed", player.GetAnimSpeed());
        anim.SetBool("isAiming", player.IsAiming());

        Shooting();
    }


    public void CameraZoom()
    {
        lerpFOV = defaultFOV;

        if (player.IsAiming())
        {
            lerpFOV = _ZoomSizeFOV;
        }

        player.GetPlayerCamera().GetComponent<Camera>().fieldOfView = Mathf.Lerp(player.GetPlayerCamera().GetComponent<Camera>().fieldOfView, lerpFOV, Time.deltaTime * 20);
    }

    /// <summary>
    /// Instantiate the object pool objects
    /// </summary>
    private void GenerateObjectPool()
    {
        for (int i = 0; i < objectPoolSize; i++)
        {
            GameObject go = Instantiate(muzzleEffect, transform);
            if (IsOwner) go.gameObject.layer = LayerMask.NameToLayer("RenderOnTop");
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

        float distance = 200;

        targetDirection = ray.GetPoint(200);

        float spreadX = Random.Range(-(distance * _ProjectileSpread), (distance * _ProjectileSpread)) / 1000;
        float spreadY = Random.Range(-(distance * _ProjectileSpread), (distance * _ProjectileSpread)) / 1000;
        float spreadZ = Random.Range(-(distance * _ProjectileSpread), (distance * _ProjectileSpread)) / 1000;

        if (player.GetAnimSpeed() < 0.5f)
        {
            spreadX /= 1.5f;
            spreadY /= 1.5f;
            spreadZ /= 1.5f;
        }

        if (player.IsAiming())
        {
            spreadX /= 2.0f;
            spreadY /= 2.0f;
            spreadZ /= 2.0f;
        }

        targetDirection = new Vector3(targetDirection.x + spreadX, targetDirection.y + spreadY, targetDirection.z + spreadZ);

        return targetDirection;
    }

    private float DefaultCrossHairSize()
    {
        float _size = _ProjectileSpread / 3;

        if (player.GetAnimSpeed() < 0.5f)
        {
            _size /= 1.5f;
        }

        if (player.IsAiming()) _size /= 2.0f;

        return _size;
    }

    /// <summary>
    /// Calls the fire on the server side
    /// </summary>
    [ServerRpc]
    private void FireVoidServerRPC()
    {
        FireVoidClientRPC();

        for (int i = 0; i < _ProjectileNumber; i++)
        {
            GameObject _projectile = Instantiate(projectile.gameObject, shotPoint.position, Quaternion.identity);
            _projectile.GetComponent<NetworkObject>().Spawn();
            _projectile.GetComponent<ProjectileManager>().SetProperties(_ProjectileDamage, _ProjectileSpeed, _ProjectileLife, transform.root.gameObject);
            _projectile.transform.LookAt(FireDirection());
        }
    }

    /// <summary>
    /// Calls the fire on the client side
    /// </summary>
    [ClientRpc]
    private void FireVoidClientRPC()
    {
        if(!player.IsOwner) StartCoroutine(Fire());
        if (IsOwner) CrossHairManagement.instance.SetSpreadValue(DefaultCrossHairSize() * 2);
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
