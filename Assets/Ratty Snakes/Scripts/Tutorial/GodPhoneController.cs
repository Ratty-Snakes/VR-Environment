using UnityEngine;
using TMPro;
using System.Collections;

public class GodPhoneController : MonoBehaviour
{
    [Header("Referencias")]
    public AudioSource audioSource;
    public GameObject canvasSubtitulos;
    public TextMeshProUGUI textoSubtitulos;

    [Header("Assets")]
    public AudioClip sonidoRing;
    public AudioClip sonidoColgar; // El sonido "Clack" mecánico

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
    // 🔌 MÉTODOS PARA EL SOCKET INTERACTOR (LA BASE FÍSICA)
    // ====================================================================
    // Conecta estos al XR Socket Interactor de la base del teléfono.

    // Evento: Select Entered (Al ponerlo en la base)
    public void PonerEnBase()
    {
        estaColgado = true;
        Debug.Log("📞 Físicas: Teléfono puesto en la base.");

        // Feedback sonoro mecánico (Clack)
        if (audioSource && sonidoColgar) audioSource.PlayOneShot(sonidoColgar);

        // Llamamos a la lógica original del juego
        AlColgarTelefono();

        // Restauramos rotación por si acaso
        RestaurarRotacion();
    }

    // Evento: Select Exited (Al cogerlo de la base)
    public void QuitarDeBase()
    {
        estaColgado = false;
        Debug.Log("📞 Físicas: Teléfono levantado.");

        // Llamamos a la lógica original del juego
        AlDescolgarTelefono();
    }

    // ====================================================================
    // 🧠 LÓGICA ORIGINAL (COMPATIBILIDAD CON GAME MANAGER)
    // ====================================================================

    public void AlDescolgarTelefono()
    {
        if (estaSonando)
        {
            ContestarLlamada();
        }
        else
        {
            // Si lo coges sin que suene
            RestaurarRotacion();
            // Debug.Log("Has descolgado, pero nadie llamaba.");
        }
    }

    public void AlColgarTelefono()
    {
        if (llamadaEnCurso)
        {
            Colgar();
        }
        // Aquí podrías añadir lógica extra si el GameManager necesita saber que has colgado aunque no hubiera llamada
    }

    // --- LÓGICA INTERNA ---

    public void EmpezarA_Sonar()
    {
        if (llamadaEnCurso || estaSonando) return;

        estaSonando = true;
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

        // 2. Notificar al GameManager (Si existe en la escena)
        // Descomenta esto cuando estés en la escena del juego real
        /*
        if (GameManager.Instance != null)
        {
            GameManager.Instance.JugadorContestoTelefono();
        }
        */
    }

    public void Colgar()
    {
        llamadaEnCurso = false;
        if (canvasSubtitulos != null) canvasSubtitulos.SetActive(false);
        audioSource.Stop();
        RestaurarRotacion();

        // Aquí podrías notificar al GameManager si hiciera falta
    }

    public void ReproducirFraseDios(string texto, AudioClip audioVoz = null)
    {
        if (rutinaLlamada != null) StopCoroutine(rutinaLlamada);
        rutinaLlamada = StartCoroutine(RutinaHablar(texto, audioVoz));
    }

    IEnumerator RutinaHablar(string texto, AudioClip audioVoz)
    {
        if (canvasSubtitulos != null) canvasSubtitulos.SetActive(true);
        textoSubtitulos.text = "";
        if (audioVoz != null) audioSource.PlayOneShot(audioVoz);

        foreach (char letra in texto.ToCharArray())
        {
            textoSubtitulos.text += letra;
            yield return new WaitForSeconds(0.03f);
        }
    }

    void RestaurarRotacion()
    {
        if (modeloVisualAuricular != null)
        {
            modeloVisualAuricular.localRotation = rotacionOriginalVisual;
        }
    }
}