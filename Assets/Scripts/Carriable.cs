using UnityEngine;

public class Carriable : Interactable
{
    [Header("Inventory Settings")]
    public string inventoryName = "Item";

    protected bool isCarried;
    protected bool isEquipped;

    private void Awake()
    {
        if (GetComponent<Rigidbody>() == null)
        {
            Rigidbody rb = gameObject.AddComponent<Rigidbody>();
            rb.mass = 5f;
            rb.collisionDetectionMode = CollisionDetectionMode.Continuous;
        }
    }

    public override void Interact(GameObject player)
    {
        if (isCarried)
            return;

        PlayerController playerController =
            player.GetComponent<PlayerController>();

        if (playerController == null)
            return;

        // Only mark it as carried if the inventory accepted the item.
        if (playerController.CarryItem(this))
        {
            isCarried = true;
        }
    }

    public override bool CanInteract(GameObject player)
    {
        if (isCarried)
            return false;

        PlayerController playerController =
            player.GetComponent<PlayerController>();

        if (playerController != null &&
            playerController.IsHoldingItem)
        {
            return false;
        }

        return base.CanInteract(player);
    }

    public virtual void OnEquipped()
    {
        isEquipped = true;
    }

    public virtual void OnHolstered()
    {
        isEquipped = false;
    }

    public virtual void OnDropped()
    {
        isCarried = false;
        isEquipped = false;
    }

    public virtual void OnPlaced()
    {
        isCarried = false;
        isEquipped = false;

        Destroy(this);
    }
}