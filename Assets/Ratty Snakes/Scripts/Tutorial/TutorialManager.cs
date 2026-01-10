using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using static UnityEngine.Rendering.STP;

public class TutorialManager : MonoBehaviour
{
    public static TutorialManager Instance;

    [Header("Referencias")]
    public GodPhoneController telefono;
    public NPCManager npcManager;

    [Header("NPCs de Prueba")]
    public NPCData benitoBueno; // Asigna aquí el SO de David
    public NPCData jesusMalo;   // Asigna aquí el SO del Falso Jesús

    [Header("Configuración")]
    public string nombreEscenaJuego = "GameScene";

    // Variables de control de flujo
    private bool telefonoDescolgado = false;
    private bool botonPulsado = false;
    private bool decisionTomada = false;

    // Control de audios/broncas
    private bool reproduciendoBronca = false;
    private string fraseBroncaActual = "¡No! ¡Eso no es lo que te he dicho!"; // Frase por defecto

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

    public void AlPulsarBotonFisico()
    {
        botonPulsado = true;
    }

    // --- EVENTOS INTERNOS ---

    void AlRecibirDecisionNPC(bool fueAlCielo)
    {
        decisionTomada = true;
    }

    void AlIntentarAccionProhibida()
    {
        // Si el jugador intenta hacer lo contrario a lo permitido
        if (!reproduciendoBronca)
        {
            StartCoroutine(RutinaBronca());
        }
    }

    IEnumerator RutinaBronca()
    {
        reproduciendoBronca = true;
        // Reproduce la bronca específica del momento actual
        telefono.ReproducirFraseDios(fraseBroncaActual);
        yield return new WaitForSeconds(3f);
        reproduciendoBronca = false;
    }

    // --- FLUJO PRINCIPAL DEL TUTORIAL ---

