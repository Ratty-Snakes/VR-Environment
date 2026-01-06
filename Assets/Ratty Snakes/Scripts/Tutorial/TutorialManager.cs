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

    private bool telefonoDescolgado = false;
    private bool botonPulsado = false;
    private bool decisionTomada = false;
    private bool decisionFueCielo = false;
    private bool reproduciendoBronca = false;

    private void Awake()
    {
        if (Instance == null) Instance = this;
    }

    void Start()
    {
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

    public void JugadorContestoTelefono() { telefonoDescolgado = true; }
    public void AlPulsarBotonFisico() { botonPulsado = true; }

    void AlRecibirDecisionNPC(bool fueAlCielo)
    {
        decisionTomada = true;
        decisionFueCielo = fueAlCielo;
    }

    void AlIntentarAccionProhibida()
    {
        if (!reproduciendoBronca) StartCoroutine(RutinaBronca());
    }

    IEnumerator RutinaBronca()
    {
        reproduciendoBronca = true;
        // Usamos una versión de Hablar que NO espera, para no romper el flujo
        telefono.ReproducirFraseDios("¡No! ¡Lee los papeles! Estas haciendo lo contrario.");
        yield return new WaitForSeconds(3f);
        reproduciendoBronca = false;
    }

    // --- FLUJO CORREGIDO ---
    IEnumerator RutinaTutorial()
    {
        yield return new WaitForSeconds(1f);
        telefono.EmpezarA_Sonar();
        yield return new WaitUntil(() => telefonoDescolgado);
        yield return new WaitForSeconds(0.5f);

        yield return Hablar("Bienvenido. Soy el Jefe.");

        // CORRECCIÓN: Reseteamos ANTES de hablar.
        // Si pulsas el botón mientras habla, se guardará el true.
        botonPulsado = false;
        yield return Hablar("Pulsa el BOTON rojo para empezar.");

        // Ahora el WaitUntil detectará si ya lo has pulsado antes
        yield return new WaitUntil(() => botonPulsado);

        // --- FASE 1: BENITO ---
        npcManager.SetRestriccionesTutorial(false, true); // Solo Aceptar permitido
        npcManager.SpawnNPC_Tutorial(benitoBueno);

        yield return new WaitForSeconds(1f);

        // CORRECCIÓN: Preparamos la variable de decisión antes de dar la chapa
        decisionTomada = false;

        yield return Hablar("Este es Benito. Es buena gente.");
        yield return Hablar("Mírale y haz PULGAR ARRIBA para salvarlo.");

        // Si ya lo has mandado al cielo mientras hablaba, esto pasará directo
        yield return new WaitUntil(() => decisionTomada);

        yield return Hablar("Bien hecho. Al siguiente.");

        // --- FASE 2: JESÚS ---

        // CORRECCIÓN: Reseteamos botón antes
        botonPulsado = false;
        yield return Hablar("Dale al boton otra vez.");

        yield return new WaitUntil(() => botonPulsado);

        npcManager.SetRestriccionesTutorial(true, false); // Solo Rechazar permitido
        npcManager.SpawnNPC_Tutorial(jesusMalo);

        yield return new WaitForSeconds(1f);

        // CORRECCIÓN: Preparamos variable antes
        decisionTomada = false;

        yield return Hablar("Este es un desastre. Hay que echarlo.");
        yield return Hablar("Haz PULGAR ABAJO y luego TIRA DE LA PALANCA.");

        // Aquí es más complejo porque son dos pasos (Gesto + Palanca).
        // El WaitUntil espera a que la decisión final (palanca) esté hecha.
        // Si eres rapidísimo y haces las dos cosas mientras habla, funcionará.
        yield return new WaitUntil(() => decisionTomada);

        yield return Hablar("Perfecto. Ya sabes trabajar.");

        npcManager.SetRestriccionesTutorial(false, false);

        yield return new WaitForSeconds(1f);
        telefono.Colgar();
        yield return new WaitForSeconds(2f);
        SceneManager.LoadScene(nombreEscenaJuego);
    }

    IEnumerator Hablar(string texto)
    {
        telefono.ReproducirFraseDios(texto);
        // Esperamos, pero si el jugador avanza rápido, el WaitUntil de fuera lo pillará
        yield return new WaitForSeconds(2f + texto.Length * 0.08f);
    }
}