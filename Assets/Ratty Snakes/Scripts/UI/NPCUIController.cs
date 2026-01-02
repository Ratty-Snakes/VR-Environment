using UnityEngine;
using TMPro;

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

        // Activamos todo el canvas primero
        panelCompleto.SetActive(true);

        // Rellenamos campo a campo.
        // La función se encarga de: Poner el texto Y apagar la caja si está vacío.

        ActualizarCampo(txtNombre, datos.nombre);
        ActualizarCampo(txtEdad, datos.edad.ToString() + " años");
        ActualizarCampo(txtHobbies, "Hobbies:\n\n" + datos.hobbies);
        ActualizarCampo(txtCausaMuerte, "Causa de muerte:\n\n" + datos.causaMuerte);

        // Para historiales largos
        ActualizarCampo(txtActosBuenos, "Actos Buenos:\n\n" + datos.actosBuenos);
        ActualizarCampo(txtActosMalos, "Actos Malos:\n\n" + datos.actosMalos);

        // Forzamos a Unity a recalcular el layout inmediatamente (a veces tarda un frame)
        UnityEngine.UI.LayoutRebuilder.ForceRebuildLayoutImmediate(panelCompleto.GetComponent<RectTransform>());
    }

    public void OcultarDatos()
    {
        panelCompleto.SetActive(false);
    }

    // --- Función Mágica ---
    // Si el texto está vacío, apaga al PADRE (la caja negra) para que no ocupe espacio.
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
            // El Vertical Layout Group del abuelo reordenará todo hacia arriba.
            cajaNegraPadre.SetActive(false);
        }
    }
}