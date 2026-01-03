using UnityEngine;
using System.Collections;
using UnityEngine.SceneManagement;

public class TutorialManager : MonoBehaviour
{
    // ESTA ES LA VARIABLE QUE BUSCA EL TELÉFONO:
    public static TutorialManager Instance;

    [Header("Referencias")]
    public GodPhoneController telefonoDios;
    public NPCManager npcManager;

    [Header("Datos Tutorial")]
    public NPCData benitoBueno; // El Santo
    public NPCData jesusMalo;   // El Impostor

    [Header("Configuracion")]
    public string escenaJuego = "GameScene"; // Pon aquí el nombre exacto de tu escena de juego

    private int pasoTutorial = 0;

    void Awake()
    {
        // Configuración del Singleton
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    void Start()
    {
        // Paso 0: Esperar un poco y llamar
        StartCoroutine(InicioTutorial());
    }

    IEnumerator InicioTutorial()
    {
        yield return new WaitForSeconds(3f); // Silencio inicial
        telefonoDios.EmpezarA_Sonar();
    }

    // ESTA ES LA FUNCIÓN QUE LLAMA EL TELÉFONO
    public void JugadorContestoTelefono()
    {
        StartCoroutine(RutinaTutorial());
    }

    IEnumerator RutinaTutorial()
    {
        // --- FASE 1: INTRODUCCION ---
        yield return Hablar("¡Eh, tu! Si, el nuevo. Pedro se ha cogido la baja por estres eterno. Estas al mando.");
        yield return Hablar("No rompas nada. Tienes una fila de almas esperando juicio.");

        // --- FASE 2: BENITO (EL BUENO) ---
        pasoTutorial = 1;
        yield return Hablar("Vamos a probar. Mira a este tipo. Es Benito.");

        // Spawnear a Benito
        npcManager.SpawnNPC_Tutorial(benitoBueno);
        yield return new WaitForSeconds(4f);

        // [CORRECCIÓN] Nos suscribimos ANTES de hablar
        bool decisionCorrecta = false;
        bool haDecidido = false;

        System.Action<bool> verificarBenito = (enviadoAlCielo) => {
            haDecidido = true;
            decisionCorrecta = enviadoAlCielo;
        };
        npcManager.OnDecisionTutorial += verificarBenito; // <--- OREJA PUESTA YA

        yield return Hablar("Lee sus papeles. Ha salvado perritos. Es un santo. Mandalo al CIELO (Pulgar Arriba).");

        // Si el jugador fue rápido y decidió mientras Dios hablaba, esto pasará inmediatamente
        if (!haDecidido) yield return new WaitUntil(() => haDecidido);

        npcManager.OnDecisionTutorial -= verificarBenito; // Oreja fuera

        if (decisionCorrecta)
        {
            yield return Hablar("Bien hecho. ¿Ves? No es tan dificil.");
        }
        else
        {
            yield return Hablar("¡¿Pero que haces?! ¡Era un santo! En fin... primer aviso.");
        }

        yield return new WaitForSeconds(3f);

        // --- FASE 3: JESUS (EL MALO) ---
        pasoTutorial = 2;
        yield return Hablar("Siguiente. ¡Uy, mira quien viene!");

        npcManager.SpawnNPC_Tutorial(jesusMalo);
        yield return new WaitForSeconds(4f);

        // [CORRECCIÓN] Nos suscribimos ANTES de hablar
        haDecidido = false;
        System.Action<bool> verificarJesus = (enviadoAlCielo) => {
            haDecidido = true;
            decisionCorrecta = !enviadoAlCielo; // Debe ser FALSE (Infierno)
        };
        npcManager.OnDecisionTutorial += verificarJesus; // <--- OREJA PUESTA YA

        yield return Hablar("Se llama 'Jesus', pero no es MI chico. Es un impostor. Lee su ficha, es un desastre.");
        yield return Hablar("Mandalo al INFIERNO (Palanca o Pulgar Abajo). ¡Rapido!");

        if (!haDecidido) yield return new WaitUntil(() => haDecidido);

        npcManager.OnDecisionTutorial -= verificarJesus;

        // Feedback
        if (decisionCorrecta)
        {
            yield return Hablar("Perfecto. Huele a chamusquina, me encanta.");
        }
        else
        {
            // AQUI ENTRARÁ SI LO ACEPTAS (ERROR)
            yield return Hablar("¿Lo has mandado arriba? ¡Te dije que era un impostor! Me debes una.");
        }

        yield return new WaitForSeconds(3f);

        // --- FASE 4: OUTRO Y REGLAS ---
        yield return Hablar("Escucha bien la regla de oro: El Cielo tiene CUPO LIMITADO.");
        yield return Hablar("Solo caben unas pocas personas al dia. Si metes a demasiados, el sistema explota.");
        yield return Hablar("Y si metes a gente mala... bueno, ya veremos que pasa con tu contrato.");
        yield return Hablar("Venga, empieza tu turno real. Buena suerte.");

        telefonoDios.Colgar();
        yield return new WaitForSeconds(2f);

        // --- FIN TUTORIAL ---
        Debug.Log("Cargando escena de juego...");
        SceneManager.LoadScene(escenaJuego);
    }

    // Funcion auxiliar
    IEnumerator Hablar(string frase)
    {
        // Calculamos duracion
        float duracionEstimada = (frase.Length * 0.08f) + 2.0f;

        telefonoDios.ReproducirFraseDios(frase);

        yield return new WaitForSeconds(duracionEstimada);
    }
}