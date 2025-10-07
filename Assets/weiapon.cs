using System.Collections;
using System.Collections.Generic;
using Mirror;
using UnityEngine;

public class weiapon : NetworkBehaviour
{

    void Update()
    {
        if (isLocalPlayer)
        {
            transform.rotation = moviem.instance.playerCamera.transform.rotation;
        }
    }
}
