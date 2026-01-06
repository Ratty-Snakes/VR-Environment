using UnityEngine;
using UnityEngine.XR.Content.Interaction;

public class LeverLockSystem : MonoBehaviour
{
    [Header("Referencias")]
    public XRLever xrLever;
    public AudioSource audioSource;

    [Header("Configuración Bloqueo")]
    // Tus valores actuales: -50 y -45
    public float minAngleLocked = -50f;
    public float maxAngleLocked = -45f;

    [Header("Configuración Desbloqueo")]
    public float minAngleUnlocked = -90f; // Asegúrate que este llega más lejos que el bloqueado
    public float maxAngleUnlocked = 0f;   // Posición inicial

    [Header("Sonidos")]
    public AudioClip sonidoAtascado;
    public AudioClip sonidoDesbloqueo;

    // --- CAMBIO AQUÍ: Añadimos 'public' y 'get' para leerlo desde fuera ---
    public bool IsLocked { get; private set; } = true;

    void Start()
    {
        if (xrLever != null) xrLever.selectEntered.AddListener(OnAgarrar);
        BloquearPalanca();
    }

    void OnDestroy()
    {
        if (xrLever != null) xrLever.selectEntered.RemoveListener(OnAgarrar);
    }

    public void BloquearPalanca()
    {
        IsLocked = true; // Actualizamos la propiedad pública

        if (xrLever != null)
        {
            xrLever.minAngle = minAngleLocked;
            xrLever.maxAngle = maxAngleLocked;
            xrLever.value = false;
        }
    }

    public void DesbloquearPalanca()
    {
        if (IsLocked)
        {
            IsLocked = false;

            if (xrLever != null)
            {
                xrLever.minAngle = minAngleUnlocked;
                xrLever.maxAngle = maxAngleUnlocked;
            }

            if (audioSource != null && sonidoDesbloqueo != null)
                audioSource.PlayOneShot(sonidoDesbloqueo);
        }
    }

    private void OnAgarrar(UnityEngine.XR.Interaction.Toolkit.SelectEnterEventArgs args)
    {
        if (IsLocked)
        {
            if (audioSource != null && sonidoAtascado != null)
                audioSource.PlayOneShot(sonidoAtascado);
        }
    }
}