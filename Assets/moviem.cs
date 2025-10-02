using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
using UnityEngine.UI;
using UnityEngine.SocialPlatforms;

public class moviem : NetworkBehaviour
{
    public static moviem instance;
    public Text text_muni;
    public Slider bar;
    [SyncVar] public int Vida = 200;
    public float speed = 5f;
    public float jumpForce = 5f;
    private Rigidbody rb;
    public Camera playerCamera;
    private float xRotation = 0f;
    public float mouseSensitivity = 100;
    public int damage;
    public int municion;
    [SyncVar] public float delay = 2f;
    public Transform spawn;
    public GameObject bala;
    public GameObject[] Armas;
    [SyncVar] public bool puedeDisparar = true;

    public enum Typ
    {
        Automatico,
        Manual
    }

    public Typ typo;

    public override void OnStartLocalPlayer()
    {
        // Activar solo la cámara del jugador local
        playerCamera.gameObject.SetActive(true);
    }

    void Start()
    {
        Armastates();
        puedeDisparar = true;
        rb = GetComponent<Rigidbody>();
        instance = GetComponent<moviem>();
    }

    void Update()
    {
        if (isLocalPlayer)
        {
            text_muni.text = municion.ToString();
            // --- Barra de vida ---
            bar.value = Vida;
            if (Vida <= 0)
            {
                NetworkServer.Destroy(gameObject);
                return;
            }
            if (Input.GetKeyDown(KeyCode.R)&& municion > 0)
            {
                Cmdreload();
            }
            // --- Disparo ---
            if (typo == Typ.Automatico)
            {
                if (Input.GetKey(KeyCode.Mouse0) && puedeDisparar && municion > 0)
                {
                    puedeDisparar = false;
                    CmdCrearBala();
                }
            }
            else if (typo == Typ.Manual)
            {
                if (Input.GetKeyDown(KeyCode.Mouse0) && puedeDisparar && municion > 0)
                {
                    puedeDisparar = false;
                    CmdCrearBala();
                }
            }

            // --- Movimiento ---
            float moveX = Input.GetAxis("Horizontal");
            float moveZ = Input.GetAxis("Vertical");

            Vector3 move = transform.right * moveX + transform.forward * moveZ;
            Vector3 newVelocity = new Vector3(move.x * speed, rb.velocity.y, move.z * speed);
            rb.velocity = newVelocity;

            if (Input.GetKeyDown(KeyCode.Space) && IsGrounded())
            {
                Vector3 vel = rb.velocity;
                vel.y = 0; // resetea la velocidad vertical para saltar limpio
                rb.velocity = vel;

                rb.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
            }

            // --- Rotación ---
            float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity * Time.deltaTime;
            float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity * Time.deltaTime;

            xRotation -= mouseY;
            xRotation = Mathf.Clamp(xRotation, -90f, 90f);
            playerCamera.transform.localRotation = Quaternion.Euler(xRotation, 0f, 0f);

            transform.Rotate(Vector3.up * mouseX);
        }
    }

    [Command]
    void CmdCrearBala()
    {
        municion--;

        // Instanciar y sincronizar la bala inmediatamente
        GameObject b = Instantiate(bala, spawn.position, spawn.rotation);
        NetworkServer.Spawn(b, connectionToClient);
        b.GetComponent<Rigidbody>().AddForce(transform.forward * 1000);

        // Iniciar cooldown
        StartCoroutine(DisparoCooldown());
    }

    [Command]
    void Cmdreload()
    {
        StartCoroutine(nameof(recarga));
    }
    IEnumerator recarga() {
        puedeDisparar = false;
        text_muni.text = municion.ToString();
        yield return new WaitForSeconds(delay * 2);
        puedeDisparar = true;
        municion = 20;
    }

    IEnumerator DisparoCooldown()
    {
        // Espera antes de permitir otro disparo
        yield return new WaitForSeconds(delay);
        puedeDisparar = true;
    }

    bool IsGrounded()
    {
        // Raycast desde el centro hacia abajo
        return Physics.Raycast(transform.position, Vector3.down, 1.1f);
    }

    void Anim(Transform go)
    {
        Vector3 v = new Vector3(0, 0, go.transform.localPosition - 1);
        go.localPosition = Vector3.Lerp(go.transform.localPosition, v, delay * Time.deltaTime);
        
    }
    public void Armastates()
    {
        for (int i = 0; i < Armas.Length; i++)
        {
            if (Armas[i].activeInHierarchy)
            {
                switch (i)
                {
                    case 0:
                        typo = Typ.Manual;
                        damage = 10;
                        municion = 20;
                        delay = 0.5f;
                        break;
                    case 1:
                        typo = Typ.Manual;
                        damage = 100;
                        municion = 5;
                        delay = 1;
                        break;
                    case 2:
                        typo = Typ.Automatico;
                        damage = 30;
                        municion = 60;
                        delay = 0.25f;
                        break;
                    case 3:
                        typo = Typ.Manual;
                        damage = 200;
                        municion = 5;
                        delay = 1;
                        break;
                }
            }
        }
    }
}