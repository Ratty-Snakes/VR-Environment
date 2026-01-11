using UnityEngine;

public class NPCHoverEffect : MonoBehaviour
{
    [Header("Configuración Levitación")]
    public float amplitud = 0.05f; // Cuánto sube y baja (muy sutil)
    public float velocidad = 1.5f; // Cuán rápido lo hace
    public bool aleatorizarInicio = true; // Para que no todos floten sincronizados

    private Vector3 posInicial;
    private float offsetTiempo;
    private bool estaActivo = false;

    void Awake()
    {
        // Guardamos la posición local original (0,0,0 respecto al padre)
        posInicial = transform.localPosition;
    }

    public void ActivarLevitacion()
    {
        if (aleatorizarInicio) offsetTiempo = Random.Range(0f, 10f);
        estaActivo = true;
    }

    public void DesactivarLevitacion()
    {
        estaActivo = false;
        // Volvemos suavemente a la posición original (opcional, o salto directo)
        transform.localPosition = posInicial;
    }

    void Update()
    {
        if (!estaActivo) return;

        // Fórmula matemática de la onda: Sin(tiempo * velocidad) * altura
        float nuevoY = posInicial.y + Mathf.Sin((Time.time + offsetTiempo) * velocidad) * amplitud;

        // Aplicamos solo al eje Y local
        transform.localPosition = new Vector3(posInicial.x, nuevoY, posInicial.z);
    }
}