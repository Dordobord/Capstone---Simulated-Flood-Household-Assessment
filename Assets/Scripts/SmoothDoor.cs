using UnityEngine;

public class SmoothDoor : Interactable
{
    [Header("Door Settings")]
    [SerializeField] private Transform doorPivot;
    [SerializeField] private float openAngle = -90f;
    [SerializeField] private float closedAngle = 0f;
    [SerializeField] private float smoothSpeed = 5f;

    private bool isOpen = false;
    private float targetAngle;
    private float currentAngle;

    private void Start()
    {
        promptMessage = "Open Door";
        targetAngle = closedAngle;
        currentAngle = closedAngle;

        if (doorPivot == null)
        {
            // Fallback to parent if attached to the mesh child
            doorPivot = transform.parent != null ? transform.parent : transform;
        }

        if (doorPivot != null)
        {
            doorPivot.localRotation = Quaternion.Euler(0f, currentAngle, 0f);
        }
    }

    public override void Interact(GameObject player)
    {
        isOpen = !isOpen;
        targetAngle = isOpen ? openAngle : closedAngle;
        promptMessage = isOpen ? "Close Door" : "Open Door";

        // Update interaction UI on player
        PlayerController pc = player.GetComponent<PlayerController>();
        if (pc != null)
        {
            pc.UpdateInteractionUI();
        }
    }

    private void Update()
    {
        if (doorPivot == null) return;
        currentAngle = Mathf.LerpAngle(currentAngle, targetAngle, Time.deltaTime * smoothSpeed);
        doorPivot.localRotation = Quaternion.Euler(0f, currentAngle, 0f);
    }
}

