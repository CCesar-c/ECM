using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
using UnityEngine.UI;

public class moviem : NetworkBehaviour
{
    public static moviem instance;

    [Space(10)]
    [Header("Menus")]
    public Slider s_sensi;
    public Dropdown dropFps;
    public GameObject menu;

    [Space(10)]
    [Header("UI")]
    public Text text_muni;
    public Slider bar;

    [Space(10)]
    [Header("Estadísticas del Jugador")]
    [SyncVar] public int Vida = 200;
    [SyncVar] public int municion;
    [SyncVar] public float delay = 2f;
    [SyncVar] public int damage;

    [Space(10)]
    [Header("Movimiento")]
    public float speed = 5f;
    public float jumpForce = 5f;
    private Rigidbody rb;

    [Space(10)]
    [Header("Cámara")]
    public Camera playerCamera;
    public float mouseSensitivity;
    private float xRotation;
    public bool puedeMirar = false;

    [Space(10)]
    [Header("Armas")]
    public GameObject[] Armas;
    public Transform spawn;
    public GameObject bala;
    public bool puedeDisparar = true;

    public enum Typ
    {
        Automatico,
        Manual
    }

    [SyncVar] public Typ typo;
    [SyncVar] float forca;

    Vector3 posInicial = new Vector3(0.6f, -0.3f, 1.1f);
    Vector3 retroceso = new Vector3(0, 0, -1f);
    Vector3 a = new Vector3(0f, -0.2f, 1f);
    float targetRecoil;
    bool menuAtivo;

    // --- Inicialización ---
    public override void OnStartLocalPlayer()
    {
        playerCamera.gameObject.SetActive(true);
    }

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        instance = this;
        puedeDisparar = true;
    }

    void Update()
    {
        if (!isLocalPlayer) return;

        text_muni.text = municion.ToString();
        bar.value = Vida;

        if (Vida <= 0)
        {
            text_muni.text = "Game Over";
            return;
        }

        // --- Zoom y posición del arma ---
        if (puedeMirar)
        {
            playerCamera.fieldOfView = Mathf.Lerp(playerCamera.fieldOfView, 30, 5f * Time.deltaTime);
            for (int i = 0; i < Armas.Length; i++)
            {
                Armas[i].transform.localPosition = Vector3.Lerp(Armas[i].transform.localPosition, a, 10f * Time.deltaTime);
            }
        }
        else
        {
            playerCamera.fieldOfView = Mathf.Lerp(playerCamera.fieldOfView, 60, 5f * Time.deltaTime);
            for (int i = 0; i < Armas.Length; i++)
            {
                Armas[i].transform.localPosition = Vector3.Lerp(Armas[i].transform.localPosition, posInicial, 10f * Time.deltaTime);
            }
        }

        // --- Menú ---
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            menuAtivo = !menuAtivo;
        }

        menu.transform.localScale = Vector3.Lerp(
            menu.transform.localScale,
            menuAtivo ? Vector3.one : Vector3.zero,
            10f * Time.deltaTime
        );

        // --- Recarga ---
        if (Input.GetKeyDown(KeyCode.R) && municion >= 0)
        {
            Cmdreload();
        }

        // --- Mirar ---
        if (Input.GetKey(KeyCode.Mouse1)) puedeMirar = true;
        if (Input.GetKeyUp(KeyCode.Mouse1)) puedeMirar = false;
        if (!puedeMirar) targetRecoil = 0f;

        // --- Disparo ---
        if (typo == Typ.Automatico)
        {
            if (Input.GetKey(KeyCode.Mouse0) && puedeDisparar && municion > 0)
            {
                Disparar();
            }
        }
        else if (typo == Typ.Manual)
        {
            if (Input.GetKeyDown(KeyCode.Mouse0) && puedeDisparar && municion > 0)
            {
                Disparar();
            }
        }

        // --- Cambiar arma ---
        if (Input.GetKey(KeyCode.Alpha1))
        {
            typo = Typ.Manual;
            damage = 10;
            municion = 20;
            delay = 1f;
            forca = 2;
            for (int a = 0; a < Armas.Length; a++)
            {
                Armas[a].SetActive(false);
            }
            Armas[0].SetActive(true);
        }
        if (Input.GetKey(KeyCode.Alpha2))
        {
            typo = Typ.Manual;
            damage = 100;
            municion = 5;
            delay = 2;
            forca = 4;
            for (int a = 0; a < Armas.Length; a++)
            {
                Armas[a].SetActive(false);
            }
            Armas[1].SetActive(true);
        }
        if (Input.GetKey(KeyCode.Alpha3))
        {
            typo = Typ.Automatico;
            damage = 30;
            municion = 60;
            delay = 0.125f;
            forca = 0.5f;
            for (int a = 0; a < Armas.Length; a++)
            {
                Armas[a].SetActive(false);
            }
            Armas[2].SetActive(true);
        }
        if (Input.GetKey(KeyCode.Alpha4))
        {
            typo = Typ.Manual;
            damage = 200;
            municion = 5;
            delay = 2;
            forca = 10;
            for (int a = 0; a < Armas.Length; a++)
            {
                Armas[a].SetActive(false);
            }
            Armas[3].SetActive(true);
        }


        // --- Movimiento ---
        float moveX = Input.GetAxis("Horizontal");
        float moveZ = Input.GetAxis("Vertical");

        Vector3 move = transform.right * moveX + transform.forward * moveZ;
        Vector3 newVelocity = new Vector3(move.x * speed, rb.velocity.y, move.z * speed);
        rb.velocity = newVelocity;

        if (Input.GetKeyDown(KeyCode.Space) && IsGrounded())
        {
            rb.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
        }

        // --- Rotación ---
        float mouseX = Input.GetAxis("Mouse X") * s_sensi.value * Time.deltaTime;
        float mouseY = Input.GetAxis("Mouse Y") * s_sensi.value * Time.deltaTime;

        xRotation -= mouseY;
        xRotation = Mathf.Clamp(xRotation, -90f, 90f);
        playerCamera.transform.localRotation = Quaternion.Euler(xRotation - targetRecoil, 0f, 0f);

        transform.Rotate(Vector3.up * mouseX);
    }

    bool IsGrounded()
    {
        return Physics.Raycast(transform.position, -transform.up, 1.1f);
    }

    void Disparar()
    {
        targetRecoil += forca;
        for (int i = 0; i < Armas.Length; i++)
        {

            Armas[i].transform.localPosition = Vector3.Lerp(Armas[i].transform.localPosition, Armas[i].transform.localPosition + retroceso, 10f * Time.deltaTime);
            Armas[i].transform.localPosition = Vector3.Lerp(Armas[i].transform.localPosition + retroceso, Armas[i].transform.localPosition, 10f * Time.deltaTime);

        }
        StartCoroutine(DisparoCooldown());
        CmdCrearBala();
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

        for (int i = 0; i < Armas.Length; i++)
        {
            switch (i)
            {
                case 0: municion = 20; break;
                case 1: municion = 5; break;
                case 2: municion = 60; break;
                case 3: municion = 5; break;
            }
        }
        targetRecoil = 0;
    }

    public void CambioFps()
    {
        switch (dropFps.value)
        {
            case 0: Application.targetFrameRate = 30; break;
            case 1: Application.targetFrameRate = 45; break;
            case 2: Application.targetFrameRate = 60; break;
            case 3: Application.targetFrameRate = 90; break;
            case 4: Application.targetFrameRate = 120; break;
        }
    }
}
