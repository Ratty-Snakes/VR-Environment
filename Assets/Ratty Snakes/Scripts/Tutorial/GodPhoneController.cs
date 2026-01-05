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

    [Header("Efecto Vibración")]
    public Transform modeloVisualAuricular; // <--- ARRASTRA AQUÍ LA MALLA (HIJO) DEL AURICULAR
    public float velocidadVibracion = 20f;  // Qué tan rápido tiembla
    public float anguloMaximo = 5f;         // Cuánto gira (5 a -5 grados)

    private bool estaSonando = false;
    private bool llamadaEnCurso = false;
    private Coroutine rutinaLlamada;
    private Quaternion rotacionOriginalVisual; // Para recordar cómo estaba antes de temblar

    void Start()
    {
        // 1. Guardamos la rotación original para restaurarla luego
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
        // SI ESTÁ SONANDO -> HACEMOS EL EFECTO DE VIBRACIÓN
        if (estaSonando && modeloVisualAuricular != null)
        {
            // Calculamos el ángulo usando Seno (va de -1 a 1 suavemente)
            float anguloZ = Mathf.Sin(Time.time * velocidadVibracion) * anguloMaximo;

            // Aplicamos la rotación SOLO en el eje Z local
            modeloVisualAuricular.localRotation = Quaternion.Euler(0, 0, anguloZ);
        }
    }

    // --- MÉTODOS PÚBLICOS ---

    public void AlDescolgarTelefono()
    {
        if (estaSonando)
        {
            ContestarLlamada();
        }
        else
        {
            // Si lo coges sin que suene, nos aseguramos de que el visual esté recto
            RestaurarRotacion();
            Debug.Log("Has descolgado, pero nadie llamaba.");
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
        audioSource.clip = sonidoRing;
        audioSource.loop = true;
        audioSource.Play();
    }

    void ContestarLlamada()
    {
        estaSonando = false;
        llamadaEnCurso = true;

        // PARAMOS EL RING Y LA VIBRACIÓN
        audioSource.Stop();
        audioSource.loop = false;
        RestaurarRotacion(); // <--- IMPORTANTE: Que deje de estar torcido al cogerlo

        if (TutorialManager.Instance != null)
        {
            TutorialManager.Instance.JugadorContestoTelefono();
        }
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

    public void Colgar()
    {
        llamadaEnCurso = false;
        if (canvasSubtitulos != null) canvasSubtitulos.SetActive(false);
        audioSource.Stop();
        RestaurarRotacion();
    }

    // Función auxiliar para dejar el teléfono quieto
    void RestaurarRotacion()
    {
        if (modeloVisualAuricular != null)
        {
            modeloVisualAuricular.localRotation = rotacionOriginalVisual;
        }
    }
}