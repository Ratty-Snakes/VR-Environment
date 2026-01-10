using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    [Header("Referencias Escena")]
    public GodPhoneController telefonoDios;
    public BookUIController libroRegistro;
    public NPCManager npcManager;

    [Header("Base de Datos")]
    public List<NPCData> poolGlobalNpcs;

    [Header("Configuracion del Dia")]
    public int minClientes = 3;
    public int maxClientes = 6;
    [Range(0.1f, 1f)]
    public float porcentajeCupo = 0.5f;
    public string nombreEscenaMenu = "MainMenu";

    // Variables de Estado
    private List<NPCData> listaNpcsHoy = new List<NPCData>();
    private int cupoDiario;
    private int almasAceptadas = 0;
    private int karmaAcumulado = 0;
    private int indiceActual = 0;
    private bool botonDesbloqueado = false;

    private void Awake()
    {
        if (Instance == null) Instance = this;
    }

    void Start()
    {
        GenerarDiaAleatorio();
        // Actualizamos el libro al inicio en silencio para que tenga datos
        ActualizarLibro();
        StartCoroutine(RutinaInicioDia());
    }

    void GenerarDiaAleatorio()
    {
        if (poolGlobalNpcs == null || poolGlobalNpcs.Count == 0)
        {
            Debug.LogError("ERROR: Pool vacio.");
            return;
        }

        int maxPosible = Mathf.Min(maxClientes, poolGlobalNpcs.Count);
        int cantidadHoy = Random.Range(minClientes, maxPosible + 1);

        List<NPCData> copiaPool = new List<NPCData>(poolGlobalNpcs);
        listaNpcsHoy.Clear();

        for (int i = 0; i < cantidadHoy; i++)
        {
            int indexAleatorio = Random.Range(0, copiaPool.Count);
            listaNpcsHoy.Add(copiaPool[indexAleatorio]);
            copiaPool.RemoveAt(indexAleatorio);
        }

        cupoDiario = Mathf.RoundToInt(listaNpcsHoy.Count * porcentajeCupo);
        if (cupoDiario < 1 && listaNpcsHoy.Count > 0) cupoDiario = 1;
    }

    IEnumerator RutinaInicioDia()
    {
        botonDesbloqueado = false;
        yield return new WaitForSeconds(2f);

        telefonoDios.EmpezarA_Sonar();

        // Esperamos a que el jugador descuelgue
        yield return new WaitUntil(() => !telefonoDios.audioSource.loop);
        yield return new WaitForSeconds(0.5f);

        int total = listaNpcsHoy.Count;

        if (total == 0)
        {
            // Caso raro: No hay nadie (Día libre)
            yield return Hablar("¿Sabes qué? Hoy no ha muerto nadie. Tómate el día libre.");
            yield return Hablar("Vuelve a casa.");
            SceneManager.LoadScene(nombreEscenaMenu);
        }
        else
        {
            // --- NUEVO DIÁLOGO CON PERSONALIDAD ---

            // 1. El despiste
            yield return Hablar("Soy yo otra vez. Casi se me olvida darte los números de hoy...");

            // 2. El Límite (La restricción)
            yield return Hablar("A ver, hoy andamos cortos de espacio. Las nubes están a reventar.");
            yield return Hablar("El LÍMITE DE HOY es de " + cupoDiario + " personas. Ni una más.");

            // 3. La Cola (La presión)
            yield return Hablar("Y no te duermas, porque tienes a " + total + " almas esperando ahí en la cola.");

            // 4. El Libro (La herramienta)
            ActualizarLibro(); // Nos aseguramos de que esté actualizado visualmente aquí
            yield return Hablar("Por cierto, si pierdes la cuenta o quieres revisar cuántos difuntos faltan");
            yield return Hablar("Consulta el Libro de Registro que tienes en la mesa.");
            yield return Hablar("Ahí se apunta todo automáticamente. Úsalo.");

            // 5. Despedida
            yield return Hablar("Eso es todo. Suerte");
        }

        yield return new WaitForSeconds(1f);

        // Esperamos a que cuelgue para desbloquear el botón (opcional, pero queda mejor)
        // O simplemente colgamos nosotros si tarda mucho
        telefonoDios.Colgar();

        if (total > 0) botonDesbloqueado = true;
    }

    // --- LOGICA DE JUEGO (Igual que antes) ---

    public void IntentarTraerSiguiente()
    {
        if (!botonDesbloqueado) return;
        npcManager.BotonSiguientePulsado();
    }

    public NPCData ObtenerSiguienteNPC()
    {
        if (indiceActual < listaNpcsHoy.Count)
        {
            NPCData data = listaNpcsHoy[indiceActual];
            indiceActual++;
            ActualizarLibro();
            return data;
        }
        return null;
    }

    public void RegistrarEntradaCielo()
    {
        if (indiceActual > 0)
        {
            NPCData npcJuzgado = listaNpcsHoy[indiceActual - 1];
            karmaAcumulado += npcJuzgado.karmaScore;
            Debug.Log("Karma actual: " + karmaAcumulado);
        }

        almasAceptadas++;
        ActualizarLibro();
        VerificarFinJornada();
    }

    public void RegistrarRechazo()
    {
        ActualizarLibro();
        VerificarFinJornada();
    }

    void VerificarFinJornada()
    {
        if (indiceActual >= listaNpcsHoy.Count)
        {
            StartCoroutine(SecuenciaFinalDia());
        }
    }

    // --- FINAL DEL JUEGO (Igual que antes) ---

    IEnumerator SecuenciaFinalDia()
    {
        botonDesbloqueado = false;
        Debug.Log("Fin del dia. Iniciando juicio...");

        yield return new WaitForSeconds(3f);

        telefonoDios.EmpezarA_Sonar();
        yield return new WaitUntil(() => !telefonoDios.audioSource.loop);
        yield return new WaitForSeconds(0.5f);

        if (almasAceptadas > cupoDiario)
        {
            yield return Hablar("... ¿Sabes contar?");
            yield return Hablar("Te dije claramente que el limite era de " + cupoDiario + " personas.");
            yield return Hablar("Has dejado entrar a " + almasAceptadas + ". Ahora tengo los fieles durmiendo en el suelo.");
            yield return Hablar("Esto es un desastre administrativo. Estás DESPEDIDO.");
        }
        else if (almasAceptadas == 0)
        {
            yield return Hablar("¿Hola? ¿Hay alguien ahi?");
            yield return Hablar("He revisado el registro y esta vacío. Cero entradas.");
            yield return Hablar("Entiendo que seas exigente, pero necesitamos llenar cuota.");
            yield return Hablar("No me sirves si no trabajas. Estás DESPEDIDO.");
        }
        else if (karmaAcumulado <= 0)
        {
            yield return Hablar("Mmm... los numeros cuadran. Has respetado el límite.");
            yield return Hablar("Pero estoy mirando la lista de invitados y... uff.");
            yield return Hablar("Has llenado el Cielo de basura.");
            yield return Hablar("Lo siento, chico. No tienes criterio moral. Estás DESPEDIDO.");
        }
        else
        {
            yield return Hablar("Veamos el registro...");
            yield return Hablar("El cupo es correcto. Bien hecho.");
            yield return Hablar("Y la calidad de las almas... Vaya, excelente.");
            yield return Hablar("Has filtrado a la gentuza y nos has traido a gente decente.");
            yield return Hablar("No es facil mantener el equilibrio, pero tu lo has clavado hoy.");
            yield return Hablar("Estás CONTRATADO, te has ganado el sueldo. Nos vemos mañana.");
        }

        yield return new WaitForSeconds(2f);
        telefonoDios.Colgar();

        yield return new WaitForSeconds(2f);
        Debug.Log("Cargando Menu...");
        SceneManager.LoadScene(nombreEscenaMenu);
    }

    void ActualizarLibro()
    {
        if (libroRegistro != null)
        {
            int pendientes = listaNpcsHoy.Count - indiceActual;
            libroRegistro.ActualizarPagina(almasAceptadas, cupoDiario, pendientes);
        }
    }

    IEnumerator Hablar(string frase)
    {
        telefonoDios.ReproducirFraseDios(frase);
        yield return new WaitForSeconds(2f + frase.Length * 0.08f);
    }
}