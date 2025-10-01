using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
using UnityEngine.UI;


public class moviem : NetworkBehaviour
{
    public static moviem instance;
    public Slider bar;
    // public bsla bala;
    [SyncVar] public int Vida = 200;
    public float speed = 5f;      // Velocidad del movimiento
    public float jumpForce = 5f;  // Fuerza del salto
    private Rigidbody rb;
    public bool isGrounded;
    public Camera playerCamera;
    private float xRotation = 0f;
    public float mouseSensitivity = 100;
    public int damage = 20;
    [SyncVar] public int municion = 20;
    [SyncVar] public float delay = 2f;
    public Transform spawn;
    public GameObject bala;

    public bool puedeDisparar = false;

    public override void OnStartLocalPlayer()
    {
        // Activar solo la cámara del jugador local
        playerCamera.gameObject.SetActive(true);
    }

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        instance = GetComponent<moviem>();
    }

    void Update()
    {
        // --- Movimiento en el plano ---
        if (isLocalPlayer)
        {
            bar.value = Vida;
            if (Vida <= 0)
            {
                //Debug.Log("morri..!!");
                NetworkServer.Destroy(gameObject);
            }

            if (Input.GetKeyDown(KeyCode.Mouse0))
            {
                puedeDisparar = true;
                if (puedeDisparar && municion > 0)
                {
                    CmdCrearBala();
                }
            }
            float moveX = Input.GetAxis("Horizontal");  // A/D o ←/→
            float moveZ = Input.GetAxis("Vertical");    // W/S o ↑/↓

            // Dirección en base a la rotación del jugador
            Vector3 move = transform.right * moveX + transform.forward * moveZ;

            // Mantener la velocidad Y (gravedad/salto)
            Vector3 newVelocity = new Vector3(move.x * speed, rb.velocity.y, move.z * speed);
            rb.velocity = newVelocity;
            // --- Salto ---
            if (Input.GetKeyDown(KeyCode.Space) && isGrounded)
            {
                rb.AddForce(transform.up * jumpForce, ForceMode.Impulse);
                isGrounded = false;
            }

            float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity * Time.deltaTime;
            float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity * Time.deltaTime;

            // Rotación vertical de la cámara
            xRotation -= mouseY;
            xRotation = Mathf.Clamp(xRotation, -90f, 90f);
            playerCamera.transform.localRotation = Quaternion.Euler(xRotation, 0f, 0f);

            // Rotación horizontal del cuerpo del jugador
            transform.Rotate(Vector3.up * mouseX);
        }
    }

    [Command]
    void CmdCrearBala()
    {
        StartCoroutine(DisparoCooldown());
    }

    IEnumerator DisparoCooldown()
    {
        //municion--;
        GameObject b = Instantiate(bala, spawn.position, spawn.rotation);
        NetworkServer.Spawn(b, connectionToClient);
        b.GetComponent<Rigidbody>().AddForce(transform.forward * 1000);

        puedeDisparar = false;
        yield return new WaitForSeconds(delay);
        puedeDisparar = true;
    }

    // Detectar si está en el suelo
    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Ground"))
        {
            isGrounded = true;
        }
    }
    void OnCollisionExit(Collision collision)
    {
        if (collision.gameObject.CompareTag("Ground"))
        {
            isGrounded = false;
        }
    }
}
