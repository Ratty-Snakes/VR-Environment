using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.SceneManagement; // Necesario para cambiar de escena

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
    public string nombreEscenaMenu = "MainMenu"; // Pon aqui el nombre EXACTO de tu Menu

    // Variables de Estado
    private List<NPCData> listaNpcsHoy = new List<NPCData>();
    private int cupoDiario;
    private int almasAceptadas = 0;
    private int karmaAcumulado = 0; // Puntos de bondad/maldad
    private int indiceActual = 0;
    private bool botonDesbloqueado = false;

    private void Awake()
    {
        if (Instance == null) Instance = this;
    }

    void Start()
    {
        GenerarDiaAleatorio();
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

        yield return new WaitUntil(() => !telefonoDios.audioSource.loop);
        yield return new WaitForSeconds(0.5f);

        int total = listaNpcsHoy.Count;

        if (total == 0)
        {
            yield return Hablar("Hoy no hay trabajo. Vuelve a casa.");
            SceneManager.LoadScene(nombreEscenaMenu);
        }
        else
        {
            yield return Hablar("Buenos dias. Vamos al lio.");
            yield return Hablar("Hoy tenemos a " + total + " almas en la puerta.");
            yield return Hablar("El limite estricto es de " + cupoDiario + " personas.");
            ActualizarLibro();
            yield return Hablar("Fijate bien en lo que han hecho en vida. No quiero errores.");
            yield return Hablar("Pulsa el boton rojo cuando estes listo.");
        }

        yield return new WaitForSeconds(1f);
        telefonoDios.Colgar();

        if (total > 0) botonDesbloqueado = true;
    }

    // --- LOGICA DE JUEGO ---

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
        // Recuperamos al NPC que acabamos de juzgar (el anterior al indice actual)
        if (indiceActual > 0)
        {
            NPCData npcJuzgado = listaNpcsHoy[indiceActual - 1];
            // Sumamos su puntuacion (puede ser negativa si es malo)
            karmaAcumulado += npcJuzgado.karmaScore;
            Debug.Log("Karma actual: " + karmaAcumulado);
        }

        almasAceptadas++;
        ActualizarLibro();
        VerificarFinJornada();
    }

    public void RegistrarRechazo()
    {
        // Al rechazar, no sumamos ni restamos karma (oportunidad perdida)
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

    // --- FINAL DEL JUEGO ---

    IEnumerator SecuenciaFinalDia()
    {
        botonDesbloqueado = false;
        Debug.Log("Fin del dia. Iniciando juicio...");

        // 1. Pausa dramatica antes de la llamada
        yield return new WaitForSeconds(3f);

        // 2. Llamada final
        telefonoDios.EmpezarA_Sonar();
        yield return new WaitUntil(() => !telefonoDios.audioSource.loop);
        yield return new WaitForSeconds(0.5f);

        // 3. Evaluacion de resultados

        // CASO A: TE HAS PASADO DEL CUPO (OVERCROWD)
        if (almasAceptadas > cupoDiario)
        {
            yield return Hablar("... ¿Sabes contar?");
            yield return Hablar("Te dije claramente que el limite era de " + cupoDiario + " personas.");
            yield return Hablar("Has dejado entrar a " + almasAceptadas + ". Ahora tengo angeles durmiendo en el suelo.");
            yield return Hablar("Esto es un desastre administrativo. Estas DESPEDIDO.");
        }
        // CASO B: NO HAS METIDO A NADIE (VAGO)
        else if (almasAceptadas == 0)
        {
            yield return Hablar("¿Hola? ¿Hay alguien ahi?");
            yield return Hablar("He revisado el registro y esta vacio. Cero entradas.");
            yield return Hablar("Entiendo que seas exigente, pero necesitamos llenar cuota.");
            yield return Hablar("No me sirves si no trabajas. Estas DESPEDIDO.");
        }
        // CASO C: CUPO BIEN, PERO KARMA MALO (MAL CRITERIO)
        else if (karmaAcumulado <= 0)
        {
            yield return Hablar("Mmm... los numeros cuadran. Has respetado el cupo.");
            yield return Hablar("Pero estoy mirando la lista de invitados y... uff.");
            yield return Hablar("Gente corrupta, mentirosos, ladrones de perritos...");
            yield return Hablar("Tu puntuacion de Karma es negativa. Has llenado el Cielo de basura.");
            yield return Hablar("Lo siento, chico. No tienes criterio moral. Estas DESPEDIDO.");
        }
        // CASO D: VICTORIA (TODO BIEN)
        else
        {
            yield return Hablar("Veamos el registro...");
            yield return Hablar("El cupo es correcto. Bien hecho.");
            yield return Hablar("Y la calidad de las almas... Vaya, excelente.");
            yield return Hablar("Has filtrado a la gentuza y nos has traido a gente decente.");
            yield return Hablar("No es facil mantener el equilibrio, pero tu lo has clavado hoy.");
            yield return Hablar("Descansa, te has ganado el sueldo. Nos vemos mañana.");
        }

        yield return new WaitForSeconds(2f);
        telefonoDios.Colgar();

        yield return new WaitForSeconds(2f);

        // 4. Volver al Menu
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