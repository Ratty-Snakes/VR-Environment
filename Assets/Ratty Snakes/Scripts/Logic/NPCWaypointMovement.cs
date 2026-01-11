using UnityEngine;
using System;
using System.Collections;

public class NPCWaypointMovement : MonoBehaviour
{
    [Header("Configuración")]
    public float velocidad = 2f;
    public float velocidadRotacion = 5f;

    // --- REFERENCIAS ---
    [HideInInspector] public Transform[] listaWaypoints;
    [HideInInspector] public TrapdoorController controladorTrampilla;

    // Referencia al controlador de textos (NUEVO)
    private NPCReactionController reactionController;

    // --- ESTADOS ---
    private int indiceActual = 0;
    private bool estaMoviendose = false;
    public bool esperandoDecision = false;

    // Eventos
    public Action AlLlegarA_Trampilla;
    public Action AlLlegarA_Cielo;

    private void Awake()
    {
        // Obtenemos la referencia al nacer (NUEVO)
        reactionController = GetComponent<NPCReactionController>();
    }

    void Update()
    {
        if (!estaMoviendose || listaWaypoints == null || listaWaypoints.Length == 0) return;
        if (esperandoDecision) return;

        MoverseHaciaObjetivo();
    }

    void MoverseHaciaObjetivo()
    {
        Transform destino = listaWaypoints[indiceActual];

        // 1. Calcular dirección
        Vector3 direccion = (destino.position - transform.position);
        Vector3 direccionPlana = new Vector3(direccion.x, 0, direccion.z).normalized;

        // 2. Moverse
        transform.position = Vector3.MoveTowards(transform.position, destino.position, velocidad * Time.deltaTime);

        // 3. Rotar
        if (direccionPlana != Vector3.zero)
        {
            Quaternion rotacionObjetivo = Quaternion.LookRotation(direccionPlana);
            transform.rotation = Quaternion.Slerp(transform.rotation, rotacionObjetivo, velocidadRotacion * Time.deltaTime);
        }

        // 4. Comprobar llegada
        if (Vector3.Distance(transform.position, destino.position) < 0.1f)
        {
            GestionarLlegadaPunto(indiceActual);
        }
    }

    void GestionarLlegadaPunto(int indice)
    {
        // CAMBIO AQUÍ: Ahora la mesa es el índice 2 (porque el 1 es el pasillo intermedio)
        if (indice == 2)
        {
            estaMoviendose = false;
            esperandoDecision = true;
            Debug.Log("NPC: He llegado a la mesa. Esperando juicio.");

            if (reactionController != null)
            {
                reactionController.MostrarFraseEntrada();
            }

            AlLlegarA_Trampilla?.Invoke();
        }
        // CASO B: Llegamos al final (Cielo)
        // Esto sigue funcionando igual porque usa .Length (el último)
        else if (indice == listaWaypoints.Length - 1)
        {
            Debug.Log("NPC: ¡Estoy en el cielo! Adiós.");
            AlLlegarA_Cielo?.Invoke();
            Destroy(gameObject);
        }
        // CASO C: Punto intermedio (Spawn o Pasillo)
        else
        {
            indiceActual++;
        }
    }

    // --- MÉTODOS PÚBLICOS ---

    public void EmpezarCaminar(Transform[] ruta, TrapdoorController trampilla)
    {
        listaWaypoints = ruta;
        controladorTrampilla = trampilla;
        indiceActual = 0;
        estaMoviendose = true;
        esperandoDecision = false;

        var rb = GetComponent<Rigidbody>();
        if (rb) { rb.isKinematic = true; rb.useGravity = false; }
    }

    public void IrAlCielo()
    {
        esperandoDecision = false;
        estaMoviendose = true;
        indiceActual++;
    }

    public void CaerAlInfierno()
    {
        esperandoDecision = false;
        estaMoviendose = false;
        StartCoroutine(SecuenciaCaida());
    }

    IEnumerator SecuenciaCaida()
    {
        if (controladorTrampilla != null) controladorTrampilla.OpenTrapdoor();
        else Debug.LogError("Error: ¡El NPC no tiene referencia a la trampilla!");

        yield return new WaitForSeconds(0.5f);

        Rigidbody rb = GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.isKinematic = false;
            rb.useGravity = true;
            rb.AddForce(Vector3.down * 2f, ForceMode.Impulse);
            rb.angularVelocity = UnityEngine.Random.insideUnitSphere * 2f;
        }
    }
}