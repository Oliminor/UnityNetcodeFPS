using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

public class WeaponInventory : NetworkBehaviour
{
    private Animator anim;


    public Animator GetAnimator() { return anim; }

    void Awake()
    {
        anim = GetComponent<Animator>();
    }

    public void SetWeaponAnimatorController(RuntimeAnimatorController _animController)
    {
        anim.runtimeAnimatorController = _animController;
    }
}
