using UnityEngine;
using TMPro;
using System.Collections;

public class GodPhoneController : MonoBehaviour
{
    [Header("Referencias")]
    public AudioSource audioSource;
    public GameObject canvasSubtitulos;
    public TextMeshProUGUI textoSubtitulos;

    [Header("Audio General")]
    public AudioClip sonidoRing;
    public AudioClip sonidoColgar; // El sonido "Clack" mecánico cuando lo pones en la base

    [Header("Audio Voz (Efecto Undertale)")]
    public AudioClip sonidoVoz; // <--- ARRASTRA AQUÍ EL SONIDO "BLIP" CORTO
    [Range(0.01f, 0.2f)]
    public float velocidadEscritura = 0.05f; // Velocidad del texto
    [Range(0.8f, 1.2f)]
    public float variacionTono = 1.1f;       // Cuánto desafina (Personalidad)

    [Header("Efecto Vibración")]
    public Transform modeloVisualAuricular;
    public float velocidadVibracion = 20f;
    public float anguloMaximo = 5f;

    [Header("Estado Físico")]
    public bool estaColgado = true; // Variable clave para el TutorialManager

    // Estado interno
    private bool estaSonando = false;
    private bool llamadaEnCurso = false;
    private Coroutine rutinaLlamada;
    private Quaternion rotacionOriginalVisual;

    void Start()
    {
        // 1. Guardamos la rotación original
        if (modeloVisualAuricular != null)
        {
            rotacionOriginalVisual = modeloVisualAuricular.localRotation;
        }

        // 2. Ocultamos subtítulos
        if (canvasSubtitulos != null)
        {
            canvasSubtitulos.SetActive(false);
        }
    }

    void Update()
    {
        // Efecto de vibración
        if (estaSonando && modeloVisualAuricular != null)
        {
            float anguloZ = Mathf.Sin(Time.time * velocidadVibracion) * anguloMaximo;
            modeloVisualAuricular.localRotation = Quaternion.Euler(0, 0, anguloZ);
        }
    }

    // ====================================================================
    // MÉTODOS PARA EL SOCKET INTERACTOR (LA BASE FÍSICA)
    // ====================================================================

    public void PonerEnBase()
    {
        estaColgado = true;
        Debug.Log("Físicas: Teléfono puesto en la base.");

        // Feedback sonoro mecánico (Clack)
        if (audioSource && sonidoColgar)
        {
            audioSource.pitch = 1f; // Reseteamos pitch por si acaso
            audioSource.PlayOneShot(sonidoColgar);
        }

        // Llamamos a la lógica original del juego
        AlColgarTelefono();

        RestaurarRotacion();
    }

    public void QuitarDeBase()
    {
        estaColgado = false;
        Debug.Log("Físicas: Teléfono levantado.");
        AlDescolgarTelefono();
    }

    // ====================================================================
    // LÓGICA DE JUEGO
    // ====================================================================

    public void AlDescolgarTelefono()
    {
        if (estaSonando)
        {
            ContestarLlamada();
        }
        else
        {
            RestaurarRotacion();
        }
    }

    public void AlColgarTelefono()
    {
        if (llamadaEnCurso)
        {
            Colgar();
        }
    }

    // --- LÓGICA INTERNA ---

    public void EmpezarA_Sonar()
    {
        if (llamadaEnCurso || estaSonando) return;

        estaSonando = true;
        audioSource.pitch = 1f; // Pitch normal para el Ring
        audioSource.clip = sonidoRing;
        audioSource.loop = true;
        audioSource.Play();
    }

    void ContestarLlamada()
    {
        estaSonando = false;
        llamadaEnCurso = true;

        audioSource.Stop();
        audioSource.loop = false;
        RestaurarRotacion();

        // 1. Notificar al Tutorial
        if (TutorialManager.Instance != null)
        {
            TutorialManager.Instance.JugadorContestoTelefono();
        }
    }

    public void Colgar()
    {
        llamadaEnCurso = false;
        if (canvasSubtitulos != null) canvasSubtitulos.SetActive(false);

        audioSource.Stop();
        RestaurarRotacion();
    }

    public void ReproducirFraseDios(string texto)
    {
        if (rutinaLlamada != null) StopCoroutine(rutinaLlamada);
        rutinaLlamada = StartCoroutine(RutinaHablar(texto));
    }

    // --- LA CORRUTINA MAGICA TIPO UNDERTALE ---
    IEnumerator RutinaHablar(string texto)
    {
        if (canvasSubtitulos != null) canvasSubtitulos.SetActive(true);
        textoSubtitulos.text = "";

        foreach (char letra in texto.ToCharArray())
        {
            textoSubtitulos.text += letra;

            // Lógica de sonido por letra
            if (!char.IsWhiteSpace(letra) && audioSource != null && sonidoVoz != null)
            {
                // Variamos el tono aleatoriamente
                audioSource.pitch = Random.Range(1f - (variacionTono - 1f), variacionTono);
                audioSource.PlayOneShot(sonidoVoz);
            }

            yield return new WaitForSeconds(velocidadEscritura);
        }

        // Al terminar la frase, reseteamos el pitch para el futuro
        if (audioSource != null) audioSource.pitch = 1f;
    }

    void RestaurarRotacion()
    {
        if (modeloVisualAuricular != null)
        {
            modeloVisualAuricular.localRotation = rotacionOriginalVisual;
        }
    }
}