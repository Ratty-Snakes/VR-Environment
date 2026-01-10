using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class TutorialGestureUI : MonoBehaviour
{
    [Header("Grupo ACEPTAR (Arrastra Thumbs Up L y R)")]
    public GameObject[] gestosAceptar;

    [Header("Grupo RECHAZAR (Arrastra Thumbs Down, Middle y Index - L y R)")]
    public GameObject[] gestosRechazar;

    void Awake()
    {
        // Al empezar, ocultamos TODO para que la pantalla esté limpia
        OcultarTodo();
    }

    // --- FUNCIONES PÚBLICAS ---

    public void MostrarModoAceptar()
    {
        // Encendemos Thumbs Up
        AlternarVisuales(gestosAceptar, true);
        // Apagamos todo lo de rechazar (por si acaso)
        AlternarVisuales(gestosRechazar, false);
    }

    public void MostrarModoRechazar()
    {
        // Apagamos Thumbs Up
        AlternarVisuales(gestosAceptar, false);
        // Encendemos Thumbs Down, Dedo y Negación
        AlternarVisuales(gestosRechazar, true);
    }

    public void OcultarTodo()
    {
        AlternarVisuales(gestosAceptar, false);
        AlternarVisuales(gestosRechazar, false);
    }

    // --- LÓGICA INTERNA (Apaga gráficos, mantiene lógica) ---
    void AlternarVisuales(GameObject[] listaGestos, bool estado)
    {
        foreach (GameObject gestoPadre in listaGestos)
        {
            if (gestoPadre != null)
            {
                // 1. Buscamos todas las Imágenes en los hijos (Iconos)
                Image[] imagenes = gestoPadre.GetComponentsInChildren<Image>(true);
                foreach (Image img in imagenes) img.enabled = estado;

                // 2. Buscamos todos los Textos en los hijos
                TextMeshProUGUI[] textos = gestoPadre.GetComponentsInChildren<TextMeshProUGUI>(true);
                foreach (TextMeshProUGUI txt in textos) txt.enabled = estado;
            }
        }
    }
}