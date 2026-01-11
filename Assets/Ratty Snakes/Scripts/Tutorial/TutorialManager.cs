using System;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class TutorialManager : MonoBehaviour
{
    public static TutorialManager Instance;

    [Header("Referencias")]
    public GodPhoneController telefono;
    public NPCManager npcManager;

    [Header("UI Gestos")]
    public TutorialGestureUI uiGestos;

    [Header("Guías Visuales (Flechas)")]
    public GameObject flechaBoton;
    public GameObject flechaPalanca;

    [Header("NPCs de Prueba")]
    public NPCData benitoBueno;
    public NPCData jesusMalo;

    [Header("Configuración")]
    public string nombreEscenaJuego = "GameScene";

    // Variables de control de flujo
    private bool telefonoDescolgado = false;
    private bool botonPulsado = false;
    private bool decisionTomada = false;

    // Control de audios/broncas
    private bool reproduciendoBronca = false;
    private string fraseBroncaActual = "¡No! ¡Eso no es lo que te he dicho!";

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

        // --- INICIALIZACIÓN DE SEGURIDAD ---
        if (flechaBoton != null) flechaBoton.SetActive(false);
        if (flechaPalanca != null) flechaPalanca.SetActive(false);

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

    // --- MÉTODOS PÚBLICOS ---

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
        if (!reproduciendoBronca)
        {
            StartCoroutine(RutinaBronca());
        }
    }

    IEnumerator RutinaBronca()
    {
        reproduciendoBronca = true;
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

        yield return Hablar("¿Hola? Aquí el Jefe. Sí, Dios.");
        yield return Hablar("Escucha, he tenido que despedir a San Pedro.");
        yield return Hablar("El muy borracho no dejaba de beber 'agua' en el trabajo.");
        yield return Hablar("Ahora tú estás al cargo.");
        yield return Hablar("Te encargarás de decidir quién se merece entrar al cielo y quien no.");

        yield return Hablar("Empecemos tu entrenamiento, becario.");
        botonPulsado = false;

        // ---> ENCENDER FLECHA BOTÓN <---
        if (flechaBoton != null) flechaBoton.SetActive(true);

        yield return Hablar("Pulsa el BOTÓN rojo de la mesa para llamar al primer difunto de la fila.");

        yield return new WaitUntil(() => botonPulsado);

        // ---> APAGAR FLECHA BOTÓN <---
        if (flechaBoton != null) flechaBoton.SetActive(false);


        // =================================================================
        // FASE 2: BENITO (DAVID) - EL BUENO
        // =================================================================

        npcManager.SetRestriccionesTutorial(false, true);
        fraseBroncaActual = "¡No! ¡David es bueno! ¡Lee los papeles!";

        npcManager.SpawnNPC_Tutorial(benitoBueno);
        yield return new WaitForSeconds(1f);

        decisionTomada = false;

        yield return Hablar("¿Quién tenemos aquí? Ajá, este es David.");
        yield return Hablar("Mira su biografía en la pantalla.");

        // --- PAUSA LECTURA ---
        // 1. Dejamos 2 segundos para leer "Mira su biografía..."
        yield return new WaitForSeconds(2f);

        // 2. Borramos el texto del teléfono para dejar espacio visual
        telefono.ReproducirFraseDios("");

        // 3. Dejamos 8 segundos de silencio para que el jugador lea la UI del NPC
        yield return new WaitForSeconds(10f);
        // ------------------------

        yield return Hablar("Adoptó 15 perritos con tres patas y donó su pensión. Murió de... ¿ternura ?");
        yield return Hablar("Un blando, pero buena gente. A este lo queremos.");

        if (uiGestos != null) uiGestos.MostrarModoAceptar();

        yield return Hablar("Para aceptarlo al cielo, haz el gesto de PULGAR ARRIBA.");

        yield return new WaitUntil(() => decisionTomada);

        if (uiGestos != null) uiGestos.OcultarTodo();

        yield return Hablar("¡Exacto! Bienvenido al Paraíso, David.");
        yield return new WaitForSeconds(1f);


        // =================================================================
        // FASE 3: EL IMPOSTOR (JESÚS FAKE) - EL MALO
        // =================================================================

        yield return Hablar("Bien hecho. Fácil, ¿eh? Vamos con el siguiente.");

        botonPulsado = false;

        // ---> ENCENDER FLECHA BOTÓN (OTRA VEZ) <---
        if (flechaBoton != null) flechaBoton.SetActive(true);

        yield return Hablar("Dale al botón otra vez para llamar al siguiente.");

        yield return new WaitUntil(() => botonPulsado);

        // ---> APAGAR FLECHA BOTÓN <---
        if (flechaBoton != null) flechaBoton.SetActive(false);

        npcManager.SetRestriccionesTutorial(true, false);
        fraseBroncaActual = "¿Estás loco? ¡Es un impostor! ¡Pulgar abajo!";

        npcManager.SpawnNPC_Tutorial(jesusMalo);
        yield return new WaitForSeconds(1f);

        decisionTomada = false;

        yield return Hablar("¡Ay, no! ¡Otra vez este pesado!");

        // --- PAUSA LECTURA ---
        yield return new WaitForSeconds(2f); // Espera breve
        telefono.ReproducirFraseDios("");    // Limpia texto
        yield return new WaitForSeconds(10f); // Tiempo para ver al NPC feo
        // ------------------------

        yield return Hablar("Se hace llamar 'Jesús'. Pero ese no es mi chico.");
        yield return Hablar("Dice que multiplica panes, pero los roba del Mercadona.");
        yield return Hablar("Es un estafador.");

        if (uiGestos != null) uiGestos.MostrarModoRechazar();

        yield return Hablar("A este no lo quiero ni ver. Haz el gesto de PULGAR ABAJO.");

        yield return new WaitUntil(() => !npcManager.sistemaPalanca.IsLocked);

        if (uiGestos != null) uiGestos.OcultarTodo();

        yield return Hablar("¡Bien hecho! ¿Ves esa palanca a tu derecha?");

        // ---> ENCENDER FLECHA PALANCA <---
        if (flechaPalanca != null) flechaPalanca.SetActive(true);

        yield return Hablar("TIRA DE LA PALANCA y mándalo al Infierno.");

        yield return new WaitUntil(() => decisionTomada);

        // ---> APAGAR FLECHA PALANCA <---
        if (flechaPalanca != null) flechaPalanca.SetActive(false);


        // =================================================================
        // FASE 4: OVERBOOKING Y CIERRE
        // =================================================================

        yield return Hablar("Perfecto. Que se pudra ese impostor.");

        yield return Hablar("Escucha, novato. El trabajo real no será tan fácil.");
        yield return Hablar("Tenemos un problema de overbooking. El cielo está a reventar.");
        yield return Hablar("No caben todos. Tendrás que elegir quién entra y quién se queda fuera.");

        yield return Hablar("Cada mañana te llamaré y te daré un LÍMITE DIARIO de difuntos que pueden ser aceptados.");
        yield return Hablar("Si dejas entrar a más gente de la cuenta... tú y yo tendremos problemas.");

        yield return Hablar("Y ojo: no metas a cualquiera para llenar el cupo. No quiero basura en mi cielo.");
        yield return Hablar("Léete bien sus fichas. Asegúrate de que sus actos buenos compensen los malos.");

        yield return Hablar("Al final del día revisaré tu trabajo personalmente.");
        yield return Hablar("Si el balance es positivo, tienes el puesto. Si no... estás despedido.");

        yield return Hablar("Venga, cuelga el teléfono. Tu turno empieza... ¡YA!");

        yield return new WaitForSeconds(1f);

        if (telefono.estaColgado)
        {
            Debug.Log("El jugador ya había colgado. Esperando un momento dramático...");
            yield return new WaitForSeconds(2f);
        }
        else
        {
            Debug.Log("Esperando a que el jugador cuelgue...");
            yield return new WaitUntil(() => telefono.estaColgado);
            yield return new WaitForSeconds(1f);
        }

        Debug.Log("Tutorial completado. Guardando y saliendo.");
        PlayerPrefs.SetInt("TutorialCompletado", 1);
        PlayerPrefs.Save();
        SceneManager.LoadScene(nombreEscenaJuego);
    }

    IEnumerator Hablar(string texto)
    {
        telefono.ReproducirFraseDios(texto);
        yield return new WaitForSeconds(2f + texto.Length * 0.06f);
    }
}