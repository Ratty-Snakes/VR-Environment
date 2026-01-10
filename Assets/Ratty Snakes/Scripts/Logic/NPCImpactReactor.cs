using UnityEngine;
using System.Collections;

[RequireComponent(typeof(Collider))]
[RequireComponent(typeof(AudioSource))]
public class NPCImpactReactor : MonoBehaviour
{
    [Header("Configuración")]
    // Ya no es obligatorio asignarlo en el inspector, lo haremos por código
    public Transform modeloCabeza;
    public AudioClip sonidoOof;
    public float fuerzaGolpeMinima = 1f;

    [Header("Animación Impacto")]
    public float anguloRetroceso = -45f;
    public float velocidadRebote = 10f;

    private bool estaSiendoGolpeado = false;
    private AudioSource audioSource;
    private NPCReactionController reacciones;
    private NPCWaypointMovement movimiento; // Para chequear si está en WP1

    void Awake() // Cambiamos Start a Awake para inicializar referencias propias antes
    {
        audioSource = GetComponent<AudioSource>();
        audioSource.spatialBlend = 1f;
        audioSource.playOnAwake = false;

        reacciones = GetComponent<NPCReactionController>();
        movimiento = GetComponent<NPCWaypointMovement>();
    }

    // --- NUEVA FUNCIÓN PÚBLICA ---
    // Esta función la llamará el Manager cuando nazca el NPC
    public void ConfigurarCabeza(Transform objetoVisual)
    {
        modeloCabeza = objetoVisual;
    }

    void OnCollisionEnter(Collision collision)
    {
        // Seguridad: Si no nos han pasado la cabeza todavía, no hacemos nada
        if (modeloCabeza == null) return;

        // Seguridad: Solo reaccionar si estamos en el WP1 (Mesa)
        if (movimiento != null && !movimiento.esperandoDecision) return;

        if (collision.relativeVelocity.magnitude < fuerzaGolpeMinima) return;

        if (!estaSiendoGolpeado)
        {
            StartCoroutine(RutinaGolpe());
        }
    }

    IEnumerator RutinaGolpe()
    {
        estaSiendoGolpeado = true;

        // Sonido
        if (audioSource && sonidoOof)
        {
            audioSource.pitch = Random.Range(0.8f, 1.2f);
            audioSource.PlayOneShot(sonidoOof);
        }

        // Queja (Texto)
        if (reacciones != null && NPCManager.Instance != null)
        {
            string frase = NPCManager.Instance.GetQuejaRandom();
            reacciones.MostrarQueja(frase);
        }

        // Animación Cabeza
        if (modeloCabeza != null)
        {
            Quaternion rotOriginal = modeloCabeza.localRotation;
            // Usamos localRotation para que funcione aunque el NPC esté girado
            Quaternion rotGolpe = rotOriginal * Quaternion.Euler(anguloRetroceso, 0, 0);

            float t = 0;
            // Ida
            while (t < 1f)
            {
                t += Time.deltaTime * velocidadRebote;
                modeloCabeza.localRotation = Quaternion.Lerp(rotOriginal, rotGolpe, t);
                yield return null;
            }
            // Vuelta
            t = 0;
            while (t < 1f)
            {
                t += Time.deltaTime * (velocidadRebote * 0.5f);
                modeloCabeza.localRotation = Quaternion.Lerp(rotGolpe, rotOriginal, t);
                yield return null;
            }
            modeloCabeza.localRotation = rotOriginal;
        }

        yield return new WaitForSeconds(0.2f);
        estaSiendoGolpeado = false;
    }
}