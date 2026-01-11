using UnityEngine;
using TMPro;
using UnityEngine.UI; // <--- NECESARIO para LayoutRebuilder
using System.Collections; // <--- NECESARIO para las Corrutinas

public class NPCUIController : MonoBehaviour
{
    [Header("Control Principal")]
    public GameObject panelCompleto; // El objeto padre que oculta todo (Canvas o Contenedor)

    [Header("Referencias a los TEXTOS (Los hijos)")]
    public TextMeshProUGUI txtNombre;
    public TextMeshProUGUI txtEdad;
    public TextMeshProUGUI txtHobbies;
    public TextMeshProUGUI txtCausaMuerte;
    public TextMeshProUGUI txtActosBuenos;
    public TextMeshProUGUI txtActosMalos;

    void Start()
    {
        OcultarDatos();
    }

    public void MostrarDatos(NPCData datos)
    {
        if (datos == null) return;

        // 1. Activamos todo el canvas primero para que los cálculos funcionen
        panelCompleto.SetActive(true);

        // 2. Rellenamos campo a campo.
        ActualizarCampo(txtNombre, datos.nombre);
        ActualizarCampo(txtEdad, datos.edad.ToString() + " años");
        ActualizarCampo(txtHobbies, "Hobbies:\n\n" + datos.hobbies);
        ActualizarCampo(txtCausaMuerte, "Causa de muerte:\n\n" + datos.causaMuerte);
        ActualizarCampo(txtActosBuenos, "Actos Buenos:\n\n" + datos.actosBuenos);
        ActualizarCampo(txtActosMalos, "Actos Malos:\n\n" + datos.actosMalos);

        // 3. INICIAMOS LA REPARACIÓN DEL LAYOUT
        // En lugar de hacerlo inmediato, llamamos a la corrutina que espera un frame.
        StartCoroutine(ForzarActualizacionLayout());
    }

    public void OcultarDatos()
    {
        panelCompleto.SetActive(false);
    }

    // --- CORRUTINA PARA ARREGLAR EL "DIRTY LAYOUT" ---
    IEnumerator ForzarActualizacionLayout()
    {
        // Esperamos al final del frame. Esto da tiempo a TextMeshPro a calcular
        // cuánto espacio ocupa el texto nuevo.
        yield return new WaitForEndOfFrame();

        RectTransform rectTransformPanel = panelCompleto.GetComponent<RectTransform>();

        if (rectTransformPanel != null)
        {
            // Forzamos la reconstrucción dos veces por seguridad (a veces los layouts anidados fallan a la primera)
            LayoutRebuilder.ForceRebuildLayoutImmediate(rectTransformPanel);

            // Opcional: Si sigue fallando, descomenta la siguiente línea para esperar otro frame extra
            // yield return null; 

            LayoutRebuilder.ForceRebuildLayoutImmediate(rectTransformPanel);
        }
    }

    // --- Función Mágica ---
    void ActualizarCampo(TextMeshProUGUI campoTexto, string contenido)
    {
        if (campoTexto == null) return;

        // Obtenemos al padre (la caja negra 'Item_Data')
        GameObject cajaNegraPadre = campoTexto.transform.parent.gameObject;

        // Comprobamos si el contenido es relevante (no es null)
        bool tieneContenido = !string.IsNullOrEmpty(contenido);

        if (tieneContenido)
        {
            cajaNegraPadre.SetActive(true);
            campoTexto.text = contenido;
        }
        else
        {
            // Si está vacío, apagamos la caja entera. 
            cajaNegraPadre.SetActive(false);
        }
    }
}