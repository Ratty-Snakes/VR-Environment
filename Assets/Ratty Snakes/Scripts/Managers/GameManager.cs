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
        // Actualizamos el libro al inicio en silencio para que tenga datos visuales
        ActualizarLibro();
        StartCoroutine(RutinaInicioDia());
    }

    // --- CORRECCIÓN 1: Generación Inteligente para evitar días imposibles ---
    void GenerarDiaAleatorio()
    {
        if (poolGlobalNpcs == null || poolGlobalNpcs.Count == 0)
        {
            Debug.LogError("ERROR: Pool vacio.");
            return;
        }

        // Limpiamos listas anteriores
        listaNpcsHoy.Clear();

        // Creamos copias de listas para filtrar
        List<NPCData> copiaPool = new List<NPCData>(poolGlobalNpcs);
        List<NPCData> buenos = copiaPool.FindAll(x => x.karmaScore > 0);

        int maxPosible = Mathf.Min(maxClientes, poolGlobalNpcs.Count);
        int cantidadHoy = Random.Range(minClientes, maxPosible + 1);

        // PASO A: Aseguramos al menos 1 bueno (si existen en la base de datos)
        if (buenos.Count > 0)
        {
            int r = Random.Range(0, buenos.Count);
            NPCData npcBueno = buenos[r];

            listaNpcsHoy.Add(npcBueno);

            // Lo quitamos de la copiaPool para que no salga repetido en el relleno
            copiaPool.Remove(npcBueno);
        }

        // PASO B: Rellenamos el resto de huecos con cualquiera (buenos o malos)
        while (listaNpcsHoy.Count < cantidadHoy && copiaPool.Count > 0)
        {
            int indexAleatorio = Random.Range(0, copiaPool.Count);
            listaNpcsHoy.Add(copiaPool[indexAleatorio]);
            copiaPool.RemoveAt(indexAleatorio);
        }

        // PASO C: Barajamos la lista para que el bueno no salga siempre el primero
        Shuffle(listaNpcsHoy);

        // PASO D: Calculamos cupo. Usamos CeilToInt (redondeo hacia arriba) para ser más amables.
        cupoDiario = Mathf.CeilToInt(listaNpcsHoy.Count * porcentajeCupo);

        // Seguridad mínima
        if (cupoDiario < 1 && listaNpcsHoy.Count > 0) cupoDiario = 1;
    }

    // --- CORRECCIÓN 2: Algoritmo de Barajado (Fisher-Yates) ---
    void Shuffle<T>(List<T> list)
    {
        int n = list.Count;
        while (n > 1)
        {
            n--;
            int k = Random.Range(0, n + 1);
            T value = list[k];
            list[k] = list[n];
            list[n] = value;
        }
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
            yield return Hablar("¿Sabes qué? Hoy no ha muerto nadie. Tómate el día libre.");
            yield return Hablar("Vuelve a casa.");
            SceneManager.LoadScene(nombreEscenaMenu);
        }
        else
        {
            // Diálogos del día
            yield return Hablar("Soy yo otra vez. Casi se me olvida darte los números de hoy...");
            yield return Hablar("A ver, hoy andamos cortos de espacio. Las nubes están a reventar.");
            yield return Hablar("El LÍMITE DE HOY es de " + cupoDiario + " personas. Ni una más.");
            yield return Hablar("Y no te duermas, porque tienes a " + total + " almas esperando ahí en la cola.");

            ActualizarLibro();
            yield return Hablar("Por cierto, si pierdes la cuenta o quieres revisar cuántos difuntos faltan.Consulta el Libro de Registro Diario que tienes en la mesa.");
            yield return Hablar("Ahí se apunta todo automáticamente. Úsalo.");

            yield return Hablar("Eso es todo. Suerte.");
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
        if (indiceActual > 0)
        {
            // indiceActual ya avanzó, así que el NPC juzgado es el (indice - 1)
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

    // --- FINAL DEL JUEGO (Juicio Final) ---

    IEnumerator SecuenciaFinalDia()
    {
        botonDesbloqueado = false;
        Debug.Log("Fin del dia. Iniciando juicio...");

        yield return new WaitForSeconds(3f);

        telefonoDios.EmpezarA_Sonar();
        yield return new WaitUntil(() => !telefonoDios.audioSource.loop);
        yield return new WaitForSeconds(0.5f);

        // 1. CASO OVERBOOKING (Prioridad máxima)
        if (almasAceptadas > cupoDiario)
        {
            yield return Hablar("... ¿Sabes contar?");
            yield return Hablar("Te dije claramente que el limite era de " + cupoDiario + " personas.");
            yield return Hablar("Has dejado entrar a " + almasAceptadas + ". Ahora tengo los fieles durmiendo en el suelo.");
            yield return Hablar("Esto es un desastre administrativo. Estás DESPEDIDO.");
        }
        // 2. CASO VAGO (No trabajó)
        else if (almasAceptadas == 0)
        {
            yield return Hablar("¿Hola? ¿Hay alguien ahi?");
            yield return Hablar("He revisado el registro y esta vacío. Cero entradas.");
            yield return Hablar("Entiendo que seas exigente, pero necesitamos llenar cuota.");
            yield return Hablar("No me sirves si no trabajas. Estás DESPEDIDO.");
        }
        // 3. CASO KARMA NEGATIVO (Mala calidad)
        // CORRECCIÓN 3: Cambiado de <= 0 a < 0. Si el karma es 0, te salva.
        else if (karmaAcumulado < 0)
        {
            yield return Hablar("Mmm... los numeros cuadran. Has respetado el límite.");
            yield return Hablar("Pero estoy mirando la lista de invitados y... uff.");
            yield return Hablar("Has llenado el Cielo de basura.");
            yield return Hablar("Tu balance de Karma es negativo (" + karmaAcumulado + ").");
            yield return Hablar("Lo siento, chico. No tienes criterio moral. Estás DESPEDIDO.");
        }
        // 4. CASO ÉXITO (Cupo OK y Karma >= 0)
        else
        {
            yield return Hablar("Veamos el registro...");
            yield return Hablar("Has cumplido el límite. Bien hecho.");

            if (karmaAcumulado == 0)
            {
                // Diálogo especial si pasó por los pelos
                yield return Hablar("La calidad de las almas es... justa. Pero aceptable.");
            }
            else
            {
                yield return Hablar("Y la calidad de las almas... Vaya, excelente.");
                yield return Hablar("Has filtrado a la gentuza y nos has traido a gente decente.");
            }

            yield return Hablar("No es fácil mantener el equilibrio, pero tu lo has clavado hoy.");
            yield return Hablar("Estás CONTRATADO, te has ganado el sueldo. Que Dios te lo pague.");
            yield return Hablar("Nos vemos mañana.");
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