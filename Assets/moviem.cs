using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;


public class moviem : NetworkBehaviour
{
    [SyncVar] public int Vida = 200;
    public float speed = 5f;      // Velocidad del movimiento
    public float jumpForce = 5f;  // Fuerza del salto
    private Rigidbody rb;
    private bool isGrounded;
    public Camera playerCamera;
    private float xRotation = 0f;
    public Transform spawn;
    public GameObject bala;
    private readonly float mouseSensitivity = 100;

    public override void OnStartLocalPlayer()
    {
        // Activar solo la cámara del jugador local
        playerCamera.gameObject.SetActive(true);
    }

    void Start()
    {
        rb = GetComponent<Rigidbody>();
    }

    void Update()
    {
        // --- Movimiento en el plano ---
        if (isLocalPlayer)
        {
            if (Vida <= 0)
            {
                NetworkServer.Destroy(this.gameObject);
            }
            if (Input.GetKey(KeyCode.Mouse0))
            {
                CrearBala();
            }

            float moveX = Input.GetAxis("Horizontal");  // A/D o ←/→
            float moveZ = Input.GetAxis("Vertical");    // W/S o ↑/↓

            // Dirección en base a la rotación del jugador
            Vector3 move = transform.right * moveX + transform.forward * moveZ;

            // Mantener la velocidad Y (gravedad/salto)
            Vector3 newVelocity = new Vector3(move.x * speed, rb.velocity.y, move.z * speed);
            rb.velocity = newVelocity;
            // --- Salto ---
            if (Input.GetButtonDown("Jump") && isGrounded)
            {
                rb.AddForce(transform.up * jumpForce, ForceMode.Impulse);
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

    // Detectar si está en el suelo
    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Ground"))
        {
            isGrounded = true;
        }
    }

    private void OnCollisionExit(Collision collision)
    {
        if (collision.gameObject.CompareTag("Ground"))
        {
            isGrounded = false;
        }
    }
    [Command]
    public void CrearBala()
    {
        GameObject b = Instantiate(bala, spawn.transform.position, spawn.transform.rotation);
        NetworkServer.Spawn(b);
    }

}
