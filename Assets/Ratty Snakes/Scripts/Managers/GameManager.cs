using UnityEngine;
using System.Collections.Generic;

public class GameManager : MonoBehaviour
{
    // Singleton: Para poder acceder a este script desde cualquier lado fácilmente
    public static GameManager Instance;

    [Header("Configuración de la Partida")]
    [Tooltip("Arrastra aquí TODOS tus archivos de NPC (los 10 posibles)")]
    public List<NPCData> poolCompletaNPCs;

    [Tooltip("Cuántos NPCs aparecerán en esta sesión")]
    public int npcsPorPartida = 5;

    [Tooltip("Límite de plazas en el cielo hoy (lo que dice Dios)")]
    public int limiteCieloDiario = 3;

    [Header("Estado Actual (Solo lectura)")]
    public int actualesEnCielo = 0;
    public int actualesRechazados = 0;

    // Cola de NPCs que saldrán en esta partida específica
    private Queue<NPCData> colaDePartida = new Queue<NPCData>();

    private void Awake()
    {
        // Configuración básica del Singleton
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    private void Start()
    {
        // Para probar, iniciamos la partida nada más empezar.
        // Más adelante, esto lo llamará el botón "Jugar" del menú.
        IniciarNuevoDia();
    }

    public void IniciarNuevoDia()
    {
        actualesEnCielo = 0;
        actualesRechazados = 0;
        colaDePartida.Clear();

        Debug.Log($"--- INICIANDO DÍA --- Límite Cielo: {limiteCieloDiario}");

        // 1. Barajar la lista completa (Shuffle)
        List<NPCData> listaBarajada = new List<NPCData>(poolCompletaNPCs);
        BarajarLista(listaBarajada);

        // 2. Coger solo los primeros 'npcsPorPartida' y meterlos en la cola
        int cantidad = Mathf.Min(npcsPorPartida, listaBarajada.Count);
        for (int i = 0; i < cantidad; i++)
        {
            colaDePartida.Enqueue(listaBarajada[i]);
        }

        Debug.Log($"Se han seleccionado {colaDePartida.Count} NPCs para hoy.");

        // Aquí más tarde llamaremos al NPCManager para que saque al primero
        // NPCManager.Instance.SacarSiguienteNPC();
    }

    // Método para obtener el siguiente de la fila
    public NPCData ObtenerSiguienteNPC()
    {
        if (colaDePartida.Count > 0)
            return colaDePartida.Dequeue();

        return null; // No quedan más
    }

    // Algoritmo simple para barajar listas (Fisher-Yates)
    void BarajarLista<T>(List<T> lista)
    {
        for (int i = 0; i < lista.Count; i++)
        {
            T temp = lista[i];
            int randomIndex = Random.Range(i, lista.Count);
            lista[i] = lista[randomIndex];
            lista[randomIndex] = temp;
        }
    }

    // Métodos para actualizar contadores
    public void RegistrarEntradaCielo()
    {
        actualesEnCielo++;
        CheckLimite();
    }

    public void RegistrarRechazo()
    {
        actualesRechazados++;
    }

    private void CheckLimite()
    {
        if (actualesEnCielo >= limiteCieloDiario)
        {
            Debug.LogWarning("¡OJO! Se ha alcanzado el límite diario del cielo.");
            // Aquí más tarde activaremos alguna alarma visual o sonora
        }
    }
}