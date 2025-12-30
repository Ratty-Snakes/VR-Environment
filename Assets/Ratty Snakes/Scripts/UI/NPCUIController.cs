using UnityEngine;
using TMPro; // Necesario para TextMeshPro

public class NPCUIController : MonoBehaviour
{
    [Header("Referencias de Texto UI")]
    public GameObject panelCompleto; // El objeto padre de todo para ocultarlo/mostrarlo
    public TextMeshProUGUI txtNombre;
    public TextMeshProUGUI txtEdad;
    public TextMeshProUGUI txtHobbies;
    public TextMeshProUGUI txtCausaMuerte;
    public TextMeshProUGUI txtActosBuenos;
    public TextMeshProUGUI txtActosMalos;

    void Start()
    {
        // Al empezar el juego, la pantalla debe estar apagada
        OcultarDatos();
    }

    public void MostrarDatos(NPCData datos)
    {
        if (datos == null) return;

        // Rellenamos los textos con la info del ScriptableObject
        txtNombre.text = datos.nombre;
        txtEdad.text = datos.edad.ToString() + " años";
        txtHobbies.text = "Hobbies: " + datos.hobbies;
        txtCausaMuerte.text = "Causa: " + datos.causaMuerte;
        txtActosBuenos.text = datos.actosBuenos;
        txtActosMalos.text = datos.actosMalos;

        // Encendemos la pantalla
        panelCompleto.SetActive(true);
    }

    public void OcultarDatos()
    {
        panelCompleto.SetActive(false);
    }
}