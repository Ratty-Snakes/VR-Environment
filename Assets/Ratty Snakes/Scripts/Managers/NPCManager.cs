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

    [Header("Estado Actual")]
    private GameObject npcActualObj;
    private NPCWaypointMovement movimientoActual;
    private NPCReactionController reaccionActual;
    private NPCData datosActuales;

    // <--- CAMBIO: Variable para saber si la mesa está ocupada
    private bool mesaOcupada = false;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    void Start()
    {
        // <--- CAMBIO: YA NO llamamos a TraerSiguienteNPC automáticamente al inicio.
        // El juego empieza vacío esperando que pulses el botón.
        Debug.Log("Esperando a que el jugador pulse el botón de SIGUIENTE.");
    }

    // <--- CAMBIO: Nueva función pública para el Botón de la Mesa
    public void BotonSiguientePulsado()
    {
        // Si ya hay alguien en la mesa o procesándose, el botón no hace nada
        if (mesaOcupada)
        {
            Debug.Log("¡Espera! Todavía hay un alma en proceso.");
            return;
        }

        Debug.Log("¡DING! Llamando al siguiente...");
        TraerSiguienteNPC();
    }

    public void TraerSiguienteNPC()
    {
        datosActuales = GameManager.Instance.ObtenerSiguienteNPC();

        if (datosActuales == null)
        {
            Debug.Log("¡Jornada terminada! No quedan más NPCs.");
            return;
        }

        // <--- CAMBIO: Marcamos la mesa como ocupada
        mesaOcupada = true;

        Debug.Log($"Procesando a: {datosActuales.nombre}");

        // Instanciar
        npcActualObj = Instantiate(npcPrefab, listaWaypoints[0].position, listaWaypoints[0].rotation);

        // 1. Configurar Movimiento
        movimientoActual = npcActualObj.GetComponent<NPCWaypointMovement>();
        if (movimientoActual != null)
        {
            movimientoActual.EmpezarCaminar(listaWaypoints, controladorTrampilla);
            movimientoActual.AlLlegarA_Trampilla += AlLlegarAMesa;
            movimientoActual.AlLlegarA_Cielo += LimpiarYTraerSiguiente;
        }

        // 2. Configurar Reacción
        reaccionActual = npcActualObj.GetComponent<NPCReactionController>();
        if (reaccionActual != null)
        {
            reaccionActual.Initialize(datosActuales);
        }
    }

    void AlLlegarAMesa()
    {
        Debug.Log("EL JUEZ DEBE DECIDIR AHORA.");
        if (uiController != null) uiController.MostrarDatos(datosActuales);
    }

    // --- DECISIONES ---

    public void Decidir_Aceptar()
    {
        if (npcActualObj == null) return;

        if (uiController != null) uiController.OcultarDatos();
        GameManager.Instance.RegistrarEntradaCielo();

        if (reaccionActual != null) reaccionActual.ShowPositiveReaction();

        movimientoActual.IrAlCielo();

        // Nota: mesaOcupada sigue siendo true hasta que el NPC se vaya del todo
    }

    public void Decidir_Rechazar_Veredicto()
    {
        if (npcActualObj == null) return;

        if (uiController != null) uiController.OcultarDatos();

        if (reaccionActual != null) reaccionActual.ShowNegativeReaction();
    }

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

    // Esta función se llama cuando el NPC llega al cielo O cuando ha caído por la trampilla
    void LimpiarYTraerSiguiente()
    {
        if (movimientoActual != null)
        {
            movimientoActual.AlLlegarA_Trampilla -= AlLlegarAMesa;
            movimientoActual.AlLlegarA_Cielo -= LimpiarYTraerSiguiente;
        }

        if (controladorTrampilla != null) controladorTrampilla.CloseTrapdoor();

        // Limpiamos referencias del objeto anterior
        npcActualObj = null;

        // <--- CAMBIO CRUCIAL:
        // Antes llamábamos a TraerSiguienteNPC() aquí. 
        // AHORA solo liberamos la mesa para que el botón funcione de nuevo.
        mesaOcupada = false;
        Debug.Log("Mesa libre. Pulsa el botón para el siguiente.");
    }
}