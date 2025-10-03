using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class bsla : NetworkBehaviour
{
    [ServerCallback]
    void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.GetComponent<moviem>() != null)
        {
            collision.gameObject.GetComponent<moviem>().Vida -= moviem.instance.damage;
            NetworkServer.Destroy(this.gameObject);
            Destroy(this.gameObject, 5f);
        }
        else
        {
            NetworkServer.Destroy(this.gameObject);
            Destroy(this.gameObject, 5f);
        }
    }
}
