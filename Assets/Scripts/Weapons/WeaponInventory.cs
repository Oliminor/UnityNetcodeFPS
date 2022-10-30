using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

public class WeaponInventory : NetworkBehaviour
{

    [SerializeField] Transform[] weapons;
    [SerializeField] int[] defaultWeapons = new int[4];

    private Animator anim;
    int currentWeaponIndex;
    NetworkVariable<int> serverIndex = new NetworkVariable<int>(-1);

    PlayerMovement player;

    private bool isObjectCarried = false;
    public Animator GetAnimator() { return anim; }

    void Awake()
    {
        player = transform.root.GetComponent<PlayerMovement>();
        anim = GetComponent<Animator>();
    }

    /// <summary>
    /// Calls the weapon spawned (and when the serverIndex change for some reason, Netcode I guess)
    /// </summary>
    public override void OnNetworkSpawn()
    {
        // Calls ActivateWeaponClientRPC, when the serverIndex changes
        serverIndex.OnValueChanged += ActivateWeaponClientRPC;
        if (serverIndex.Value != -1)
        {
            ActivateWeapon(serverIndex.Value);
        }
    }

    private void Start()
    {
        if (player.IsOwner)
        {
            ActivateWeaponServerRPC(0);
        }
    }

    private void Update()
    {
        if (!player.IsOwner) return;

        DropFlag();

        if (isObjectCarried) return;

        ScrollBetweenWeapons();
    }

    /// <summary>
    /// Drops the flag and spawn a new temporary flag spawner for the other players
    /// </summary>
    private void DropFlag()
    {
        if (Input.GetKeyDown(KeyCode.R) && isObjectCarried)
        {
            weapons[serverIndex.Value].GetComponent<ObjectIndex>().DropObjectServerRPC();
            defaultWeapons[defaultWeapons.Length - 1] = 0;
            isObjectCarried = false;
            ActivateWeaponServerRPC(0);
        }
    }

    /// <summary>
    /// Add the weapon to the weapon inventory (index only, the weapons are already on the Player prefab)
    /// </summary>
    public void AddWeapon(int _index)
    {
        if (!player.IsOwner) return;

        if (weapons[_index - 1].tag == "CarriedObject")
        {
            defaultWeapons[defaultWeapons.Length - 1] = _index;
            return;
        }

        for (int i = 0; i < defaultWeapons.Length; i++)
        {
            if (defaultWeapons[i] == _index) return;
        }

        for (int i = 0; i < defaultWeapons.Length - 1; i++)
        {
            if (defaultWeapons[i] == 0)
            {
                defaultWeapons[i] = _index;
                return;
            }
        }
    }

    /// <summary>
    /// Change the weapon animator according to the type of the weapon
    /// </summary>
    public void SetWeaponAnimatorController(RuntimeAnimatorController _animController)
    {
        anim.runtimeAnimatorController = _animController;
    }

    /// <summary>
    /// Disable weapons before active the current one the player using (or pickus up weapon)
    /// </summary>
    private void DisableAllWeapons()
    {
        for (int i = 0; i < weapons.Length; i++)
        {
            weapons[i].gameObject.SetActive(false);
        }
    }

    /// <summary>
    /// Activate the weapon on the client side
    /// </summary>
    [ClientRpc]
    private void ActivateWeaponClientRPC(int _PrevIndex, int _NewIndex)
    {
        Debug.Log("Weapon scroll: " + _PrevIndex + " " + _NewIndex);
        ActivateWeapon(_NewIndex);
    }

    /// <summary>
    /// Activate the weapon on the server side
    /// </summary>
    [ServerRpc]
    public void ActivateWeaponServerRPC(int _index)
    {
        serverIndex.Value = _index;
    }

    /// <summary>
    ///  The weapon activation function
    /// </summary>
    private void ActivateWeapon(int _index)
    {
        DisableAllWeapons();
        weapons[_index].gameObject.SetActive(true);

        if (weapons[_index].gameObject.TryGetComponent(out WeaponManager _weaponManager)) _weaponManager.PickedUp();
    }

    /// <summary>
    /// Activate the picked up weapon
    /// </summary>
    public void ActivatePickedUpWeapon(int _index)
    {
        if (isObjectCarried) return;

        ActivateWeaponServerRPC(_index);
        if (weapons[_index].tag == "CarriedObject") isObjectCarried = true;
    }


    /// <summary>
    /// Scroll or using numerical keys to change weapon on the player character hand
    /// </summary>
    private void ScrollBetweenWeapons()
    {
        if (Input.GetKeyDown(KeyCode.Alpha1))
        {
            if (defaultWeapons[0] != 0) ActivateWeaponServerRPC(0);
        }

        if (Input.GetKeyDown(KeyCode.Alpha2))
        {
            if (defaultWeapons[1] != 0) ActivateWeaponServerRPC(1);
        }

        if (Input.GetKeyDown(KeyCode.Alpha3))
        {
            if (defaultWeapons[2] != 0) ActivateWeaponServerRPC(2);
        }

        if (Input.mouseScrollDelta.y < 0) currentWeaponIndex = SearchAvaliableWeaponDescend(currentWeaponIndex);
        else if (Input.mouseScrollDelta.y > 0) currentWeaponIndex = SearchAvaliableWeaponAscend(currentWeaponIndex);
        else return;

        ActivateWeaponServerRPC(defaultWeapons[currentWeaponIndex] - 1);
    }

    /// <summary>
    ///   When the player scrolls up it checks for the next weapon ont he weapon inventory and skips the empty index (0)
    /// </summary>
    private int SearchAvaliableWeaponAscend(int _currentIndex)
    {
        for (int i = _currentIndex + 1; i < defaultWeapons.Length - 1; i++) if (defaultWeapons[i] != 0) return i;

        return 0;
    }

    /// <summary>
    ///  When the player scrolls down it checks for the next weapon ont he weapon inventory and skips the empty index (0)
    /// </summary>
    private int SearchAvaliableWeaponDescend(int _currentIndex)
    {
        if (_currentIndex == 0) _currentIndex = defaultWeapons.Length - 1;
        for (int i = _currentIndex - 1; i >= 0; i--) if (defaultWeapons[i] != 0) return i;

        return 0;
    }
}
