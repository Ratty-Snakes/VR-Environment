using UnityEngine;
using TMPro;

public class BookUIController : MonoBehaviour
{
    [Header("Referencias de Textos")]
    public TextMeshProUGUI textoCupo;      // Asigna aquí el texto grande (ej: "0 / 2")
    public TextMeshProUGUI textoRestantes; // Asigna aquí el texto de "En Cola: 5"

    [Header("Colores")]
    public Color colorNormal = Color.black;
    public Color colorPeligro = Color.red; // Se pone rojo si te pasas del límite

    // Esta función la llamará el GameManager cada vez que pase algo
    public void ActualizarPagina(int aceptados, int limiteDiario, int enCola)
    {
        // 1. Actualizamos el Cupo
        if (textoCupo != null)
        {
            textoCupo.text = $"Límite: {aceptados} / {limiteDiario}";

            // Feedback visual: Si te pasas, se pone rojo
            if (aceptados > limiteDiario) textoCupo.color = colorPeligro;
            else textoCupo.color = colorNormal;
        }

        // 2. Actualizamos la Cola
        if (textoRestantes != null)
        {
            textoRestantes.text = $"En cola: {enCola}";
        }
    }
}