using UnityEngine;
using UnityEngine.InputSystem;

public class Flashlight : Carriable
{
    [Header("Flashlight Settings")]
    [SerializeField] private Light flashlightLight;
    [SerializeField] private Key toggleKey = Key.F;
    [SerializeField] private bool turnOffWhenHolstered = true;

    private bool isLightOn;

    private void Start()
    {
        promptMessage = "Pick Up Flashlight";
        inventoryName = "Flashlight";

        if (flashlightLight == null)
        {
            flashlightLight =
                GetComponentInChildren<Light>(true);
        }

        SetFlashlight(false);
    }

    private void Update()
    {
        if (!isEquipped)
            return;

        if (Keyboard.current == null)
            return;

        if (Keyboard.current[toggleKey].wasPressedThisFrame)
        {
            SetFlashlight(!isLightOn);
        }
    }

    public override void OnEquipped()
    {
        base.OnEquipped();
    }

    public override void OnHolstered()
    {
        base.OnHolstered();

        if (turnOffWhenHolstered)
        {
            SetFlashlight(false);
        }
    }

    public override void OnDropped()
    {
        base.OnDropped();
        SetFlashlight(false);
    }

    private void SetFlashlight(bool state)
    {
        isLightOn = state;

        if (flashlightLight != null)
        {
            flashlightLight.enabled = state;
        }
    }
}