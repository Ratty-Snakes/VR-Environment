using UnityEngine;
using System.Collections;
using UnityEngine.SceneManagement;

public class TutorialManager : MonoBehaviour
{
    public static TutorialManager Instance;

    [Header("Referencias")]
    public GodPhoneController telefono;
    public NPCManager npcManager;

    [Header("NPCs de Prueba")]
    public NPCData benitoBueno;
    public NPCData jesusMalo;

    [Header("Configuración")]
    public string nombreEscenaJuego = "GameScene";

    // Variables de control de flujo
    private bool telefonoDescolgado = false;
    private bool botonPulsado = false;
    private bool decisionTomada = false;
    private bool decisionFueCielo = false;

    // Variable para evitar que la bronca suene 5 veces seguidas
    private bool reproduciendoBronca = false;

    private void Awake()
    {
        if (Instance == null) Instance = this;
    }

    void Start()
    {
        // Nos suscribimos a los eventos del NPCManager
        if (npcManager != null)
        {
            npcManager.OnDecisionTutorial += AlRecibirDecisionNPC;
            npcManager.OnIntentoProhibido += AlIntentarAccionProhibida;
        }

        StartCoroutine(RutinaTutorial());
    }

    void OnDestroy()
    {
        // Nos desuscribimos para evitar errores de memoria
        if (npcManager != null)
        {
            npcManager.OnDecisionTutorial -= AlRecibirDecisionNPC;
            npcManager.OnIntentoProhibido -= AlIntentarAccionProhibida;
        }
    }

    // --- MÉTODOS PÚBLICOS (Conectados a eventos físicos) ---

    public void JugadorContestoTelefono()
    {
        telefonoDescolgado = true;
    }

    // IMPORTANTE: Conecta esto al evento del botón físico en la escena del Tutorial
    public void AlPulsarBotonFisico()
    {
        botonPulsado = true;
    }

    // --- EVENTOS INTERNOS ---

    void AlRecibirDecisionNPC(bool fueAlCielo)
    {
        decisionTomada = true;
        decisionFueCielo = fueAlCielo;
    }

    void AlIntentarAccionProhibida()
    {
        // Si el jugador hace lo contrario a lo que debe (ej: rechazar a Benito)
        if (!reproduciendoBronca)
        {
            StartCoroutine(RutinaBronca());
        }
    }

    IEnumerator RutinaBronca()
    {
        reproduciendoBronca = true;
        // Mensaje de error sin pausar el flujo principal
        telefono.ReproducirFraseDios("¡No! ¡Lee los papeles! Estas haciendo lo contrario.");
        yield return new WaitForSeconds(3f);
        reproduciendoBronca = false;
    }

    // --- FLUJO PRINCIPAL DEL TUTORIAL ---

    IEnumerator RutinaTutorial()
    {
        // 1. INTRO
        yield return new WaitForSeconds(1f);
        telefono.EmpezarA_Sonar();

        yield return new WaitUntil(() => telefonoDescolgado);
        yield return new WaitForSeconds(0.5f);

        yield return Hablar("Bienvenido. Soy el Jefe.");

        // PROTECCIÓN SPEEDRUNNER: Reseteamos antes de hablar
        botonPulsado = false;
        yield return Hablar("Pulsa el BOTON rojo para empezar.");

        yield return new WaitUntil(() => botonPulsado);

        // --- FASE 1: BENITO (EL BUENO) ---

        // REGLA: Prohibido rechazar (Infierno bloqueado)
        npcManager.SetRestriccionesTutorial(false, true);

        npcManager.SpawnNPC_Tutorial(benitoBueno);
        yield return new WaitForSeconds(1f);

        decisionTomada = false; // Reset antes de instruir

        yield return Hablar("Este es Benito. Es buena gente.");
        yield return Hablar("Mírale y haz el gesto de PULGAR ARRIBA para salvarlo.");

        // Esperamos a que el jugador acierte. Si falla, saltará la bronca y no avanzará.
        yield return new WaitUntil(() => decisionTomada);

        yield return Hablar("Bien hecho. Al siguiente.");

        // --- FASE 2: JESÚS (EL MALO) ---

        botonPulsado = false; // Reset
        yield return Hablar("Dale al boton otra vez.");

        yield return new WaitUntil(() => botonPulsado);

        // REGLA: Prohibido aceptar (Cielo bloqueado)
        npcManager.SetRestriccionesTutorial(true, false);

        npcManager.SpawnNPC_Tutorial(jesusMalo);
        yield return new WaitForSeconds(1f);

        decisionTomada = false; // Reset

        yield return Hablar("Este es un desastre. Hay que echarlo.");
        yield return Hablar("Haz el gesto de PULGAR ABAJO para sentenciarlo.");

        // Esperamos a que la palanca se desbloquee (significa que hizo el gesto bien)
        yield return new WaitUntil(() => !npcManager.sistemaPalanca.IsLocked);

        yield return Hablar("Bien. Ahora TIRA DE LA PALANCA.");

        // Esperamos a que baje la palanca físicamente
        yield return new WaitUntil(() => decisionTomada);

        yield return Hablar("Perfecto. Ya sabes trabajar.");

        // Limpiamos restricciones y cerramos
        npcManager.SetRestriccionesTutorial(false, false);

        yield return new WaitForSeconds(1f);
        yield return Hablar("No me falles. Te paso al turno real.");

        telefono.Colgar();
        yield return new WaitForSeconds(2f);

        // --- GUARDADO DE PROGRESO ---
        // Marcamos que el tutorial está completado (1 = True)
        PlayerPrefs.SetInt("TutorialCompletado", 1);
        PlayerPrefs.Save();
        // ---------------------------

        Debug.Log("Tutorial completado. Cargando juego...");
        SceneManager.LoadScene(nombreEscenaJuego);
    }

    IEnumerator Hablar(string texto)
    {
        telefono.ReproducirFraseDios(texto);
        // Esperamos el tiempo de la frase
        yield return new WaitForSeconds(2f + texto.Length * 0.08f);
    }
}