using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class weiapon : MonoBehaviour
{

    void Update()
    {
        transform.rotation = moviem.instance.playerCamera.transform.rotation;
    }
}
