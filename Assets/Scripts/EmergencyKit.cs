using UnityEngine;

public class EmergencyKit : Carriable
{
    private void Start()
    {
        promptMessage = "Pick Up Emergency Survival Kit";
        inventoryName = "Emergency Kit";
    }

    public override void Interact(GameObject player)
    {
        base.Interact(player); // This handles picking it up
        
        if (GameManager.Instance != null)
        {
            GameManager.Instance.UpdateObjectiveText
            ("KIT SECURED: You are carrying the survival kit! Carry it to a high, dry place (like the table) or drop it somewhere safe.");
        }
    }
}

