using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(CharacterController))]
public class PlayerController : MonoBehaviour
{
    [Header("First Person Movement")]
    [SerializeField] private float moveSpeed = 5.0f;
    [SerializeField] private float mouseSensitivity = 0.1f;
    [SerializeField] private float headHeightPercent = 0.85f; // camera height relative to character height

    [Header("Interaction Settings")]
    [SerializeField] private float interactionRadius = 4f;
    [SerializeField] private Transform holdPoint; 

    private CharacterController characterController;
    private InputAction moveAction;
    private InputAction lookAction;
    private InputAction interactAction;
    
    private float velocityY = 0f;
    private float cameraPitch = 0f;
    private bool isInputEnabled = false;

    private Interactable currentInteractable;
    private Transform mainCameraTransform;

    private Carriable[] inventory = new Carriable[4];
    private int selectedSlotIndex = 0;

    private void Awake()
    {
        characterController = GetComponent<CharacterController>();
    }

    private void Start()
    {
        if (InputSystem.actions != null)
        {
            moveAction = InputSystem.actions.FindAction("Player/Move");
            lookAction = InputSystem.actions.FindAction("Player/Look");
            interactAction = InputSystem.actions.FindAction("Player/Interact");
        }
        else
        {
            Debug.LogWarning("Project-wide Input Actions not found.");
        }
    }

