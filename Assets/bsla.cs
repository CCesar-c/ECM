using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class bsla : NetworkBehaviour
{
    void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.GetComponent<moviem>() != null)
        {
            collision.gameObject.GetComponent<moviem>().Vida -= moviem.instance.damage;
            NetworkServer.Destroy(this.gameObject);
        }
        else
        {
            NetworkServer.Destroy(this.gameObject);
        }
    }
}