    IEnumerator RutinaTutorial()
    {
        // =================================================================
        // FASE 1: INTRODUCCIÓN
        // =================================================================

        yield return new WaitForSeconds(1f);
        telefono.EmpezarA_Sonar();

        yield return new WaitUntil(() => telefonoDescolgado);
        yield return new WaitForSeconds(0.5f);

        yield return Hablar("¿Hola? Aquí el Jefe. Sí, Dios. El de la barba no, el original.");
        yield return Hablar("Escucha, he tenido que despedir a San Pedro.");
        yield return Hablar("El muy borracho no dejaba de beber 'agua' en el trabajo.");
        yield return Hablar("Ahora estás al cargo. No lo estropees.");
        yield return Hablar("Te encargarás de decidir quién se merece entrar al cielo y quien no.");

        yield return Hablar("Empecemos tu entrenamiento.");
        botonPulsado = false; // Reset botón
        yield return Hablar("Pulsa el BOTÓN rojo de la mesa para llamar al primer difunto de la fila.");

        yield return new WaitUntil(() => botonPulsado);


        // =================================================================
        // FASE 2: BENITO (DAVID) - EL BUENO
        // =================================================================

        // REGLA: Prohibido rechazar (Infierno bloqueado)
        npcManager.SetRestriccionesTutorial(false, true);
        // Configuramos la bronca específica por si el jugador intenta rechazarlo
        fraseBroncaActual = "¡No! ¡David es bueno! ¡Lee los papeles!";

        npcManager.SpawnNPC_Tutorial(benitoBueno);
        yield return new WaitForSeconds(1f);

        decisionTomada = false; // Reset decisión

        yield return Hablar("Vaya... este es David. Murió de... ¿ternura?");
        yield return Hablar("Madre mía. Mira su ficha en la pantalla.");
        yield return Hablar("Adoptó perros de tres patas y donó su pensión. Un blando, pero buena gente.");
        yield return Hablar("A este lo queremos.");
        yield return Hablar("Para aceptarlo al cielo, haz el gesto de PULGAR ARRIBA.");

        // Esperamos a que el jugador acierte. Si falla, saltará la bronca automática.
        yield return new WaitUntil(() => decisionTomada);

        yield return Hablar("¡Exacto! Bienvenido al Paraíso, David.");
        yield return new WaitForSeconds(1f);


        // =================================================================
        // FASE 3: EL IMPOSTOR (JESÚS FAKE) - EL MALO
        // =================================================================

        yield return Hablar("Bien hecho. Fácil, ¿eh? Vamos con el siguiente.");

        botonPulsado = false; // Reset botón
        yield return Hablar("Dale al botón otra vez para llamar al siguiente.");

        yield return new WaitUntil(() => botonPulsado);

        // REGLA: Prohibido aceptar (Cielo bloqueado)
        npcManager.SetRestriccionesTutorial(true, false);
        // Configuramos la bronca específica por si intenta aceptarlo
        fraseBroncaActual = "¿Estás loco? ¡Es un impostor! ¡Pulgar abajo!";

        npcManager.SpawnNPC_Tutorial(jesusMalo);
        yield return new WaitForSeconds(1f);

        decisionTomada = false; // Reset decisión

        yield return Hablar("¡Ay, no! ¡Otra vez este pesado!");
        yield return Hablar("Se hace llamar 'Jesús'. Dice que es mi hijo. ¡Ja!");
        yield return Hablar("Dice que multiplica panes, pero los roba del Mercadona.");
        yield return Hablar("Es un estafador.");
        yield return Hablar("A este no lo quiero ni ver. Haz el gesto de PULGAR ABAJO.");

        // Esperamos a que la palanca se desbloquee (significa que hizo el gesto bien)
        yield return new WaitUntil(() => !npcManager.sistemaPalanca.IsLocked);

        yield return Hablar("¡Bien hecho! ¿Ves esa palanca a tu derecha?");
        yield return Hablar("Ahora se ha desbloqueado. TIRA DE ELLA y mándalo al Infierno.");

        // Esperamos a que baje la palanca físicamente
        yield return new WaitUntil(() => decisionTomada);


        // =================================================================
        // FASE 4: OVERBOOKING Y CIERRE
        // =================================================================

        yield return Hablar("Perfecto. Que se pudra ese impostor.");

        yield return Hablar("Escucha, novato. El trabajo real no es tan fácil.");
        yield return Hablar("Tenemos un problema de overbooking. El cielo está a reventar.");
        yield return Hablar("No caben todos. Tendrás que elegir quién entra y quién se queda fuera.");

        yield return Hablar("Cada mañana te llamaré y te daré un LÍMITE DIARIO de difuntos que pueden ser aceptados.");
        yield return Hablar("Si dejas entrar a más gente de la cuenta... tú y yo tendremos problemas.");

        // --- NUEVO: Explicación Calidad (Sin decir Karma explícitamente) ---
        yield return Hablar("Y ojo: no metas a cualquiera para llenar el cupo. No quiero basura en mi cielo.");
        yield return Hablar("Léete bien sus fichas. Asegúrate de que sus actos buenos compensen los malos.");
        yield return Hablar("El cupo es limitado y el destino es ciego. Quizás el siguiente sea un santo... o quizás sea peor que este.");
        yield return Hablar("Tendrás que confiar en tu instinto, porque no hay vuelta atrás.");
       

        yield return Hablar("Al final del día revisaré tu trabajo personalmente.");
        yield return Hablar("Si el balance es positivo, tienes el puesto. Si no... estás despedido.");

        yield return Hablar("Venga, cuelga el teléfono. Tu turno empieza... ¡YA!");

        // 1. Esperamos a que termine de decir la frase
        yield return new WaitForSeconds(1f);

        // 2. LÓGICA DE FINALIZACIÓN

        // CASO A: El jugador ya lo había colgado mientras Dios hablaba (Impaciente)
        if (telefono.estaColgado)
        {
            Debug.Log("El jugador ya había colgado. Esperando un momento dramático...");
            yield return new WaitForSeconds(3f); // Pausa para asimilar
        }
        // CASO B: El jugador lo tiene en la mano o está en el suelo
        else
        {
            Debug.Log("Esperando a que el jugador cuelgue...");
            // El tutorial se PAUSA aquí hasta que la variable cambie a true
            yield return new WaitUntil(() => telefono.estaColgado);

            // Una vez colgado, damos un respiro de 1 segundo (el "Clack")
            yield return new WaitForSeconds(1f);
        }

        // --- GUARDADO Y CAMBIO DE ESCENA ---
        Debug.Log("Tutorial completado. Guardando y saliendo.");

        PlayerPrefs.SetInt("TutorialCompletado", 1);
        PlayerPrefs.Save();

        // Opción: Si tienes un fader, úsalo aquí. Si no, carga directa.
        SceneManager.LoadScene(nombreEscenaJuego);
    }

        IEnumerator Hablar(string texto)
    {
        telefono.ReproducirFraseDios(texto);
        // Esperamos un tiempo base (2s) + un poco extra según lo largo que sea el texto
        yield return new WaitForSeconds(2f + texto.Length * 0.06f);
    }
}