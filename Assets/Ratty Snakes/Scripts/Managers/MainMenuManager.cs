using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenuManager : MonoBehaviour
{
    [Header("Nombres de Escenas")]
    public string nombreEscenaJuego = "GameScene";
    public string nombreEscenaTutorial = "TutorialScene";

    [Header("Paneles UI")]
    public GameObject mainPanel;    // El menú principal (Jugar / Opciones / Salir)
    public GameObject optionsPanel; // El menú de opciones (Sliders / Volver / Reset)

    void Start()
    {
        // --- LÍNEA DE PRUEBAS (BORRAR AL TERMINAR EL DESARROLLO) ---
        // Esto fuerza a que el juego olvide que ya jugaste cada vez que inicias el menú.
        PlayerPrefs.DeleteKey("TutorialCompletado");
        // -----------------------------------------------------------

        // Al empezar, nos aseguramos de ver el menú principal y no las opciones
        VolverAlMenuPrincipal();
    }

    // ---------------------------------------------------------
    // LÓGICA DE JUEGO (EL BOTÓN "JUGAR")
    // ---------------------------------------------------------

    // Conecta esto al botón "JUGAR" / "EMPEZAR" del MainPanel
    public void BotonJugarPulsado()
    {
        // Preguntamos a la memoria: ¿Ha completado el tutorial? (1=Sí, 0=No)
        if (PlayerPrefs.GetInt("TutorialCompletado", 0) == 1)
        {
            // Ya lo ha jugado -> Cargamos el JUEGO directamente
            Debug.Log("Tutorial ya completado. Cargando Juego...");
            SceneManager.LoadScene(nombreEscenaJuego);
        }
        else
        {
            // Es la primera vez (o vale 0) -> Cargamos el TUTORIAL
            Debug.Log("Primera vez. Cargando Tutorial...");
            SceneManager.LoadScene(nombreEscenaTutorial);
        }
    }

    // ---------------------------------------------------------
    // NAVEGACIÓN DE MENÚS (PANEL OPCIONES)
    // ---------------------------------------------------------

    // Conecta esto al botón "OPCIONES"
    public void AbrirOpciones()
    {
        mainPanel.SetActive(false);
        optionsPanel.SetActive(true);
    }

    // Conecta esto al botón "VOLVER" (dentro de Opciones)
    public void VolverAlMenuPrincipal()
    {
        optionsPanel.SetActive(false);
        mainPanel.SetActive(true);
    }

    // ---------------------------------------------------------
    // FUNCIONES EXTRA (SALIR / REPETIR / RESET)
    // ---------------------------------------------------------

    // Conecta esto al botón "SALIR"
    public void QuitGame()
    {
        Debug.Log("Saliendo del juego...");
        Application.Quit();
    }

    // Opcional: Pon esto en un botón dentro de Opciones por si alguien quiere repetir el tutorial
    public void ForzarTutorial()
    {
        SceneManager.LoadScene(nombreEscenaTutorial);
    }

    // Opcional: Para tus pruebas (o un botón de "Borrar Partida" en opciones)
    public void ResetearProgreso()
    {
        PlayerPrefs.DeleteKey("TutorialCompletado");
        PlayerPrefs.Save();
        Debug.Log("Memoria borrada. El juego ahora cree que eres nuevo.");
    }
}