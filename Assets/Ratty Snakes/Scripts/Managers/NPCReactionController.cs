using UnityEngine;
using TMPro;
using System.Collections;

public class NPCReactionController : MonoBehaviour
{
    [Header("Configuración de la Burbuja")]
    public GameObject dialogueBubble;
    public TextMeshProUGUI dialogueText;
    public float displayTime = 3f;

    [Header("Efecto Máquina de Escribir")]
    public float typingSpeed = 0.05f;

    [Header("Configuración de Audio (Sistema)")]
    public AudioSource audioSource;
    public AudioClip sonidoVoz; // El sonido "blip" genérico

    [Tooltip("Variación aleatoria para que parezca que habla y no sea un robot")]
    public float variacionTono = 0.1f;

    // Datos del NPC actual
    private NPCData currentData;
    private Coroutine activeCoroutine;

    // Variable interna para guardar el tono de este NPC concreto
    private float tonoActualNPC = 1f;

    void Start()
    {
        if (dialogueBubble != null)
            dialogueBubble.SetActive(false);

        if (audioSource == null) audioSource = GetComponent<AudioSource>();
    }

    // --- AQUÍ RECIBIMOS LOS DATOS ---
    public void Initialize(NPCData data)
    {
        currentData = data;

        // Leemos el tono de voz del Scriptable Object
        if (currentData != null)
        {
            tonoActualNPC = currentData.tonoVoz;
        }
        else
        {
            tonoActualNPC = 1f; // Valor por defecto si no hay datos
        }
    }

    // --- MÉTODOS DE REACCIÓN ---

    public void MostrarFraseEntrada()
    {
        if (currentData != null && !string.IsNullOrEmpty(currentData.fraseEntrada))
        {
            StartReaction(currentData.fraseEntrada);
        }
    }

    public void ShowPositiveReaction()
    {
        if (currentData != null) StartReaction(currentData.reaccionPositiva);
    }

    public void ShowNegativeReaction()
    {
        if (currentData != null) StartReaction(currentData.reaccionNegativa);
    }

    public void MostrarQueja(string textoQueja)
    {
        StartReaction(textoQueja);
    }

    // --- LÓGICA INTERNA ---

    private void StartReaction(string message)
    {
        if (activeCoroutine != null) StopCoroutine(activeCoroutine);
        activeCoroutine = StartCoroutine(TypewriterRoutine(message));
    }

    private IEnumerator TypewriterRoutine(string message)
    {
        if (dialogueBubble == null || dialogueText == null) yield break;

        // 1. Limpiar y Mostrar
        dialogueText.text = "";
        dialogueBubble.SetActive(true);

        // 2. Bucle de escritura
        foreach (char letter in message.ToCharArray())
        {
            dialogueText.text += letter;

            // --- LÓGICA DE SONIDO ---
            if (!char.IsWhiteSpace(letter) && audioSource != null && sonidoVoz != null)
            {
                // Usamos el tono del NPC + la variación aleatoria
                float tonoFinal = tonoActualNPC + Random.Range(-variacionTono, variacionTono);

                audioSource.pitch = tonoFinal;
                audioSource.PlayOneShot(sonidoVoz);
            }
            // ------------------------

            yield return new WaitForSeconds(typingSpeed);
        }

        // 3. Esperar
        yield return new WaitForSeconds(displayTime);

        // 4. Ocultar
        dialogueBubble.SetActive(false);
        activeCoroutine = null;
    }
}