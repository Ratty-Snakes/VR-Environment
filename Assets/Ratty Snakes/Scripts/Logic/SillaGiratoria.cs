using UnityEngine;

public class SillaGiratoria : MonoBehaviour
{
    [Header("Referencias")]
    [Tooltip("Arrastra aquí tu Main Camera o el XR Origin")]
    public Transform objetivoASeguir;

    [Header("Rotación (Giro)")]
    [Tooltip("Velocidad de giro.")]
    public float velocidadGiro = 10f;
    [Tooltip("Ajuste por si la silla está rotada 90 o 180 grados respecto a la cámara")]
    public float offsetRotacionY = 0f;

    [Header("Traslación (Movimiento)")]
    [Tooltip("Velocidad a la que sigue la posición. Si quieres que sea instantáneo, pon un valor muy alto (ej. 50).")]
    public float velocidadMovimiento = 10f;

    [Tooltip("Activar si quieres mantener una distancia fija respecto a la cámara (offset inicial)")]
    public bool mantenerOffsetInicial = false;
    private Vector3 offsetPosicion;

    void Start()
    {
        if (objetivoASeguir != null && mantenerOffsetInicial)
        {
            // Calculamos la diferencia inicial entre la silla y la cámara (solo en X y Z)
            offsetPosicion = transform.position - new Vector3(objetivoASeguir.position.x, transform.position.y, objetivoASeguir.position.z);
        }
    }

    void LateUpdate()
    {
        if (objetivoASeguir == null) return;

        // --- 1. LOGICA DE ROTACIÓN (Tu código original) ---
        float rotacionYObjetivo = objetivoASeguir.eulerAngles.y;
        Quaternion destinoRot = Quaternion.Euler(transform.eulerAngles.x, rotacionYObjetivo + offsetRotacionY, transform.eulerAngles.z);
        transform.rotation = Quaternion.Lerp(transform.rotation, destinoRot, Time.deltaTime * velocidadGiro);

        // --- 2. LOGICA DE POSICIÓN (Nueva) ---

        // Obtenemos la posición destino:
        // X y Z = Las del objetivo (cámara).
        // Y = La altura actual de la silla (para que no vuele).
        Vector3 posicionObjetivo = new Vector3(objetivoASeguir.position.x, transform.position.y, objetivoASeguir.position.z);

        // Si usamos offset, se lo sumamos
        if (mantenerOffsetInicial)
        {
            posicionObjetivo += offsetPosicion;
        }

        // Aplicamos el movimiento suave
        transform.position = Vector3.Lerp(transform.position, posicionObjetivo, Time.deltaTime * velocidadMovimiento);
    }
}