using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
using UnityEngine.UI;
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
    [SyncVar] public int municion;
    [SyncVar] public float delay = 2f;
    public Transform spawn;
    public GameObject bala;
    public GameObject[] Armas;
    public FixedJoystick joystick;
    public bool puedeDisparar = true;

    public enum Typ
    {
        Automatico,
        Manual
    }

    public Typ typo;
    public int espelate;

    public override void OnStartLocalPlayer()
    {
        // Activar solo la cámara del jugador local
        playerCamera.gameObject.SetActive(true);
    }

    void Start()
    {
#if UNITY_ANDROID
        Debug.Log("esto es solo para android");
#endif
        Armastates();
        puedeDisparar = true;
        rb = GetComponent<Rigidbody>();
        instance = GetComponent<moviem>();
    }

#if UNITY_ANDROID
    private Vector2 lastTouchPos;
    private bool isTouching = false;

    public void Atirar()
    {
        puedeDisparar = true;
        if (typo == Typ.Automatico)
        {
            if (puedeDisparar && municion > 0)
            {
                StartCoroutine(DisparoCooldown());
                CmdCrearBala();
            }
        }
        else if (typo == Typ.Manual)
        {
            if (puedeDisparar && municion > 0)
            {
                StartCoroutine(DisparoCooldown());
                CmdCrearBala();
            }
        }
    }
    public void DesAtirar()
    {
        puedeDisparar = false;
    }
    public void Recaregar()
    {
        if (municion >= 0)
        {
            Cmdreload();
        }
    }
    public void Pular()
    {
        if (IsGrounded())
        {
            Vector3 vel = rb.velocity;
            vel.y = 0; // resetea la velocidad vertical para saltar limpio
            rb.velocity = vel;

            rb.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
        }
    }
#endif
    void Update()
    {
        if (isLocalPlayer)
        {

#if UNITY_ANDROID
            Vector3 sim = transform.forward * joystick.Horizontal + transform.right * joystick.Vertical;
            this.rb.AddForce(sim.normalized * speed);

            if (Application.isMobilePlatform) //
            {
                foreach (Transform t in playerCamera.GetComponentsInChildren<Transform>(true))
                {
                    if (t.name == "Shoot" || t.name == "Reload" || t.name == "Jump" || t.name == "Fixed Joystick")
                        t.gameObject.SetActive(true);
                }
                if (Input.touchCount > 0)
                {
                    Touch touch = Input.GetTouch(0);

                    if (touch.phase == TouchPhase.Began)
                    {
                        lastTouchPos = touch.position;
                        isTouching = true;
                    }
                    else if (touch.phase == TouchPhase.Moved && isTouching)
                    {
                        Vector2 delta = touch.position - lastTouchPos;

                        transform.Rotate(Vector3.up * delta.x);

                        // ROTACIÓN VERTICAL (X) limitada
                        xRotation -= delta.y;
                        xRotation = Mathf.Clamp(xRotation, -90f, 90f);
                        playerCamera.transform.localRotation = Quaternion.Euler(xRotation, 0f, 0f);
                    }
                    else if (touch.phase == TouchPhase.Ended || touch.phase == TouchPhase.Canceled)
                    {
                        isTouching = false;
                    }
                }

            }
#endif
            text_muni.text = municion.ToString();
            // --- Barra de vida ---
            bar.value = Vida;
            if (Vida <= 0)
            {
                NetworkServer.Destroy(gameObject);
                return;
            }
            if (Input.GetKeyDown(KeyCode.R) && municion >= 0)
            {
                Cmdreload();
            }
            if (Input.GetKeyDown(KeyCode.Alpha0))
            {

                for (int i = 0; i < Armas.Length; i++)
                {
                    Armas[i].SetActive(false);
                }
                Armas[0].SetActive(true);
                Armastates();
            }
            if (Input.GetKeyDown(KeyCode.Alpha1))
            {
                for (int i = 0; i < Armas.Length; i++)
                {
                    Armas[i].SetActive(false);
                }
                Armas[1].SetActive(true);
                Armastates();
            }
            if (Input.GetKeyDown(KeyCode.Alpha2))
            {
                for (int i = 0; i < Armas.Length; i++)
                {
                    Armas[i].SetActive(false);
                }
                Armas[2].SetActive(true);
                Armastates();
            }
            if (Input.GetKeyDown(KeyCode.Alpha3))
            {
                for (int i = 0; i < Armas.Length; i++)
                {
                    Armas[i].SetActive(false);
                }
                Armas[3].SetActive(true);
                Armastates();
            }
            // --- Disparo ---
            if (typo == Typ.Automatico)
            {
                if (Input.GetKey(KeyCode.Mouse0) && puedeDisparar && municion > 0)
                {
                    StartCoroutine(DisparoCooldown());
                    CmdCrearBala();
                }
            }
            else if (typo == Typ.Manual)
            {
                if (Input.GetKeyDown(KeyCode.Mouse0) && puedeDisparar && municion > 0)
                {
                    StartCoroutine(DisparoCooldown());
                    CmdCrearBala();
                }
            }

            // --- Movimiento ---
            float moveX = Input.GetAxis("Horizontal");
            float moveZ = Input.GetAxis("Vertical");

            Vector3 move = transform.right * moveX + transform.forward * moveZ;
            Vector3 newVelocity = new Vector3(move.x * speed, rb.velocity.y, move.z * speed);
            rb.velocity = newVelocity * Time.deltaTime;

            if (Input.GetKeyDown(KeyCode.Space) && IsGrounded())
            {
                Vector3 vel = rb.velocity;
                vel.y = 0; // resetea la velocidad vertical para saltar limpio
                rb.velocity = vel;

                rb.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
            }
            /*
                        // --- Rotación ---
                        float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity * Time.deltaTime;
                        float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity * Time.deltaTime;

                        xRotation -= mouseY;
                        xRotation = Mathf.Clamp(xRotation, -90f, 90f);
                        playerCamera.transform.localRotation = Quaternion.Euler(xRotation, 0f, 0f);

                        transform.Rotate(Vector3.up * mouseX);*/
        }
    }



    [Command]
    void CmdCrearBala()
    {
        municion--;

        GameObject b = Instantiate(bala, spawn.position, spawn.rotation);

        b.GetComponent<Rigidbody>().velocity = spawn.transform.forward * 50f;
        //b.GetComponent<Rigidbody>().AddForce(spawn.transform.forward * 10000f);

        NetworkServer.Spawn(b, connectionToClient);
    }
    IEnumerator DisparoCooldown()
    {
        // Espera antes de permitir otro disparo
        puedeDisparar = false;
        yield return new WaitForSeconds(delay);
        puedeDisparar = true;
    }
    [Command]
    void Cmdreload()
    {
        StartCoroutine(Recarga());
    }
    IEnumerator Recarga()
    {
        puedeDisparar = false;
        text_muni.text = municion.ToString();
        yield return new WaitForSeconds(delay * 2);
        puedeDisparar = true;
        municion = espelate;
    }


    bool IsGrounded()
    {
        // Raycast desde el centro hacia abajo
        return Physics.Raycast(transform.position, Vector3.down, 1.1f);
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
                        delay = 1f;
                        break;
                    case 1:
                        typo = Typ.Manual;
                        damage = 100;
                        municion = 5;
                        delay = 2;
                        break;
                    case 2:
                        typo = Typ.Automatico;
                        damage = 30;
                        municion = 60;
                        delay = 0.5f;
                        break;
                    case 3:
                        typo = Typ.Manual;
                        damage = 200;
                        municion = 5;
                        delay = 2;
                        break;
                }
                espelate = municion;
            }
        }
    }
}