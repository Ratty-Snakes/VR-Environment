using UnityEngine;
using TMPro; // Necesario para usar TextMeshPro
using System.Collections;

public class NPCReactionController : MonoBehaviour
{
    [Header("Configuración de la Burbuja")]
    [Tooltip("El objeto Canvas o Panel que contiene el gráfico del bocadillo")]
    public GameObject dialogueBubble;

    [Tooltip("El componente de texto donde escribiremos la frase")]
    public TextMeshProUGUI dialogueText;

    [Tooltip("Tiempo que el texto permanece en pantalla")]
    public float displayTime = 4f;

    // Datos del NPC actual (se rellenan al nacer)
    private NPCData currentData;

    void Start()
    {
        // Al empezar, nos aseguramos de que la burbuja esté oculta
        if (dialogueBubble != null)
            dialogueBubble.SetActive(false);
    }

    // Este método lo llama el NPCManager justo después de instanciar al NPC
    public void Initialize(NPCData data)
    {
        currentData = data;
    }

    // Llamado cuando pulsas el botón VERDE (Salvar)
    public void ShowPositiveReaction()
    {
        if (currentData != null)
        {
            StartCoroutine(ShowAndHideCoroutine(currentData.reaccionPositiva));
        }
    }

    // Llamado cuando pulsas el botón ROJO (Sentenciar)
    public void ShowNegativeReaction()
    {
        if (currentData != null)
        {
            StartCoroutine(ShowAndHideCoroutine(currentData.reaccionNegativa));
        }
    }

    // Corrutina para mostrar, esperar y ocultar
    private IEnumerator ShowAndHideCoroutine(string message)
    {
        if (dialogueBubble == null || dialogueText == null)
        {
            Debug.LogWarning("Falta asignar la Burbuja o el Texto en el NPCReactionController");
            yield break;
        }

        // 1. Poner el texto
        dialogueText.text = message;

        // 2. Mostrar la burbuja
        dialogueBubble.SetActive(true);

        // 3. Esperar
        yield return new WaitForSeconds(displayTime);

        // 4. Ocultar la burbuja
        dialogueBubble.SetActive(false);
    }
}