using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode.Components;

public class BetterNetworkAnimator : NetworkAnimator
{

    protected override bool OnIsServerAuthoritative()
    {
        return false;
    }

}
