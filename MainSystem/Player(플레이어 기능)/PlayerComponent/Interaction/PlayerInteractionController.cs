using System.Collections.Generic;
using UnityEngine;

public class PlayerInteractionController : MonoBehaviour // 주변에 NPC찾는 스크립트
{
    private readonly HashSet<NPCBase> nearbyNPCs = new(); // 가까이에 있는 NPC들

    private NPCBase currentFocused; // 현재 타겟팅된 NPC

    private void OnTriggerEnter(Collider other)
    {
        if (other.TryGetComponent(out NPCBase npc))
            nearbyNPCs.Add(npc);
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.TryGetComponent(out NPCBase npc))
        {
            nearbyNPCs.Remove(npc);

            if (currentFocused == npc)
            {
                currentFocused.OnUnfocused();
                currentFocused = null;
            }
        }
    }

    private void Update()
    {
        UpdateFocusedNPC();
    }

    void UpdateFocusedNPC()
    {
        if (nearbyNPCs.Count == 0)
        {
            if (currentFocused != null)
            {
                currentFocused.OnUnfocused();
                currentFocused = null;
            }
            return;
        }

        NPCBase closest = currentFocused;
        float minDist = currentFocused != null
            ? (currentFocused.transform.position - transform.position).sqrMagnitude
            : float.MaxValue;

        foreach (var npc in nearbyNPCs)
        {
            if (npc == currentFocused)
                continue;

            float dist = (npc.transform.position - transform.position).sqrMagnitude;

            if (dist + 0.01f < minDist)
            {
                minDist = dist;
                closest = npc;
            }
        }

        if (closest == currentFocused)
            return;

        currentFocused?.OnUnfocused();
        currentFocused = closest;
        currentFocused?.OnFocused();
    }


    public void CloseNPCInteract()
    {
        if (currentFocused != null)
        {
            currentFocused.Interact();
        }
    }
}
