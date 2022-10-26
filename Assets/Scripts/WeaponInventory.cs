using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

public class WeaponInventory : NetworkBehaviour
{

    [SerializeField] Transform[] weapons;
    [SerializeField] int[] defaultWeapons = new int[3];

    private Animator anim;
    int currentWeaponIndex;
    NetworkVariable<int> serverIndex = new NetworkVariable<int>(-1);

    PlayerMovement player;
    public Animator GetAnimator() { return anim; }

    void Awake()
    {
        player = transform.root.GetComponent<PlayerMovement>();
        anim = GetComponent<Animator>();
    }

    public override void OnNetworkSpawn()
    {
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

        ScrollBetweenWeapons();
    }

    public void AddWeapon(int _index)
    {
        if (!player.IsOwner) return;

        for (int i = 0; i < defaultWeapons.Length; i++)
        {
            if (defaultWeapons[i] == _index) return;
        }

        for (int i = 0; i < defaultWeapons.Length; i++)
        {
            if (defaultWeapons[i] == 0)
            {
                defaultWeapons[i] = _index;
                return;
            }
        }
    }


    public void SetWeaponAnimatorController(RuntimeAnimatorController _animController)
    {
        anim.runtimeAnimatorController = _animController;
    }

    private void DisableAllWeapons()
    {
        for (int i = 0; i < weapons.Length; i++)
        {
            weapons[i].gameObject.SetActive(false);
        }
    }

    [ClientRpc]
    private void ActivateWeaponClientRPC(int _PrevIndex, int _NewIndex)
    {
        Debug.Log("Weapon scroll: " + _PrevIndex + " " + _NewIndex);
        ActivateWeapon(_NewIndex);
    }

    [ServerRpc]
    public void ActivateWeaponServerRPC(int _index)
    {
        serverIndex.Value = _index;
    }
    private void ActivateWeapon(int _index)
    {
        DisableAllWeapons();
        weapons[_index].gameObject.SetActive(true);
        weapons[_index].gameObject.GetComponent<WeaponManager>().PickedUp();
    }

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

        if (Input.mouseScrollDelta.y < 0) currentWeaponIndex--;
        else if (Input.mouseScrollDelta.y > 0) currentWeaponIndex++;
        else return;

        if (currentWeaponIndex < 0) currentWeaponIndex = defaultWeapons.Length - 1;
        else if (currentWeaponIndex > defaultWeapons.Length - 1) currentWeaponIndex = 0;

        while (defaultWeapons[currentWeaponIndex] == 0)
        {
            if (Input.mouseScrollDelta.y < 0) currentWeaponIndex--;
            else if (Input.mouseScrollDelta.y > 0) currentWeaponIndex++;

            if (currentWeaponIndex < 0) currentWeaponIndex = defaultWeapons.Length - 1;
            else if (currentWeaponIndex > defaultWeapons.Length - 1) currentWeaponIndex = 0;
        }
        ActivateWeaponServerRPC(defaultWeapons[currentWeaponIndex] - 1);
    }
}
