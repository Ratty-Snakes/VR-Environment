using UnityEngine;
using System.Collections;
using System;

public class NPCManager : MonoBehaviour
{
    public static NPCManager Instance;

    [Header("Referencias de la Escena")]
    public GameObject npcPrefab;
    public TrapdoorController controladorTrampilla;
    public Transform[] listaWaypoints;
    public NPCUIController uiController;
    public LeverLockSystem sistemaPalanca;
    public HeavenDoorController puertasCielo;

    [Header("Configuracion de Tiempos")]
    public float tiempoEsperaCielo = 2.0f;

    [Header("Efectos Visuales (Feedback)")]
    public ParticleSystem fxConfetti;
    public ParticleSystem fxExplosionRoja;

    [Header("Efectos de Sonido")]
    public AudioSource audioSource;
    public AudioClip sonidoAceptar;
    public AudioClip sonidoRechazar;

    [Header("Estado Actual")]
    private GameObject npcActualObj;
    private NPCWaypointMovement movimientoActual;
    private NPCReactionController reaccionActual;
    private NPCData datosActuales;

    private bool mesaOcupada = false;
    private bool npcListoParaSentencia = false;

    // RESTRICCIONES DE TUTORIAL
    private bool tutorialBloquearAceptar = false;
    private bool tutorialBloquearRechazar = false;

    // Eventos
    public Action<bool> OnDecisionTutorial;
    public Action OnIntentoProhibido;

    [Header("Pool de Quejas (Físicas)")]
    [TextArea]
    public string[] listaQuejas = new string[] {
        "¡Oye! ¡Más respeto a los muertos!",
        "¡Ay! ¡Eso duele!",
        "¿Pero qué te pasa?",
        "¡Voy a llamar a mi abogado!",
        "¡Cuidado con la mercancía!",
        "¡Au! ¡Que soy de hueso frágil!"
    };

    public string GetQuejaRandom()
    {
        if (listaQuejas.Length == 0) return "¡Ouch!";
        return listaQuejas[UnityEngine.Random.Range(0, listaQuejas.Length)];
    }

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    void Start()
    {
        Debug.Log("Sistema NPC Listo.");
        if (sistemaPalanca != null) sistemaPalanca.BloquearPalanca();
    }

    // --- CONFIGURACIÓN PARA EL TUTORIAL ---
    public void SetRestriccionesTutorial(bool bloquearCielo, bool bloquearInfierno)
    {
        tutorialBloquearAceptar = bloquearCielo;
        tutorialBloquearRechazar = bloquearInfierno;
    }

    public void BotonSiguientePulsado() { if (!mesaOcupada) TraerSiguienteNPC(); }

    public void TraerSiguienteNPC()
    {
        if (GameManager.Instance != null) datosActuales = GameManager.Instance.ObtenerSiguienteNPC();
        if (datosActuales == null) return;
        GenerarNPC(datosActuales);
    }

    public void SpawnNPC_Tutorial(NPCData datosTutorial)
    {
        if (mesaOcupada) return;
        datosActuales = datosTutorial;
        GenerarNPC(datosActuales);
    }

    private void GenerarNPC(NPCData datos)
    {
        mesaOcupada = true;
        npcListoParaSentencia = false;
        if (sistemaPalanca != null) sistemaPalanca.BloquearPalanca();

        // 1. Crear el NPC base (contenedor)
        npcActualObj = Instantiate(npcPrefab, listaWaypoints[0].position, listaWaypoints[0].rotation);

        MeshRenderer baseMesh = npcActualObj.GetComponent<MeshRenderer>();
        if (baseMesh != null) baseMesh.enabled = false;

        // 2. Crear el modelo visual (hijo)
        if (datos.modeloEspecifico != null)
        {
            GameObject modeloVisual = Instantiate(datos.modeloEspecifico, npcActualObj.transform);
            modeloVisual.transform.localPosition = Vector3.zero;
            modeloVisual.transform.localRotation = Quaternion.identity;

            // Cambia 0.5f por el tamaño que quieras (1f es el original)
            modeloVisual.transform.localScale = Vector3.one;

            // --- ¡AQUÍ ESTÁ EL CAMBIO IMPORTANTE! --- 
            // Buscamos el script de impactos y le pasamos la cabeza nueva
            NPCImpactReactor reactor = npcActualObj.GetComponent<NPCImpactReactor>();
            if (reactor != null)
            {
                reactor.ConfigurarCabeza(modeloVisual.transform);
            }
            // ----------------------------------------
        }
        else if (baseMesh != null) baseMesh.enabled = true;

        movimientoActual = npcActualObj.GetComponent<NPCWaypointMovement>();
        if (movimientoActual != null)
        {
            movimientoActual.EmpezarCaminar(listaWaypoints, controladorTrampilla);
            movimientoActual.AlLlegarA_Trampilla += AlLlegarAMesa;
            movimientoActual.AlLlegarA_Cielo += LimpiarYTraerSiguiente;
        }

        reaccionActual = npcActualObj.GetComponent<NPCReactionController>();
        if (reaccionActual != null) reaccionActual.Initialize(datos);
    }

