using System.Collections;
using TMPro;
using Unity.Tutorials.Core.Editor;
using UnityEngine;

public class GodPhoneController : MonoBehaviour
{
    [Header("Referencias")]
    public AudioSource audioSource; // El sonido del RING y la VOZ
    public GameObject canvasSubtitulos;
    public TextMeshProUGUI textoSubtitulos;

    [Header("Assets")]
    public AudioClip sonidoRing;

    private bool estaSonando = false;
    private bool llamadaEnCurso = false;
    private Coroutine rutinaLlamada;

    // --- MÉTODOS PÚBLICOS (Para conectar en el Inspector) ---

    // Conectar al evento "Select Exited" del SOCKET BASE
    public void AlDescolgarTelefono()
    {
        if (estaSonando)
        {
            ContestarLlamada();
        }
        else
        {
            Debug.Log("Has descolgado, pero nadie llamaba.");
        }
    }

    // Conectar al evento "Select Entered" del SOCKET BASE
    public void AlColgarTelefono()
    {
        Debug.Log("Teléfono colgado en la base.");
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

        if (canvasSubtitulos != null)
        {
            canvasSubtitulos.SetActive(true);
            textoSubtitulos.text = "* RINNNG RINNNG *";
        }
    }

    public void ContestarLlamada()
    {
        estaSonando = false;
        llamadaEnCurso = true;

        // Paramos el Ring
        audioSource.Stop();
        audioSource.loop = false;

        // Avisamos al TutorialManager (lo haremos en el siguiente paso)
        TutorialManager.Instance.JugadorContestoTelefono();
    }

    public void ReproducirFraseDios(string texto, AudioClip audioVoz = null)
    {
        if (rutinaLlamada != null) StopCoroutine(rutinaLlamada);
        rutinaLlamada = StartCoroutine(RutinaHablar(texto, audioVoz));
    }

    IEnumerator RutinaHablar(string texto, AudioClip audioVoz)
    {
        canvasSubtitulos.SetActive(true);
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
    }
}