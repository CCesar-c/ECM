using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class bsla : NetworkBehaviour
{

    void Update()
    {
        // Esto asegura que la bala se mueva en todos los clientes (visual)
        transform.position += transform.forward * 50f * Time.deltaTime;
    }

    //[ServerCallback]
    void OnTriggerEnter(Collider collision)
    {
        if (collision.gameObject.GetComponent<moviem>() != null)
        {
            Debug.Log("La bala colisiono com: " + collision.gameObject.name);
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
