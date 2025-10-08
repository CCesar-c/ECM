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
    public bool puedeMirar = false;
    int i;
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
                        if (puedeDisparar)
                        {
                            Armas[i].transform.localPosition = Vector3.Lerp(Armas[i].transform.localPosition, Armas[i].transform.localPosition + retroceso, 10f * Time.deltaTime);
                            Armas[i].transform.localPosition = Vector3.Lerp(Armas[i].transform.localPosition + retroceso, Armas[i].transform.localPosition, 10f * Time.deltaTime);
                        }
                StartCoroutine(DisparoCooldown());
                CmdCrearBala();
            }
        }
        else if (typo == Typ.Manual)
        {
            if (puedeDisparar && municion > 0)
            {
                                        if (puedeDisparar)
                        {
                            Armas[i].transform.localPosition = Vector3.Lerp(Armas[i].transform.localPosition, Armas[i].transform.localPosition + retroceso, 10f * Time.deltaTime);
                            Armas[i].transform.localPosition = Vector3.Lerp(Armas[i].transform.localPosition + retroceso, Armas[i].transform.localPosition, 10f * Time.deltaTime);
                        }
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

    Vector3 posInicial = new Vector3(0.6f, -0.3f, 1.1f);
    Vector3 retroceso = new Vector3(0, 0, -1f);
    Vector3 a = new Vector3(0f, -0.2f, 1f);
    float forca;
    float targetRecoil;
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
            if (puedeMirar)
            {
                // Modo apuntando
                playerCamera.fieldOfView = Mathf.Lerp(playerCamera.fieldOfView, 30, 5f * Time.deltaTime);
                Armas[i].transform.localPosition = Vector3.Lerp(Armas[i].transform.localPosition, a, 10f * Time.deltaTime);
            }
            else
            {
                // Modo normal
                playerCamera.fieldOfView = Mathf.Lerp(playerCamera.fieldOfView, 60, 5f * Time.deltaTime);
                Armas[i].transform.localPosition = Vector3.Lerp(Armas[i].transform.localPosition, posInicial, 10f * Time.deltaTime);
            }

            // PC
            if (Application.platform != RuntimePlatform.Android)
            {
                if (Input.GetKeyDown(KeyCode.R) && municion >= 0)
                {
                    Cmdreload();
                }

                if (Input.GetKey(KeyCode.Mouse1))
                {
                    puedeMirar = true;
                }
                if (Input.GetKeyUp(KeyCode.Mouse1))
                {
                    puedeMirar = false;
                }
                if (!puedeMirar)
                {
                    targetRecoil = 0f;
                }
                // --- Disparo ---
                if (typo == Typ.Automatico)
                {
                    if (Input.GetKey(KeyCode.Mouse0) && puedeDisparar && municion > 0)
                    {
                        if (puedeDisparar)
                        {
                            targetRecoil += forca;
                            targetRecoil = Mathf.Clamp(targetRecoil, 0f, forca + 10);

                            // vuelve lentamente a 0 cuando no dispara
                            Armas[i].transform.localPosition = Vector3.Lerp(Armas[i].transform.localPosition, Armas[i].transform.localPosition + retroceso, 10f * Time.deltaTime);
                            Armas[i].transform.localPosition = Vector3.Lerp(Armas[i].transform.localPosition + retroceso, Armas[i].transform.localPosition, 10f * Time.deltaTime);
                        }

                        StartCoroutine(DisparoCooldown());
                        CmdCrearBala();
                    }
                }
                else if (typo == Typ.Manual)
                {
                    if (Input.GetKeyDown(KeyCode.Mouse0) && puedeDisparar && municion > 0)
                    {
                        if (puedeDisparar)
                        {
                            targetRecoil += forca;
                            targetRecoil = Mathf.Clamp(targetRecoil, 0f, forca + 10);

                            // vuelve lentamente a 0 cuando no dispara
                            Armas[i].transform.localPosition = Vector3.Lerp(Armas[i].transform.localPosition, Armas[i].transform.localPosition + retroceso, 10f * Time.deltaTime);
                            Armas[i].transform.localPosition = Vector3.Lerp(Armas[i].transform.localPosition + retroceso, Armas[i].transform.localPosition, 10f * Time.deltaTime);
                        }

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
                rb.velocity = newVelocity;

                if (Input.GetKeyDown(KeyCode.Space) && IsGrounded())
                {
                    rb.AddForce(Vector3.up * jumpForce);
                }

                // --- Rotación ---
                float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity * Time.deltaTime;
                float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity * Time.deltaTime;

                xRotation -= mouseY;
                xRotation = Mathf.Clamp(xRotation, -90f, 90f);
                playerCamera.transform.localRotation = Quaternion.Euler(xRotation - targetRecoil, 0f, 0f);

                transform.Rotate(Vector3.up * mouseX);
            }
        }
    }
    bool IsGrounded()
    {
        return Physics.Raycast(transform.position, Vector3.down, 1.1f);
    }

    [Command]
    void CmdCrearBala()
    {
        municion--;
        GameObject b = Instantiate(bala, spawn.position, spawn.rotation);
        b.GetComponent<Rigidbody>().velocity = spawn.transform.forward * 50f;
        NetworkServer.Spawn(b, connectionToClient);
    }

    IEnumerator DisparoCooldown()
    {
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
        yield return new WaitForSeconds(delay * 2);
        puedeDisparar = true;

        switch (i)
        {
            case 0:
                municion = 20;
                break;
            case 1:
                municion = 5;
                break;
            case 2:
                municion = 60;
                break;
            case 3:
                municion = 5;
                break;
        }
        targetRecoil = 0;
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
                forca = 2;
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
                forca = 4;
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
                delay = 0.125f;
                forca = 0.5f;
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
                forca = 10;
                for (int a = 0; a < 4; a++)
                {
                    Armas[a].SetActive(false);
                }
                Armas[3].SetActive(true);
                break;
        }
    }
}