using System.Collections;
using TMPro;
using UnityEngine;

public class NPCFloatingDialogue : MonoBehaviour
{
    [Header("Follow Settings")]
    public Transform targetToFollow;

    [Header("UI References")]
    public CanvasGroup canvasGroup; // panel + texto
    public TextMeshProUGUI dialogueText;

    [Header("Settings")]
    public float fadeDuration = 0.3f;
    public float typeSpeed = 0.04f;

    private Coroutine typingCoroutine;

    void Awake()
    {
        if (canvasGroup == null)
            canvasGroup = GetComponentInChildren<CanvasGroup>();

        HideInstant(); // aseguramos que no se vea al spawnear
    }

    // --------------------------
    // PUBLIC API
    // --------------------------

    public void ShowPositive(string text)
    {
        dialogueText.color = Color.green;
        Show(text);
    }

    public void ShowNegative(string text)
    {
        dialogueText.color = Color.red;
        Show(text);
    }

    public void Show(string text)
    {
        // Reactivar panel
        ShowPanel();

        // Cancelar escritura previa
        if (typingCoroutine != null)
            StopCoroutine(typingCoroutine);

        typingCoroutine = StartCoroutine(TypeEffect(text));
    }

    public void Hide()
    {
        StartCoroutine(FadeOut());
    }

    public void HideInstant()
    {
        canvasGroup.alpha = 0f;
        dialogueText.text = "";
    }

    // --------------------------
    // TYPEWRITER EFFECT
    // --------------------------

    private IEnumerator TypeEffect(string fullText)
    {
        dialogueText.text = "";

        foreach (char c in fullText)
        {
            dialogueText.text += c;
            yield return new WaitForSeconds(typeSpeed);
        }
    }

    // --------------------------
    // FADING
    // --------------------------

    private void ShowPanel()
    {
        StopAllCoroutines();
        StartCoroutine(FadeIn());
    }

    private IEnumerator FadeIn()
    {
        float t = 0;
        while (t < fadeDuration)
        {
            t += Time.deltaTime;
            canvasGroup.alpha = Mathf.Lerp(0, 1, t / fadeDuration);
            yield return null;
        }
        canvasGroup.alpha = 1f;
    }

    private IEnumerator FadeOut()
    {
        float t = 0;
        while (t < fadeDuration)
        {
            t += Time.deltaTime;
            canvasGroup.alpha = Mathf.Lerp(1, 0, t / fadeDuration);
            yield return null;
        }
        canvasGroup.alpha = 0f;
        dialogueText.text = "";
    }

    // --------------------------
    // FOLLOW HEAD
    // --------------------------

    private void Update()
    {
        if (targetToFollow != null)
            transform.position = targetToFollow.position;
    }
}