    void AlLlegarAMesa()
    {
        npcListoParaSentencia = true;
        if (uiController != null) uiController.MostrarDatos(datosActuales);
    }

    // ---------------------------------------------------------
    // TOMA DE DECISIONES
    // ---------------------------------------------------------

    public void RecibirGesto_Aceptar() // Pulgar Arriba
    {
        if (npcActualObj == null || !mesaOcupada || !npcListoParaSentencia) return;

        if (tutorialBloquearAceptar)
        {
            Debug.Log("Tutorial: No puedes aceptar a este NPC.");
            OnIntentoProhibido?.Invoke();
            return;
        }

        OnDecisionTutorial?.Invoke(true);
        StartCoroutine(SecuenciaAceptarCielo());
    }

    public void RecibirGesto_Rechazar() // Pulgar Abajo
    {
        if (npcActualObj == null || !mesaOcupada || !npcListoParaSentencia) return;

        if (tutorialBloquearRechazar)
        {
            Debug.Log("Tutorial: No puedes rechazar a este NPC.");
            OnIntentoProhibido?.Invoke();
            return;
        }

        if (sistemaPalanca != null && !sistemaPalanca.IsLocked) return;

        // Feedback Visual
        if (fxExplosionRoja != null) fxExplosionRoja.Play();

        // Feedback Sonoro
        if (audioSource != null && sonidoRechazar != null)
        {
            audioSource.PlayOneShot(sonidoRechazar);
        }

        if (sistemaPalanca != null) sistemaPalanca.DesbloquearPalanca();
        if (uiController != null) uiController.OcultarDatos();
        if (reaccionActual != null) reaccionActual.ShowNegativeReaction();
    }

    public void RecibirInput_Palanca()
    {
        if (npcActualObj == null || !mesaOcupada) return;
        if (sistemaPalanca != null && sistemaPalanca.IsLocked) return;

        Debug.Log("Palanca bajada correctamente. Al infierno.");

        OnDecisionTutorial?.Invoke(false);
        if (GameManager.Instance != null) GameManager.Instance.RegistrarRechazo();

        if (movimientoActual != null)
        {
            movimientoActual.CaerAlInfierno();
        }

        Invoke("LimpiarYTraerSiguiente", 4f);
    }

    IEnumerator SecuenciaAceptarCielo()
    {
        if (uiController != null) uiController.OcultarDatos();
        if (GameManager.Instance != null) GameManager.Instance.RegistrarEntradaCielo();
        if (reaccionActual != null) reaccionActual.ShowPositiveReaction();

        if (fxConfetti != null) fxConfetti.Play();

        if (audioSource != null && sonidoAceptar != null)
        {
            audioSource.PlayOneShot(sonidoAceptar);
        }

        if (puertasCielo != null)
        {
            puertasCielo.AbrirPuertas();
            puertasCielo.Invoke("CerrarPuertas", 10f);
        }

        yield return new WaitForSeconds(tiempoEsperaCielo);
        if (movimientoActual != null) movimientoActual.IrAlCielo();
    }

    void LimpiarYTraerSiguiente()
    {
        if (movimientoActual != null)
        {
            movimientoActual.AlLlegarA_Trampilla -= AlLlegarAMesa;
            movimientoActual.AlLlegarA_Cielo -= LimpiarYTraerSiguiente;
        }
        if (controladorTrampilla != null) controladorTrampilla.CloseTrapdoor();

        npcActualObj = null;
        mesaOcupada = false;
        npcListoParaSentencia = false;

        tutorialBloquearAceptar = false;
        tutorialBloquearRechazar = false;

        if (sistemaPalanca != null) sistemaPalanca.BloquearPalanca();
    }
}