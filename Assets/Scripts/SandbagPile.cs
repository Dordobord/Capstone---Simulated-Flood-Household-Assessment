using UnityEngine;

public class SandbagPile : Interactable
{
    [SerializeField] private GameObject sandbagPrefab;

    private void Start()
    {
        promptMessage = "Take Sandbag from Pile";
    }

    public override void Interact(GameObject player)
    {
        if (sandbagPrefab == null)
        {
            Debug.LogError("Sandbag Prefab is not assigned to SandbagPile.");
            return;
        }

        PlayerController pc = player.GetComponent<PlayerController>();
        if (pc != null && pc.GetCarriedItem() == null)
        {
            // Instantiate a sandbag slightly in front of the pile
            GameObject sandbagObj = Instantiate(sandbagPrefab, transform.position + transform.forward * 1.0f + Vector3.up * 0.5f, Quaternion.identity);
            Sandbag sandbag = sandbagObj.GetComponent<Sandbag>();
            if (sandbag != null)
            {
                // Force the player to carry it
                sandbag.Interact(player);
            }
        }
    }

    public override bool CanInteract(GameObject player)
    {
        // Only allow if the player is not currently carrying anything
        PlayerController pc = player.GetComponent<PlayerController>();
        if (pc != null && pc.GetCarriedItem() != null)
        {
            return false;
        }
        return base.CanInteract(player);
    }
}
