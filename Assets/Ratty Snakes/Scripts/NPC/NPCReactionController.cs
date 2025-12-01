using UnityEngine;

public class NPCReactionController : MonoBehaviour
{
    private NPCManager npcManager;
    private NPCManager.NPCData npcData;
    private NPCFloatingDialogue activeDialogue;

    public void Initialize(NPCManager manager, NPCManager.NPCData data, NPCFloatingDialogue dialogue)
    {
        npcManager = manager;
        npcData = data;
        activeDialogue = dialogue;
    }

    public void ReactPositive()
    {
        if (npcData != null && activeDialogue != null)
        {
            activeDialogue.ShowPositive(npcData.positiveReaction);
        }
        else if (npcData != null)
        {
            Debug.Log("ReactPositive: " + npcData.positiveReaction);
        }
        else
        {
            Debug.LogWarning("ReactPositive: npcData es null.");
        }
        // Añadir audio/anim future aquí
    }

    public void ReactNegative()
    {
        if (npcData != null && activeDialogue != null)
        {
            activeDialogue.ShowNegative(npcData.negativeReaction);
        }
        else if (npcData != null)
        {
            Debug.Log("ReactNegative: " + npcData.negativeReaction);
        }
        else
        {
            Debug.LogWarning("ReactNegative: npcData es null.");
        }

        // destruir floating dialogue si existe
        if (activeDialogue != null)
        {
            Destroy(activeDialogue.gameObject);
            activeDialogue = null;
        }

        // ocultar UI global (manager)
        if (npcManager != null && npcManager.uiController != null)
            npcManager.uiController.HideUI();

        // informar manager para limpiar y destruir NPC
        if (npcManager != null)
            npcManager.ClearCurrentNPC();

        // finalmente destruir este GameObject (NPC)
        Destroy(gameObject);
    }
}
