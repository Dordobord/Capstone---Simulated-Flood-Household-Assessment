using UnityEngine;
using System.Collections.Generic;

public class Interactable : MonoBehaviour
{
    public static List<Interactable> ActiveInteractables { get; private set; } = new List<Interactable>();

    [Header("Interactable Settings")]
    public string promptMessage = "Interact";
    public bool isSingleUse = false;
    protected bool hasInteracted = false;

    protected virtual void OnEnable()
    {
        if (!ActiveInteractables.Contains(this))
        {
            ActiveInteractables.Add(this);
        }
    }

    protected virtual void OnDisable()
    {
        ActiveInteractables.Remove(this);
    }

    public virtual void Interact(GameObject player)
    {
        if (isSingleUse && hasInteracted) return;
        hasInteracted = true;
        
        Debug.Log("Interacted with " + gameObject.name);
    }

    public virtual bool CanInteract(GameObject player)
    {
        return !isSingleUse || !hasInteracted;
    }

    public virtual string GetPrompt(GameObject player)
    {
        return promptMessage;
    }
}


