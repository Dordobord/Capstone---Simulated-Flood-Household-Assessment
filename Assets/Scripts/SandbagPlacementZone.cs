using UnityEngine;

public class SandbagPlacementZone : Interactable
{
    [Header("Placement Settings")]
    [SerializeField] private GameObject[] sandbagVisuals; // Inactive sandbags in the scene that we activate on placement
    private int placedCount = 0;

    private void Start()
    {
        promptMessage = "Place Sandbag to block entrance";

        if (sandbagVisuals != null)
        {
            foreach (var visual in sandbagVisuals)
            {
                if (visual != null) visual.SetActive(false);
            }
        }
    }

    public override void Interact(GameObject player)
    {
        PlayerController pc = player.GetComponent<PlayerController>();
        if (pc == null) return;

        Carriable carried = pc.GetCarriedItem();
        if (carried != null && carried is Sandbag)
        {
            // Destroy the carried sandbag object
            pc.ClearCarriedItem();
            Destroy(carried.gameObject);

            // Activate one visual sandbag
            if (sandbagVisuals != null && placedCount < sandbagVisuals.Length)
            {
                if (sandbagVisuals[placedCount] != null)
                {
                    sandbagVisuals[placedCount].SetActive(true);
                }
                placedCount++;
            }

            // Update objective
            if (GameManager.Instance != null)
            {
                if (placedCount >= sandbagVisuals.Length)
                {
                    GameManager.Instance.UpdateObjectiveText($"BARRIER COMPLETE: You placed {placedCount}/{sandbagVisuals.Length} sandbags! \n The main entrance is now fully blocked against incoming floodwater.");
                    promptMessage = "Barrier Complete";
                }
                else
                {
                    GameManager.Instance.UpdateObjectiveText($"BARRIER BUILDING: Placed sandbag ({placedCount}/{sandbagVisuals.Length}). Fetch more sandbags from the pile!");
                }
            }
        }
    }

    public override bool CanInteract(GameObject player)
    {
        if (placedCount >= sandbagVisuals.Length) return false;

        // Only allow if the player is holding a Sandbag
        PlayerController pc = player.GetComponent<PlayerController>();
        if (pc != null)
        {
            Carriable carried = pc.GetCarriedItem();
            if (carried != null && carried is Sandbag)
            {
                return true;
            }
        }
        return false;
    }

    public override string GetPrompt(GameObject player)
    {
        return $"Place Sandbag ({placedCount}/{sandbagVisuals.Length})";
    }
}
