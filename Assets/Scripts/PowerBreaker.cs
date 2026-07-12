using UnityEngine;

public class PowerBreaker : Interactable
{
    private bool isPowerOn = true;

    private void Start()
    {
        isSingleUse = false; // Toggleable!
        UpdateBreakerState();
    }

    private void UpdateBreakerState()
    {
        if (isPowerOn)
        {
            promptMessage = "Shut Off Main Power Breaker";
        }
        else
        {
            promptMessage = "Turn On Main Power Breaker";
        }

        PlayerController player = FindFirstObjectByType<PlayerController>();
        if (player != null)
        {
            player.UpdateInteractionUI();
        }
    }

    public override void Interact(GameObject player)
    {
        isPowerOn = !isPowerOn;
        UpdateBreakerState();

        if (GameManager.Instance != null)
        {
            if (isPowerOn)
            {
                GameManager.Instance.UpdateObjectiveText("POWER RESTORED: Main power breaker is turned ON. Keep an eye on water levels!");
                //Restore default rainy ambient light
                RenderSettings.ambientLight = new Color(0.18f, 0.22f, 0.28f, 1f);
            }
            else
            {
                GameManager.Instance.UpdateObjectiveText("POWER SAFE: You safely turned OFF the main power breaker to prevent electrocution when water enters!");
                //Dim scene 
                RenderSettings.ambientLight = new Color(0.05f, 0.05f, 0.08f, 1f);
            }
        }

        // Toggle all light bulbs
        LightBulb[] bulbs = FindObjectsByType<LightBulb>(FindObjectsSortMode.None);
        foreach (LightBulb bulb in bulbs)
        {
            if (bulb != null)
            {
                bulb.SetLightState(isPowerOn);
            }
        }
    }
}

