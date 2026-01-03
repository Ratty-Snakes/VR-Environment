using UnityEngine;
using System.Collections;
using System; // Necesario para los Actions

public class NPCManager : MonoBehaviour
{
    public static NPCManager Instance;

    [Header("Referencias de la Escena")]
    public GameObject npcPrefab;
    public TrapdoorController controladorTrampilla;
    public Transform[] listaWaypoints;
    public NPCUIController uiController;

    [Header("Configuracion de Tiempos")]
    [Tooltip("Segundos que espera el NPC agradeciendo antes de irse al cielo")]
    public float tiempoEsperaCielo = 2.0f;

    [Header("Estado Actual")]
    private GameObject npcActualObj;
    private NPCWaypointMovement movimientoActual;
    private NPCReactionController reaccionActual;
    private NPCData datosActuales;

    // Variable para saber si la mesa esta ocupada
    private bool mesaOcupada = false;

    // EVENTO NUEVO: Avisa al Tutorial de la decision (true=Cielo, false=Infierno)
    public Action<bool> OnDecisionTutorial;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    void Start()
    {
        // En el juego normal, esperamos. En el tutorial, el TutorialManager manda.
        Debug.Log("Sistema NPC Listo.");
    }

    // ---------------------------------------------------------
    // LOGICA JUEGO NORMAL (Llamada por el Boton o GameManager)
    // ---------------------------------------------------------

    public void BotonSiguientePulsado()
    {
        if (mesaOcupada)
        {
            Debug.Log("Todavia hay un alma en proceso.");
            return;
        }

        TraerSiguienteNPC();
    }

    public void TraerSiguienteNPC()
    {
        // Pedimos datos al GameManager (Juego Normal)
        if (GameManager.Instance != null)
        {
            datosActuales = GameManager.Instance.ObtenerSiguienteNPC();
        }

        if (datosActuales == null)
        {
            Debug.Log("No quedan mas NPCs en la cola (o GameManager no existe).");
            return;
        }

        GenerarNPC(datosActuales);
    }

    // ---------------------------------------------------------
    // LOGICA TUTORIAL (Llamada por TutorialManager)
    // ---------------------------------------------------------

    public void SpawnNPC_Tutorial(NPCData datosTutorial)
    {
        if (mesaOcupada) return;

        Debug.Log("TUTORIAL: Spawneando NPC especifico.");

        // Forzamos los datos que nos pasa el tutorial
        datosActuales = datosTutorial;

        GenerarNPC(datosActuales);
    }

    // ---------------------------------------------------------
    // LOGICA COMUN (Generacion visual y fisica)
    // ---------------------------------------------------------

    private void GenerarNPC(NPCData datos)
    {
        mesaOcupada = true;

        // 1. Instanciar el CONTENEDOR (NPC_Base)
        npcActualObj = Instantiate(npcPrefab, listaWaypoints[0].position, listaWaypoints[0].rotation);

        // 2. Logica de cambio de modelo (Visual)
        MeshRenderer baseMesh = npcActualObj.GetComponent<MeshRenderer>();
        if (baseMesh != null) baseMesh.enabled = false;

        if (datos.modeloEspecifico != null)
        {
            GameObject modeloVisual = Instantiate(datos.modeloEspecifico, npcActualObj.transform);
            modeloVisual.transform.localPosition = Vector3.zero;
            modeloVisual.transform.localRotation = Quaternion.identity;
        }
        else
        {
            // Si no hay modelo especifico, mostramos la capsula base
            if (baseMesh != null) baseMesh.enabled = true;
        }

        // 3. Configurar Movimiento
        movimientoActual = npcActualObj.GetComponent<NPCWaypointMovement>();
        if (movimientoActual != null)
        {
            movimientoActual.EmpezarCaminar(listaWaypoints, controladorTrampilla);
            movimientoActual.AlLlegarA_Trampilla += AlLlegarAMesa;
            movimientoActual.AlLlegarA_Cielo += LimpiarYTraerSiguiente;
        }

        // 4. Configurar Reaccion (Dialogos)
        reaccionActual = npcActualObj.GetComponent<NPCReactionController>();
        if (reaccionActual != null)
        {
            reaccionActual.Initialize(datos);
        }
    }

    void AlLlegarAMesa()
    {
        if (uiController != null) uiController.MostrarDatos(datosActuales);
    }

    // ---------------------------------------------------------
    // TOMA DE DECISIONES
    // ---------------------------------------------------------

    // GESTO: Pulgar Arriba (Cielo)
    public void Decidir_Aceptar()
    {
        if (npcActualObj == null) return;

        // AVISO TUTORIAL: Ha decidido CIELO (true)
        OnDecisionTutorial?.Invoke(true);

        // Iniciamos la secuencia con espera
        StartCoroutine(SecuenciaAceptarCielo());
    }

    IEnumerator SecuenciaAceptarCielo()
    {
        Debug.Log("Veredicto: ACEPTADO. Esperando...");

        // 1. Ocultamos los datos
        if (uiController != null) uiController.OcultarDatos();

        // 2. Registramos el punto (Solo si existe GameManager)
        if (GameManager.Instance != null) GameManager.Instance.RegistrarEntradaCielo();

        // 3. El NPC habla
        if (reaccionActual != null) reaccionActual.ShowPositiveReaction();

        // 4. PAUSA DRAMATICA
        yield return new WaitForSeconds(tiempoEsperaCielo);

        // 5. Se va caminando
        if (movimientoActual != null) movimientoActual.IrAlCielo();
    }

    // GESTO: Pulgar Abajo (Veredicto verbal negativo)
    public void Decidir_Rechazar_Veredicto()
    {
        if (npcActualObj == null) return;

        if (uiController != null) uiController.OcultarDatos();

        if (reaccionActual != null) reaccionActual.ShowNegativeReaction();
    }

    // PALANCA: Ejecucion fisica (Infierno)
    public void Decidir_Rechazar_Ejecucion()
    {
        if (npcActualObj == null) return;

        if (movimientoActual.esperandoDecision)
        {
            // AVISO TUTORIAL: Ha decidido INFIERNO (false)
            OnDecisionTutorial?.Invoke(false);

            if (GameManager.Instance != null) GameManager.Instance.RegistrarRechazo();

            movimientoActual.CaerAlInfierno();

            Invoke(nameof(LimpiarYTraerSiguiente), 4f);
        }
    }

    // ---------------------------------------------------------
    // LIMPIEZA
    // ---------------------------------------------------------

    void LimpiarYTraerSiguiente()
    {
        // Desuscribir eventos para evitar errores de memoria
        if (movimientoActual != null)
        {
            movimientoActual.AlLlegarA_Trampilla -= AlLlegarAMesa;
            movimientoActual.AlLlegarA_Cielo -= LimpiarYTraerSiguiente;
        }

        if (controladorTrampilla != null) controladorTrampilla.CloseTrapdoor();

        npcActualObj = null;
        mesaOcupada = false;

        Debug.Log("Mesa libre.");

        // NOTA: En el tutorial NO llamamos a TraerSiguienteNPC aqui automaticamente,
        // dejamos que el TutorialManager decida cuando sacar al siguiente.
    }
}