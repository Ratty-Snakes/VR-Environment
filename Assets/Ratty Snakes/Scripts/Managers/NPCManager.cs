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

    [Header("Configuracion de Tiempos")]
    public float tiempoEsperaCielo = 2.0f;

    [Header("Estado Actual")]
    private GameObject npcActualObj;
    private NPCWaypointMovement movimientoActual;
    private NPCReactionController reaccionActual;
    private NPCData datosActuales;

    private bool mesaOcupada = false;
    private bool npcListoParaSentencia = false;

    // --- NUEVO: RESTRICCIONES DE TUTORIAL ---
    private bool tutorialBloquearAceptar = false; // Si es true, NO puedes mandarlo al cielo
    private bool tutorialBloquearRechazar = false; // Si es true, NO puedes usar la palanca/gesto mal

    // Eventos
    public Action<bool> OnDecisionTutorial;
    public Action OnIntentoProhibido; // Se dispara si intentas hacer lo que no debes

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

    // ... (El resto de GenerarNPC y TraerSiguienteNPC es igual que antes) ...
    // ... CÓPIALO DEL SCRIPT ANTERIOR O DÉJALO COMO ESTABA ...

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

        npcActualObj = Instantiate(npcPrefab, listaWaypoints[0].position, listaWaypoints[0].rotation);

        MeshRenderer baseMesh = npcActualObj.GetComponent<MeshRenderer>();
        if (baseMesh != null) baseMesh.enabled = false;

        if (datos.modeloEspecifico != null)
        {
            GameObject modeloVisual = Instantiate(datos.modeloEspecifico, npcActualObj.transform);
            modeloVisual.transform.localPosition = Vector3.zero;
            modeloVisual.transform.localRotation = Quaternion.identity;
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
    // TOMA DE DECISIONES CON RESTRICCIONES
    // ---------------------------------------------------------

    public void RecibirGesto_Aceptar() // Pulgar Arriba
    {
        if (npcActualObj == null || !mesaOcupada || !npcListoParaSentencia) return;

        // NUEVO: Bloqueo de Tutorial
        if (tutorialBloquearAceptar)
        {
            Debug.Log("Tutorial: No puedes aceptar a este NPC.");
            OnIntentoProhibido?.Invoke(); // Avisamos al TutorialManager para la bronca
            return;
        }

        OnDecisionTutorial?.Invoke(true);
        StartCoroutine(SecuenciaAceptarCielo());
    }

    public void RecibirGesto_Rechazar() // Pulgar Abajo
    {
        if (npcActualObj == null || !mesaOcupada || !npcListoParaSentencia) return;

        // NUEVO: Bloqueo de Tutorial
        if (tutorialBloquearRechazar)
        {
            Debug.Log("Tutorial: No puedes rechazar a este NPC.");
            OnIntentoProhibido?.Invoke(); // Avisamos al TutorialManager para la bronca
            return;
        }

        // Anti-repetición
        if (sistemaPalanca != null && !sistemaPalanca.IsLocked) return;

        if (sistemaPalanca != null) sistemaPalanca.DesbloquearPalanca();
        if (uiController != null) uiController.OcultarDatos();
        if (reaccionActual != null) reaccionActual.ShowNegativeReaction();
    }

    // 3. PALANCA: Ejecucion (Infierno - Paso 2)
    public void RecibirInput_Palanca()
    {
        // Seguridad básica
        if (npcActualObj == null || !mesaOcupada) return;

        // SEGURIDAD 1: ¿La palanca dice que está bloqueada?
        if (sistemaPalanca != null && sistemaPalanca.IsLocked)
        {
            Debug.Log("Intento ignorado: La palanca está bloqueada físicamente.");
            return;
        }

        // SEGURIDAD 2: ¿El tutorial prohíbe rechazar ahora mismo? (Caso Benito)
        if (tutorialBloquearRechazar)
        {
            Debug.Log("Tutorial: INTENTO BLOQUEADO. No puedes rechazar a este NPC.");
            // Opcional: Si quieres que Dios le eche la bronca también si intenta tirar de la palanca a la fuerza
            OnIntentoProhibido?.Invoke();

            // IMPORTANTE: Si la palanca se bajó visualmente, hay que "resetearla" o bloquearla de nuevo
            if (sistemaPalanca != null) sistemaPalanca.BloquearPalanca();
            return;
        }

        // Si pasa todos los filtros, ejecutamos
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

        // Reset de restricciones (por seguridad)
        tutorialBloquearAceptar = false;
        tutorialBloquearRechazar = false;

        if (sistemaPalanca != null) sistemaPalanca.BloquearPalanca();
    }
}