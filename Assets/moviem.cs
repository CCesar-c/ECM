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
        Armastates(0);
        puedeDisparar = true;
        rb = GetComponent<Rigidbody>();
        instance = GetComponent<moviem>();
    }

#if UNITY_ANDROID
    int i;
    public void ChangeArma()
    {
        i++;
        if (i > 3)
            i = 0;
        Armastates(i);
    }
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
    public Vector3 retroceso = new Vector3(0, 0, -0.3f); // cuánto se mueve hacia atrás
    public float velocidad = 10f;   // velocidad del movimiento

    private Vector3 posInicial;
    private float t;                // interpolador
    private bool haciaAtras;
    void Update()
    {
        if (isLocalPlayer)
        {
            //celular
            if (Application.platform == RuntimePlatform.Android)
            {
                Vector3 sim = transform.forward * joystick.Vertical + transform.right * joystick.Horizontal;
                rb.AddForce(sim.normalized * speed);
                Transform[] arrey = GetComponentsInChildren<Transform>();
                for (int i = 0; i < arrey.Length; i++)
                {
                    if (arrey[i].name == "Controls_Mobile")
                    {
                        arrey[i].gameObject.SetActive(true);
                    }
                }
            }
            text_muni.text = municion.ToString();
            // --- Barra de vida ---
            bar.value = Vida;
            if (Vida <= 0)
            {
                text_muni.text = "Game Over";
                //NetworkServer.Destroy(gameObject);
                return;
            }
            for (int i = 0; i < Armas.Length; i++)
            {
                if (Armas[i].activeInHierarchy == false) return;
                if (haciaAtras)
                {
                    t += Time.deltaTime * velocidad;
                    Armas[i].transform.localPosition = Vector3.Lerp(posInicial, posInicial + retroceso, t);

                    if (t >= 1f)
                    {
                        haciaAtras = false;
                        t = 0f; // reinicia para volver
                    }
                }
                else if (Armas[i].transform.localPosition != posInicial) // volver a la posición inicial
                {
                    t += Time.deltaTime * velocidad;
                    Armas[i].transform.localPosition = Vector3.Lerp(posInicial + retroceso, posInicial, t);
                }
            }
            // PC
            if (Application.platform != RuntimePlatform.Android)
            {
                if (Input.GetKeyDown(KeyCode.R) && municion >= 0)
                {
                    Cmdreload();
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

                if (Input.GetKeyDown(KeyCode.Q))
                {
                    i++;
                    if (i > 3)
                        i = 0;
                    Armastates(i);
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

                    rb.AddForce(Vector3.up * jumpForce);
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
    }
    bool IsGrounded()
    {
        // Raycast desde el centro hacia abajo
        return Physics.Raycast(transform.position, Vector3.down, 1.1f);
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

    void Disparar()
    {
        haciaAtras = true;
        t = 0f;
        Debug.Log("¡Bang!");
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

    public void Armastates(int i)
    {

        switch (i)
        {
            case 0:
                typo = Typ.Manual;
                damage = 10;
                municion = 20;
                delay = 1f;
                for (int a = 0; a < 4; a++)
                {
                    Armas[a].SetActive(false);
                }
                Armas[0].SetActive(true);
                break;
            case 1:
                typo = Typ.Manual;
                damage = 100;
                municion = 5;
                delay = 2;
                for (int a = 0; a < 4; a++)
                {
                    Armas[a].SetActive(false);
                }
                Armas[1].SetActive(true);
                break;
            case 2:
                typo = Typ.Automatico;
                damage = 30;
                municion = 60;
                delay = 0.5f;
                for (int a = 0; a < 4; a++)
                {
                    Armas[a].SetActive(false);
                }
                Armas[2].SetActive(true);
                break;
            case 3:
                typo = Typ.Manual;
                damage = 200;
                municion = 5;
                delay = 2;
                for (int a = 0; a < 4; a++)
                {
                    Armas[a].SetActive(false);
                }
                Armas[3].SetActive(true);
                break;
        }
        espelate = municion;
    }
}