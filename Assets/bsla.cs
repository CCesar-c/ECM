using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class bsla : NetworkBehaviour
{
    public int damage = 20;
    public int municion = 20;

    void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.GetComponent<moviem>() != null)
        {
            collision.gameObject.GetComponent<moviem>().Vida -= damage;
        }
    }
}
