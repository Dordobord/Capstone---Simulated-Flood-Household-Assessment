using UnityEngine;

public class Sandbag : Carriable
{
    private void Start()
    {
        promptMessage = "Pick Up Sandbag";
        inventoryName = "Sandbag";
    }

    public override void Interact(GameObject player)
    {
        base.Interact(player);
        if (GameManager.Instance != null)
        {
            GameManager.Instance.UpdateObjectiveText("SANDBAG ACQUIRED: Take this sandbag to the front door entry zone to build a flood barrier!");
        }
    }
}

