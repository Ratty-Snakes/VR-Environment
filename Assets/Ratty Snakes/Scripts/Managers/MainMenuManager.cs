using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenuManager : MonoBehaviour
{
    [Header("Escenas")]
    public string nombreEscenaJuego = "GameScene";

    [Header("Paneles UI")]
    public GameObject mainPanel;    // El menú principal (Jugar / Opciones)
    public GameObject optionsPanel; // El menú de opciones (Sliders / Volver)

    void Start()
    {
        // Asegurar estado inicial correct
        VolverAlMenuPrincipal();
    }

    public void StartGame()
    {
        SceneManager.LoadScene(nombreEscenaJuego);
    }

    public void AbrirOpciones()
    {
        mainPanel.SetActive(false);
        optionsPanel.SetActive(true);
    }

    public void VolverAlMenuPrincipal()
    {
        optionsPanel.SetActive(false);
        mainPanel.SetActive(true);
    }

    public void QuitGame()
    {
        Application.Quit();
    }
}