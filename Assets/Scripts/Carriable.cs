using UnityEngine;

public class Carriable : Interactable
{
    [Header("Inventory Settings")]
    public string inventoryName = "Item";

    protected bool isCarried = false;

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
        if (isCarried) return;

        PlayerController pc = player.GetComponent<PlayerController>();
        if (pc != null)
        {
            isCarried = true;
            pc.CarryItem(this);
        }
    }

    public override bool CanInteract(GameObject player)
    {
        if (isCarried) return false;
        
        PlayerController pc = player.GetComponent<PlayerController>();
        if (pc != null && pc.GetCarriedItem() != null)
        {
            return false;
        }

        return base.CanInteract(player);
    }

    public virtual void OnDropped()
    {
        isCarried = false;
    }

    public virtual void OnPlaced()
    {
        isCarried = false;
        Destroy(this); // Remove carriable behavior so it becomes a static placed object
    }
}
