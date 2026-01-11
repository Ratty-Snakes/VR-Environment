using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class TutorialGestureUI : MonoBehaviour
{
    [Header("Grupo ACEPTAR (Gestos)")]
    public GameObject[] gestosAceptar;

    [Header("Grupo RECHAZAR (Gestos)")]
    public GameObject[] gestosRechazar;

    // --- NUEVAS VARIABLES PARA LOS TÍTULOS ---
    [Header("Títulos (Textos fijos)")]
    public GameObject[] titulosAceptar;  // Arrastra aquí los 2 textos de "Aceptar"
    public GameObject[] titulosRechazar; // Arrastra aquí los 2 textos de "Rechazar"
    // -----------------------------------------

    void Awake()
    {
        OcultarTodo();
    }

    // --- FUNCIONES PÚBLICAS ---

    public void MostrarModoAceptar()
    {
        // 1. Gestiones los Gestos (Lógica on, gráficos on/off)
        AlternarVisualesGestos(gestosAceptar, true);
        AlternarVisualesGestos(gestosRechazar, false);

        // 2. Gestionamos los Títulos (Apagar/Encender objeto entero)
        AlternarObjetosCompletos(titulosAceptar, true);
        AlternarObjetosCompletos(titulosRechazar, false);
    }

    public void MostrarModoRechazar()
    {
        // 1. Gestos
        AlternarVisualesGestos(gestosAceptar, false);
        AlternarVisualesGestos(gestosRechazar, true);

        // 2. Títulos
        AlternarObjetosCompletos(titulosAceptar, false);
        AlternarObjetosCompletos(titulosRechazar, true);
    }

    public void OcultarTodo()
    {
        // Gestos
        AlternarVisualesGestos(gestosAceptar, false);
        AlternarVisualesGestos(gestosRechazar, false);

        // Títulos
        AlternarObjetosCompletos(titulosAceptar, false);
        AlternarObjetosCompletos(titulosRechazar, false);
    }

    // --- LÓGICA INTERNA ---

    // A. Para los GESTOS: Solo apaga la imagen/texto, pero deja el padre ACTIVO (para que detecte la mano)
    void AlternarVisualesGestos(GameObject[] listaGestos, bool estado)
    {
        foreach (GameObject gestoPadre in listaGestos)
        {
            if (gestoPadre != null)
            {
                // Imágenes
                Image[] imagenes = gestoPadre.GetComponentsInChildren<Image>(true);
                foreach (Image img in imagenes) img.enabled = estado;

                // Textos
                TextMeshProUGUI[] textos = gestoPadre.GetComponentsInChildren<TextMeshProUGUI>(true);
                foreach (TextMeshProUGUI txt in textos) txt.enabled = estado;
            }
        }
    }

    // B. Para los TÍTULOS: Apaga el objeto entero (SetActive) porque no tienen lógica oculta
    void AlternarObjetosCompletos(GameObject[] listaObjetos, bool estado)
    {
        if (listaObjetos == null) return;

        foreach (GameObject obj in listaObjetos)
        {
            if (obj != null)
            {
                obj.SetActive(estado);
            }
        }
    }
}