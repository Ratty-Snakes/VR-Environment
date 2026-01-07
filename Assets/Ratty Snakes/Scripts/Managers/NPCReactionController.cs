using UnityEngine;
using TMPro;
using System.Collections;

public class NPCReactionController : MonoBehaviour
{
    [Header("Configuración de la Burbuja")]
    [Tooltip("El objeto Canvas o Panel que contiene el gráfico del bocadillo")]
    public GameObject dialogueBubble;

    [Tooltip("El componente de texto donde escribiremos la frase")]
    public TextMeshProUGUI dialogueText;

    [Tooltip("Tiempo que el texto permanece en pantalla DESPUÉS de terminar de escribirse")]
    public float displayTime = 3f;

    [Header("Efecto Máquina de Escribir")]
    [Tooltip("Velocidad de escritura (segundos por letra). Menor es más rápido.")]
    public float typingSpeed = 0.05f;

    // Datos del NPC actual
    private NPCData currentData;
    // Guardamos la corrutina activa para poder pararla si cambiamos de frase rápido
    private Coroutine activeCoroutine;

    void Start()
    {
        if (dialogueBubble != null)
            dialogueBubble.SetActive(false);
    }

    public void Initialize(NPCData data)
    {
        currentData = data;
    }

    // --- NUEVO: Llamado por NPCWaypointMovement al llegar al punto 1 ---
    public void MostrarFraseEntrada()
    {
        // Solo mostramos si hay datos y si la frase no está vacía
        if (currentData != null && !string.IsNullOrEmpty(currentData.fraseEntrada))
        {
            StartReaction(currentData.fraseEntrada);
        }
    }
    // ------------------------------------------------------------------

    // Llamado al ACEPTAR
    public void ShowPositiveReaction()
    {
        if (currentData != null)
        {
            StartReaction(currentData.reaccionPositiva);
        }
    }

    // Llamado al RECHAZAR
    public void ShowNegativeReaction()
    {
        if (currentData != null)
        {
            StartReaction(currentData.reaccionNegativa);
        }
    }

    // Función auxiliar para reiniciar el proceso limpiamente
    private void StartReaction(string message)
    {
        // Si ya estaba hablando, le cortamos la frase anterior
        if (activeCoroutine != null) StopCoroutine(activeCoroutine);

        activeCoroutine = StartCoroutine(TypewriterRoutine(message));
    }

    // La Corrutina con el efecto Typewriter
    private IEnumerator TypewriterRoutine(string message)
    {
        if (dialogueBubble == null || dialogueText == null) yield break;

        // 1. Limpiar y Mostrar
        dialogueText.text = ""; // Empezamos vacíos
        dialogueBubble.SetActive(true);

        // 2. Bucle de escritura (Letra a letra)
        foreach (char letter in message.ToCharArray())
        {
            dialogueText.text += letter;

            // Aquí podrías añadir sonido de "blip" si quisieras:
            // audioSource.PlayOneShot(blipSound);

            yield return new WaitForSeconds(typingSpeed);
        }

        // 3. Esperar para leer
        yield return new WaitForSeconds(displayTime);

        // 4. Ocultar
        dialogueBubble.SetActive(false);
        activeCoroutine = null;
    }
}