using UnityEngine;

[CreateAssetMenu(fileName = "NuevoNPC", menuName = "Juego/Datos de NPC", order = 1)]
public class NPCData : ScriptableObject
{
    [Header("Datos Personales")]
    public string nombre = "Marina";
    public int edad = 27;
    [TextArea(2, 5)] public string hobbies = "Ahogar moscas con agua";
    [TextArea(2, 5)] public string causaMuerte = "Murió por un ictus sola en su casa...";

    [Header("Historial")]
    [TextArea(3, 10)] public string actosBuenos = "Evitó que un niño se ahogase...";
    [TextArea(3, 10)] public string actosMalos = "No respetar el espacio personal...";

    [Header("Reacciones (Sistema de Juego)")]
    // Mantenemos esto porque el juego necesita saber qué dicen al aceptar/rechazar
    [TextArea(2, 3)] public string reaccionPositiva = "¡Gracias! Has sido muy amable.";
    [TextArea(2, 3)] public string reaccionNegativa = "¡No es justo! ¡Yo merecía entrar!";

    [Header("Visual")]
    public GameObject modeloEspecifico; // Opcional, por si cada NPC tiene una calavera distinta
}