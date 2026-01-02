using UnityEngine;
using System.Collections;

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

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    void Start()
    {
        Debug.Log("Esperando a que el jugador pulse el boton de SIGUIENTE.");
    }

    // Boton de la Mesa
    public void BotonSiguientePulsado()
    {
        if (mesaOcupada)
        {
            Debug.Log("[AVISO] Espera! Todavia hay un alma en proceso.");
            return;
        }

        Debug.Log("Llamando al siguiente...");
        TraerSiguienteNPC();
    }

    public void TraerSiguienteNPC()
    {
        datosActuales = GameManager.Instance.ObtenerSiguienteNPC();

        if (datosActuales == null)
        {
            Debug.Log("Jornada terminada! No quedan mas NPCs.");
            return;
        }

        mesaOcupada = true;

        // 1. Instanciar el CONTENEDOR (NPC_Base)
        npcActualObj = Instantiate(npcPrefab, listaWaypoints[0].position, listaWaypoints[0].rotation);

        // --- Logica de cambio de modelo ---
        MeshRenderer baseMesh = npcActualObj.GetComponent<MeshRenderer>();
        if (baseMesh != null) baseMesh.enabled = false;

        if (datosActuales.modeloEspecifico != null)
        {
            GameObject modeloVisual = Instantiate(datosActuales.modeloEspecifico, npcActualObj.transform);
            modeloVisual.transform.localPosition = Vector3.zero;
            modeloVisual.transform.localRotation = Quaternion.identity;
        }
        else
        {
            if (baseMesh != null) baseMesh.enabled = true;
        }
        // ------------------------------------------

        // 2. Configurar Movimiento
        movimientoActual = npcActualObj.GetComponent<NPCWaypointMovement>();
        if (movimientoActual != null)
        {
            movimientoActual.EmpezarCaminar(listaWaypoints, controladorTrampilla);
            movimientoActual.AlLlegarA_Trampilla += AlLlegarAMesa;
            movimientoActual.AlLlegarA_Cielo += LimpiarYTraerSiguiente;
        }

        // 3. Configurar Reaccion
        reaccionActual = npcActualObj.GetComponent<NPCReactionController>();
        if (reaccionActual != null)
        {
            reaccionActual.Initialize(datosActuales);
        }
    }

    void AlLlegarAMesa()
    {
        if (uiController != null) uiController.MostrarDatos(datosActuales);
    }

    // --- DECISIONES ---

    // GESTO: Pulgar Arriba (Cielo)
    public void Decidir_Aceptar()
    {
        if (npcActualObj == null) return;

        // Iniciamos la secuencia con espera
        StartCoroutine(SecuenciaAceptarCielo());
    }

    // CORRUTINA PARA LA PAUSA DRAMATICA
    IEnumerator SecuenciaAceptarCielo()
    {
        Debug.Log("Veredicto: ACEPTADO. Esperando para irse...");

        // 1. Ocultamos los datos inmediatamente para limpiar la mesa
        if (uiController != null) uiController.OcultarDatos();

        // 2. Registramos el punto
        GameManager.Instance.RegistrarEntradaCielo();

        // 3. El NPC habla (Typewriter effect)
        if (reaccionActual != null) reaccionActual.ShowPositiveReaction();

        // 4. PAUSA: Esperamos aqui el tiempo configurado mientras el NPC sigue quieto
        yield return new WaitForSeconds(tiempoEsperaCielo);

        // 5. Ahora si, se va caminando
        if (movimientoActual != null) movimientoActual.IrAlCielo();
    }

    // GESTO: Pulgar Abajo (Veredicto verbal)
    public void Decidir_Rechazar_Veredicto()
    {
        if (npcActualObj == null) return;

        if (uiController != null) uiController.OcultarDatos();

        if (reaccionActual != null) reaccionActual.ShowNegativeReaction();
        // Aqui no hace falta espera, el NPC se queda quieto esperando la palanca por defecto
    }

    // PALANCA: Ejecucion fisica
    public void Decidir_Rechazar_Ejecucion()
    {
        if (npcActualObj == null) return;

        if (movimientoActual.esperandoDecision)
        {
            GameManager.Instance.RegistrarRechazo();
            movimientoActual.CaerAlInfierno();

            Invoke(nameof(LimpiarYTraerSiguiente), 4f);
        }
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
        Debug.Log("Mesa libre.");
    }
}