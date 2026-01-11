using UnityEngine;

public class FloatingArrow : MonoBehaviour
{
    [Header("Configuración de Animación")]
    [Tooltip("Velocidad a la que sube y baja")]
    public float velocidadFlote = 2f;
    [Tooltip("Cuanto se mueve hacia arriba y abajo")]
    public float distanciaFlote = 0.05f;

    private Vector3 posicionInicial;
    private Transform camaraPrincipal;

    void Start()
    {
        posicionInicial = transform.position;

        if (Camera.main != null)
        {
            camaraPrincipal = Camera.main.transform;
        }
    }

    void Update()
    {
        // 1. BILLBOARD (Mirar siempre a la cámara)
        if (camaraPrincipal != null)
        {
            // Primero: Orientarse hacia la cámara
            transform.LookAt(camaraPrincipal);

            // SEGUNDO: CORRECCIÓN MANUAL (El giro de 180 en Z)
            // Esto obliga a la flecha a ponerse boca abajo justo después de mirar a la cámara
            //transform.Rotate(0, 0, 180);
        }

        // 2. HOVER (Flotar arriba y abajo)
        // Nota: He mantenido el '-' para que la dirección sea la que pediste
        float nuevoY = posicionInicial.y - Mathf.Sin(Time.time * velocidadFlote) * distanciaFlote;

        transform.position = new Vector3(transform.position.x, nuevoY, transform.position.z);
    }

    void OnEnable()
    {
        posicionInicial = transform.position;
    }
}