using UnityEngine;
using UnityEngine.SceneManagement; // Necesario para cambiar de escena

public class MainMenuManager : MonoBehaviour
{
    [Header("Configuración")]
    [Tooltip("El nombre exacto de tu escena de juego")]
    public string nombreEscenaJuego = "GameScene";

    public void StartGame()
    {
        Debug.Log("Cargando el juego...");
        SceneManager.LoadScene(nombreEscenaJuego);
    }

    public void QuitGame()
    {
        Debug.Log("Saliendo de la aplicación...");
        Application.Quit();
    }
}