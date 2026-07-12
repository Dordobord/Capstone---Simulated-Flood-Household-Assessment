using UnityEngine;

public class Radio : Interactable
{
    private void Start()
    {
        promptMessage = "Turn on Emergency Radio";
    }

    public override void Interact(GameObject player)
    {
        base.Interact(player);
        if (GameManager.Instance != null)
        {
            GameManager.Instance.UpdateObjectiveText("RADIO ALERT: 'Heavy rain is causing flash floods in low-lying areas. Turn off utility power and secure doors!'");
        }
    }
}
