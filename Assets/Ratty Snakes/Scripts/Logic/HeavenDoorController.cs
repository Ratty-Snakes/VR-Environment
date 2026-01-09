using UnityEngine;
using System.Collections;

public class HeavenDoorController : MonoBehaviour
{
    [Header("Referencias a los Pivotes")]
    public Transform pivotePuertaIzq;
    public Transform pivotePuertaDer;

    [Header("Configuración")]
    [Tooltip("Grados por segundo. Ponlo bajo (ej: 15) para que sea solemne.")]
    public float velocidadRotacion = 15f;

    [Tooltip("Ángulo final de apertura (ej: 90 grados).")]
    public float anguloApertura = 90f;

    [Header("Audio")]
    public AudioSource audioSource;
    public AudioClip sonidoAbrir;

    private bool estanAbiertas = false;

    // Guardamos la rotación inicial para poder cerrarlas si hiciera falta
    private Quaternion rotInicialIzq;
    private Quaternion rotInicialDer;

    void Start()
    {
        if (pivotePuertaIzq) rotInicialIzq = pivotePuertaIzq.localRotation;
        if (pivotePuertaDer) rotInicialDer = pivotePuertaDer.localRotation;
    }

    public void AbrirPuertas()
    {
        if (estanAbiertas) return; // Ya están abiertas

        estanAbiertas = true;

        // Sonido
        if (audioSource && sonidoAbrir)
        {
            audioSource.PlayOneShot(sonidoAbrir);
        }

        StopAllCoroutines();
        StartCoroutine(ProcesoApertura());
    }

    public void CerrarPuertas()
    {
        if (!estanAbiertas) return;
        estanAbiertas = false;
        StopAllCoroutines();
        StartCoroutine(ProcesoCierre());
    }

    IEnumerator ProcesoApertura()
    {
        // Calculamos rotación objetivo
        // Nota: Dependiendo de tus ejes, puede que necesites poner -anguloApertura en una de las dos
        Quaternion destinoIzq = rotInicialIzq * Quaternion.Euler(0, -anguloApertura, 0);
        Quaternion destinoDer = rotInicialDer * Quaternion.Euler(0, anguloApertura, 0);

        float tiempo = 0;

        // Bucle de movimiento
        while (Quaternion.Angle(pivotePuertaIzq.localRotation, destinoIzq) > 0.1f)
        {
            float paso = velocidadRotacion * Time.deltaTime;

            if (pivotePuertaIzq)
                pivotePuertaIzq.localRotation = Quaternion.RotateTowards(pivotePuertaIzq.localRotation, destinoIzq, paso);

            if (pivotePuertaDer)
                pivotePuertaDer.localRotation = Quaternion.RotateTowards(pivotePuertaDer.localRotation, destinoDer, paso);

            yield return null;
        }
    }

    IEnumerator ProcesoCierre()
    {
        while (Quaternion.Angle(pivotePuertaIzq.localRotation, rotInicialIzq) > 0.1f)
        {
            float paso = velocidadRotacion * Time.deltaTime;

            if (pivotePuertaIzq)
                pivotePuertaIzq.localRotation = Quaternion.RotateTowards(pivotePuertaIzq.localRotation, rotInicialIzq, paso);

            if (pivotePuertaDer)
                pivotePuertaDer.localRotation = Quaternion.RotateTowards(pivotePuertaDer.localRotation, rotInicialDer, paso);

            yield return null;
        }
    }
}