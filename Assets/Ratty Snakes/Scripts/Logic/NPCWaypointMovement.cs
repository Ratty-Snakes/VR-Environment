using UnityEngine;
using System;
using System.Collections; // Necesario para la Corrutina

public class NPCWaypointMovement : MonoBehaviour
{
    [Header("Configuración")]
    public float velocidad = 2f;
    public float velocidadRotacion = 5f;

    // --- REFERENCIAS (Se llenarán automáticamente por el NPCManager al nacer) ---
    [HideInInspector] public Transform[] listaWaypoints; // WP_0 a WP_5
    [HideInInspector] public TrapdoorController controladorTrampilla;

    // --- ESTADOS ---
    private int indiceActual = 0;
    private bool estaMoviendose = false;
    public bool esperandoDecision = false; // Para saber si está quieto en la trampilla

    // Eventos para avisar al Manager (Oye, he llegado!)
    public Action AlLlegarA_Trampilla; // WP_1
    public Action AlLlegarA_Cielo;     // WP_5

    void Update()
    {
        // Si no nos movemos, o no tenemos ruta, no hacemos nada
        if (!estaMoviendose || listaWaypoints == null || listaWaypoints.Length == 0) return;

        // Si estamos esperando la decisión del jugador, no nos movemos
        if (esperandoDecision) return;

        MoverseHaciaObjetivo();
    }

    void MoverseHaciaObjetivo()
    {
        Transform destino = listaWaypoints[indiceActual];

        // 1. Calcular dirección (ignorando altura Y para evitar inclinaciones raras)
        Vector3 direccion = (destino.position - transform.position);
        Vector3 direccionPlana = new Vector3(direccion.x, 0, direccion.z).normalized;

        // 2. Moverse
        transform.position = Vector3.MoveTowards(transform.position, destino.position, velocidad * Time.deltaTime);

        // 3. Rotar hacia donde mira (si se mueve)
        if (direccionPlana != Vector3.zero)
        {
            Quaternion rotacionObjetivo = Quaternion.LookRotation(direccionPlana);
            transform.rotation = Quaternion.Slerp(transform.rotation, rotacionObjetivo, velocidadRotacion * Time.deltaTime);
        }

        // 4. Comprobar si hemos llegado (distancia muy pequeña)
        if (Vector3.Distance(transform.position, destino.position) < 0.1f)
        {
            GestionarLlegadaPunto(indiceActual);
        }
    }

    void GestionarLlegadaPunto(int indice)
    {
        // CASO A: Llegamos al WP_1 (La Trampilla / Decisión)
        if (indice == 1)
        {
            estaMoviendose = false;
            esperandoDecision = true;
            Debug.Log("NPC: He llegado a la mesa. Esperando juicio.");

            // Avisamos al Manager para que muestre la UI
            AlLlegarA_Trampilla?.Invoke();
        }
        // CASO B: Llegamos al final (Cielo)
        else if (indice == listaWaypoints.Length - 1)
        {
            Debug.Log("NPC: ¡Estoy en el cielo! Adiós.");
            AlLlegarA_Cielo?.Invoke();
            Destroy(gameObject); // Desaparece feliz
        }
        // CASO C: Punto intermedio (curva alrededor de la mesa)
        else
        {
            indiceActual++; // Siguiente punto
        }
    }

    // --- MÉTODOS PÚBLICOS (Controlados por el Manager) ---

    // 1. INICIAR CAMINATA (Al nacer)
    public void EmpezarCaminar(Transform[] ruta, TrapdoorController trampilla)
    {
        listaWaypoints = ruta;
        controladorTrampilla = trampilla;
        indiceActual = 0; // Empieza en WP_0
        estaMoviendose = true;
        esperandoDecision = false;

        // Aseguramos que la física no interfiera al caminar
        var rb = GetComponent<Rigidbody>();
        if (rb) { rb.isKinematic = true; rb.useGravity = false; }
    }

    // 2. IR AL CIELO (Pulgar Arriba)
    public void IrAlCielo()
    {
        esperandoDecision = false;
        estaMoviendose = true;
        indiceActual++; // Pasamos del WP_1 al WP_2
    }

    // 3. CAER POR TRAMPILLA (Palanca activada)
    public void CaerAlInfierno()
    {
        esperandoDecision = false; // Ya no espera, ahora cae
        estaMoviendose = false;    // Ya no camina por waypoints
        StartCoroutine(SecuenciaCaida());
    }

    IEnumerator SecuenciaCaida()
    {
        // A. Abrir trampilla
        if (controladorTrampilla != null) controladorTrampilla.OpenTrapdoor();
        else Debug.LogError("Error: ¡El NPC no tiene referencia a la trampilla!");

        // B. Esperar un instante visual (0.5s)
        yield return new WaitForSeconds(0.5f);

        // C. Activar física para caer
        Rigidbody rb = GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.isKinematic = false; // Ahora la física le afecta
            rb.useGravity = true;   // Gravedad activada
            rb.AddForce(Vector3.down * 2f, ForceMode.Impulse); // Empujoncito hacia abajo
            rb.angularVelocity = UnityEngine.Random.insideUnitSphere * 2f; // Rotación aleatoria al caer
        }

        // D. El Manager se encargará de destruir el objeto después de unos segundos
    }
}