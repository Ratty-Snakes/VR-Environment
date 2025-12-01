using UnityEngine;

public class NPCManager : MonoBehaviour
{
    [Header("Prefabs & refs")]
    public GameObject npcPrefab;
    public Transform[] waypoints; // WP_0..WP_5
    public NPCUIController uiController;     // NPC_UI_Canvas (World Space)
    public GameObject floatingDialoguePrefab;

    [Header("Demo data")]
    public NPCData[] npcDemoData;

    private GameObject currentNPC;
    private int currentIndex = 0;
    private NPCFloatingDialogue currentFloatingDialogue; // referencia para destruirla

    [System.Serializable]
    public class NPCData
    {
        public string npcName;
        public string causeOfDeath;
        public string bio;
        public string goodActs;
        public string badActs;
        [TextArea] public string positiveReaction;
        [TextArea] public string negativeReaction;
    }

    public void SpawnNextNPC()
    {
        if (currentNPC != null) return;
        if (npcDemoData == null || npcDemoData.Length == 0) return;
        if (currentIndex >= npcDemoData.Length) return;

        int idx = currentIndex;

        // 1) Instanciar NPC en WP_0
        currentNPC = Instantiate(npcPrefab, waypoints[0].position, Quaternion.identity);

        // 2) Floating dialogue: instanciar y asignar target
        currentFloatingDialogue = null;
        if (floatingDialoguePrefab != null)
        {
            GameObject floatingUI = Instantiate(floatingDialoguePrefab);
            var dialogueComp = floatingUI.GetComponent<NPCFloatingDialogue>();
            if (dialogueComp != null)
            {
                Transform head = currentNPC.transform.Find("HeadAnchor");
                if (head != null)
                {
                    dialogueComp.targetToFollow = head;
                    // intenta asignar TextMeshPro automáticamente
                    dialogueComp.dialogueText = floatingUI.GetComponentInChildren<TMPro.TextMeshProUGUI>();
                    currentFloatingDialogue = dialogueComp;
                }
                else
                {
                    Debug.LogWarning("NPCManager: HeadAnchor no encontrado en prefab NPC.");
                }
            }
            else
            {
                Debug.LogWarning("NPCManager: floatingDialoguePrefab no contiene NPCFloatingDialogue.");
            }
        }

        // 3) ReactionController
        var reaction = currentNPC.GetComponent<NPCReactionController>();
        if (reaction == null) reaction = currentNPC.AddComponent<NPCReactionController>();
        reaction.Initialize(this, npcDemoData[idx], currentFloatingDialogue);

        // 4) Waypoint movement
        var movement = currentNPC.GetComponent<NPCWaypointMovement>();
        if (movement == null) movement = currentNPC.AddComponent<NPCWaypointMovement>();
        movement.waypoints = waypoints;

        // 5) Suscribir callbacks
        movement.onReachedWaypoint1 = () =>
        {
            // Mostrar UI en UI_SpawnPoint (uiController se encarga de la posición fija)
            uiController?.ShowUI(
                npcDemoData[idx].npcName,
                npcDemoData[idx].causeOfDeath,
                npcDemoData[idx].bio,
                npcDemoData[idx].goodActs,
                npcDemoData[idx].badActs
            );
            // movement.isWaitingDecision comienza a true dentro de movement
        };

        movement.onReachedLastWaypoint = () =>
        {
            // limpiar UI y referencias cuando llegue a WP_5
            uiController?.HideUI();
            if (currentFloatingDialogue != null)
            {
                Destroy(currentFloatingDialogue.gameObject);
                currentFloatingDialogue = null;
            }
            ClearCurrentNPC();
        };

        // 6) Arrancar movimiento (WP_0 → WP_1)
        movement.StartWalking();

        currentIndex++;
    }

    public void ClearCurrentNPC()
    {
        currentNPC = null;
    }

    // ---------- GESTOS (todos van a través del manager y se ignoran si no corresponde) ----------
    public void OnThumbsUp()
    {
        if (currentNPC == null) return;

        var movement = currentNPC.GetComponent<NPCWaypointMovement>();
        if (movement == null) return;

        // Solo procesar si está esperando decisión
        if (!movement.isWaitingDecision) return;

        // Reacción positiva y continuar hacia el cielo
        var reaction = currentNPC.GetComponent<NPCReactionController>();
        reaction?.ReactPositive();

        // ocultar UI fija (se destruye en la llegada a WP_5, pero cerramos la UI de CV al tomar la decisión)
        uiController?.HideUI();

        // destruir floating dialogue? reaction puede gestionarlo; igualmente lo limpiamos
        if (currentFloatingDialogue != null)
        {
            // permitir que la reacción escriba en él; si quieres que se muestre, no lo destruyas aquí
            // pero si prefieres que desaparezca al tomar la decisión, destrúyelo:
            // Destroy(currentFloatingDialogue.gameObject);
            // currentFloatingDialogue = null;
        }

        // Continuar movimiento
        movement.ContinueWalkingToHeaven();
    }

    public void OnThumbsDown() { HandleNegativeGesture("ThumbsDown"); }
    public void OnMiddleFinger() { HandleNegativeGesture("MiddleFinger"); }
    public void OnNegationGesture() { HandleNegativeGesture("NegationIndex"); }

    private void HandleNegativeGesture(string gestureName)
    {
        if (currentNPC == null) return;

        var movement = currentNPC.GetComponent<NPCWaypointMovement>();
        if (movement == null || !movement.isWaitingDecision) return;

        // Reacción negativa (se encargará de destruir NPC si está implementado así)
        var reaction = currentNPC.GetComponent<NPCReactionController>();
        reaction?.ReactNegative();

        // Ocultar UI fija y destruir diálogo flotante
        uiController?.HideUI();
        if (currentFloatingDialogue != null)
        {
            Destroy(currentFloatingDialogue.gameObject);
            currentFloatingDialogue = null;
        }

        // Limpiar referencia de manager (ReactNegative puede destruir el NPC también)
        ClearCurrentNPC();
    }
}
