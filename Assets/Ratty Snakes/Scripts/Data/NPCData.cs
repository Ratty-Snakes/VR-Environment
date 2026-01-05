using UnityEngine;

[CreateAssetMenu(fileName = "New NPC Data", menuName = "NPC/NPC Data")]
public class NPCData : ScriptableObject
{
    [Header("Identidad")]
    public string nombre;
    public int edad;
    [TextArea(2, 5)] public string hobbies;
    public GameObject modeloEspecifico; 

    [Header("Juicio")]
    [TextArea(2, 5)] public string causaMuerte;
    [TextArea(3, 10)] public string actosBuenos;
    [TextArea(3, 10)] public string actosMalos;

    // --- ESTO ES LO QUE TE FALTA ---
    [Header("Sistema de Karma")] 
    [Tooltip("Puntos positivos suman, negativos restan (-20 a +20)")]
    [Range(-20, 20)] 
    public int karmaScore; 
    // ------------------------------

    [Header("Reacciones")]
    [TextArea(2, 5)] public string reaccionPositiva; 
    [TextArea(2, 5)] public string reaccionNegativa; 
}