    public void SetInputEnabled(bool enabled)
    {
        isInputEnabled = enabled;
        if (enabled)
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
            SetupFirstPersonCamera();
            UpdateActiveInventorySlot();
        }
        else
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
            ReleaseCamera();
        }
    }

    private void SetupFirstPersonCamera()
    {
        Camera mainCam = Camera.main;
        if (mainCam == null)
        {
            mainCam = FindFirstObjectByType<Camera>();
        }

        if (mainCam != null)
        {
            mainCameraTransform = mainCam.transform;
            mainCameraTransform.SetParent(transform);
            
            // Position camera at head height, slightly forward
            float cameraHeight = characterController.height * headHeightPercent;
            mainCameraTransform.localPosition = new Vector3(0f, cameraHeight, 0.1f);
            mainCameraTransform.localRotation = Quaternion.identity;

            if (holdPoint == null)
            {
                GameObject hpObj = new GameObject("HoldPoint");
                hpObj.transform.SetParent(mainCameraTransform);
                hpObj.transform.localPosition = new Vector3(0.0f, -0.35f, 1.1f);
                hpObj.transform.localRotation = Quaternion.identity;
                holdPoint = hpObj.transform;
            }

            CameraFollow followComp = mainCam.GetComponent<CameraFollow>();
            if (followComp != null)
            {
                followComp.enabled = false;
            }
        }
    }

    private void ReleaseCamera()
    {
        if (mainCameraTransform != null && mainCameraTransform.parent == transform)
        {
            mainCameraTransform.SetParent(null);
        }
    }

    private void Update()
    {
        if (characterController.isGrounded)
        {
            velocityY = -0.5f; // keep player grounded
        }
        else
        {
            velocityY += Physics.gravity.y * Time.deltaTime;
        }

        if (isInputEnabled && lookAction != null && mainCameraTransform != null)
        {
            Vector2 lookInput = lookAction.ReadValue<Vector2>();
            transform.Rotate(Vector3.up * lookInput.x * mouseSensitivity);

            cameraPitch -= lookInput.y * mouseSensitivity;
            cameraPitch = Mathf.Clamp(cameraPitch, -85f, 85f);
            mainCameraTransform.localRotation = Quaternion.Euler(cameraPitch, 0f, 0f);
        }

        Vector3 moveDirection = Vector3.zero;
        if (isInputEnabled && moveAction != null)
        {
            Vector2 input = moveAction.ReadValue<Vector2>();
            
            moveDirection = (transform.forward * input.y) + (transform.right * input.x);
            if (moveDirection.sqrMagnitude > 1.0f)
            {
                moveDirection.Normalize();
            }
        }

        Vector3 velocity = moveDirection * moveSpeed;
        velocity.y = velocityY;

        characterController.Move(velocity * Time.deltaTime);

        if (!isInputEnabled) return;

        HandleInventoryControls();
        FindClosestInteractable();

        if (interactAction != null && interactAction.WasPressedThisFrame())
        {
            if (currentInteractable != null)
            {
                currentInteractable.Interact(gameObject);
            }
            else if (GetCarriedItem() != null)
            {
                DropCarriedItem();
            }
        }
    }

    private void HandleInventoryControls()
    {
        int prevSlot = selectedSlotIndex;

        if (Mouse.current != null)
        {
            float scrollY = Mouse.current.scroll.y.ReadValue();
            if (scrollY > 0.1f)
            {
                selectedSlotIndex = (selectedSlotIndex - 1 + 4) % 4; 
            }
            else if (scrollY < -0.1f)
            {
                selectedSlotIndex = (selectedSlotIndex + 1) % 4; 
            }
        }

        if (Keyboard.current != null)
        {
            if (Keyboard.current.digit1Key.wasPressedThisFrame) selectedSlotIndex = 0;
            else if (Keyboard.current.digit2Key.wasPressedThisFrame) selectedSlotIndex = 1;
            else if (Keyboard.current.digit3Key.wasPressedThisFrame) selectedSlotIndex = 2;
            else if (Keyboard.current.digit4Key.wasPressedThisFrame) selectedSlotIndex = 3;
        }

        // C. Update if slot changed
        if (selectedSlotIndex != prevSlot)
        {
            UpdateActiveInventorySlot();
        }
    }

    private void UpdateActiveInventorySlot()
    {
        // Deactivate all items in inventory except the selected slot
        for (int i = 0; i < 4; i++)
        {
            if (inventory[i] != null)
            {
                if (i == selectedSlotIndex)
                {
                    // Activate held item
                    inventory[i].gameObject.SetActive(true);
                    inventory[i].transform.SetParent(holdPoint);
                    inventory[i].transform.localPosition = Vector3.zero;
                    inventory[i].transform.localRotation = Quaternion.identity;

                    // Ensure components are disabled while held
                    Collider col = inventory[i].GetComponent<Collider>();
                    if (col != null) col.enabled = false;

                    Rigidbody rb = inventory[i].GetComponent<Rigidbody>();
                    if (rb != null) rb.isKinematic = true;
                }
                else
                {
                    // Deactivate stashed items
                    inventory[i].gameObject.SetActive(false);
                }
            }
        }

        // Notify GameManager to update UI selection highlights
        if (GameManager.Instance != null)
        {
            GameManager.Instance.UpdateInventoryUI(inventory, selectedSlotIndex);
        }

        UpdateInteractionUI();
    }

    private void FindClosestInteractable()
    {
        var interactables = Interactable.ActiveInteractables;
        Interactable closest = null;
        float minDistance = interactionRadius;

        foreach (Interactable inter in interactables)
        {
            if (inter == null || !inter.CanInteract(gameObject)) continue;

            float distance = Vector3.Distance(transform.position, inter.transform.position);
            if (distance < minDistance)
            {
                minDistance = distance;
                closest = inter;
            }
        }

        if (closest != currentInteractable)
        {
            currentInteractable = closest;
            UpdateInteractionUI();
        }
    }

    public void UpdateInteractionUI()
    {
        if (GameManager.Instance != null)
        {
            if (currentInteractable != null)
            {
                GameManager.Instance.ShowInteractionPrompt("Press [E] to " + currentInteractable.GetPrompt(gameObject));
            }
            else
            {
                GameManager.Instance.HideInteractionPrompt();
            }
        }
    }

    public bool CarryItem(Carriable item)
    {
        // Find empty slot
        int targetSlot = -1;
        for (int i = 0; i < 4; i++)
        {
            if (inventory[i] == null)
            {
                targetSlot = i;
                break;
            }
        }

        if (targetSlot == -1)
        {
            if (GameManager.Instance != null)
            {
                GameManager.Instance.UpdateObjectiveText("INVENTORY FULL: Drop an item before picking up another!");
            }
            return false;
        }

        // Add to inventory
        inventory[targetSlot] = item;
        
        // Select the newly picked up item
        selectedSlotIndex = targetSlot;
        
        UpdateActiveInventorySlot();
        return true;
    }

    public Carriable GetCarriedItem()
    {
        return inventory[selectedSlotIndex];
    }

    public void ClearCarriedItem()
    {
        inventory[selectedSlotIndex] = null;
        UpdateActiveInventorySlot();
    }

    private void DropCarriedItem()
    {
        Carriable item = inventory[selectedSlotIndex];
        if (item == null) return;

        inventory[selectedSlotIndex] = null;

        item.transform.SetParent(null);
        // Place it directly in front of the player on the floor
        item.transform.position = transform.position + transform.forward * 1.5f + Vector3.up * 0.2f;
        item.transform.rotation = Quaternion.identity;

        // Re-enable physics/colliders
        Collider col = item.GetComponent<Collider>();
        if (col != null) col.enabled = true;

        Rigidbody rb = item.GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.isKinematic = false;
            rb.AddForce(transform.forward * 2f, ForceMode.Impulse);
        }

        item.OnDropped();
        UpdateActiveInventorySlot();
    }

    private void OnDestroy()
    {
        ReleaseCamera();
        // Make sure cursor is restored if player is destroyed or scene unloaded
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }
}




