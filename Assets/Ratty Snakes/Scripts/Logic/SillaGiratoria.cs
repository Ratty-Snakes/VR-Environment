using UnityEngine;

public class SillaGiratoria : MonoBehaviour
{
    [Header("Referencias")]
    [Tooltip("Arrastra aquí tu Main Camera o el XR Origin")]
    public Transform objetivoASeguir;

    [Header("Configuración")]
    [Tooltip("Velocidad de giro. Cuanto más bajo, más suave (con retraso).")]
    public float velocidadSuavizado = 10f;

    [Tooltip("Ajuste por si la silla está rotada 90 o 180 grados respecto a la cámara")]
    public float offsetRotacionY = 0f;

    void LateUpdate()
    {
        if (objetivoASeguir == null) return;

        // 1. Obtenemos la rotación Y del objetivo (Cámara/Jugador)
        float rotacionYObjetivo = objetivoASeguir.eulerAngles.y;

        // 2. Creamos la rotación destino (Mantenemos la X y Z de la silla quietas)
        // Solo cambiamos la Y, sumando el offset si la silla estaba mal orientada
        Quaternion destino = Quaternion.Euler(transform.eulerAngles.x, rotacionYObjetivo + offsetRotacionY, transform.eulerAngles.z);

        // 3. Aplicamos la rotación con suavizado (Lerp) para que no sea brusco
        transform.rotation = Quaternion.Lerp(transform.rotation, destino, Time.deltaTime * velocidadSuavizado);
    }
}