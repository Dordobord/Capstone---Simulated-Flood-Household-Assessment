using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(CharacterController))]
public class PlayerController : MonoBehaviour
{
    [Header("First Person Movement")]
    [SerializeField] private float moveSpeed = 5.0f;
    [SerializeField] private float mouseSensitivity = 0.1f;
    [SerializeField, Range(0.5f, 1f)] private float headHeightPercent = 0.85f;

    [Header("Interaction Settings")]
    [SerializeField] private float interactionRadius = 4f;
    [SerializeField] private Transform holdPoint;

    [Header("Inventory Settings")]
    [SerializeField, Min(1)] private int inventorySlotCount = 7;
    [SerializeField] private bool equipNewlyPickedItem = true;

    private CharacterController characterController;
    private InputAction moveAction;
    private InputAction lookAction;
    private InputAction interactAction;

    private float velocityY;
    private float cameraPitch;
    private bool isInputEnabled;

    private Interactable currentInteractable;
    private Transform mainCameraTransform;

    private Carriable[] inventory;
    private int selectedSlotIndex;
    private bool isSelectedItemHolstered = true;

    public int InventorySlotCount => inventory.Length;
    public int SelectedSlotIndex => selectedSlotIndex;
    public bool IsHoldingItem => GetHeldItem() != null;

    private void Awake()
    {
        characterController = GetComponent<CharacterController>();
        inventorySlotCount = Mathf.Max(1, inventorySlotCount);
        inventory = new Carriable[inventorySlotCount];
    }

    private void Start()
    {
        if (InputSystem.actions == null)
        {
            Debug.LogWarning("Project-wide Input Actions not found.");
            return;
        }

        moveAction = InputSystem.actions.FindAction("Player/Move");
        lookAction = InputSystem.actions.FindAction("Player/Look");
        interactAction = InputSystem.actions.FindAction("Player/Interact");
    }

    public void SetInputEnabled(bool enabled)
    {
        isInputEnabled = enabled;

        if (enabled)
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
            SetupFirstPersonCamera();
            RefreshInventoryState();
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

        if (mainCam == null)
        {
            Debug.LogWarning("No camera was found for the player.");
            return;
        }

        mainCameraTransform = mainCam.transform;
        mainCameraTransform.SetParent(transform);

        float cameraHeight = characterController.height * headHeightPercent;
        mainCameraTransform.localPosition = new Vector3(0f, cameraHeight, 0.1f);
        mainCameraTransform.localRotation = Quaternion.identity;

        if (holdPoint == null)
        {
            GameObject holdPointObject = new GameObject("HoldPoint");
            holdPointObject.transform.SetParent(mainCameraTransform);
            holdPointObject.transform.localPosition = new Vector3(0f, -0.35f, 1.1f);
            holdPointObject.transform.localRotation = Quaternion.identity;
            holdPoint = holdPointObject.transform;
        }

        CameraFollow followComponent = mainCam.GetComponent<CameraFollow>();
        if (followComponent != null)
        {
            followComponent.enabled = false;
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
        ApplyGravity();
        HandleLook();
        HandleMovement();

        if (!isInputEnabled)
        {
            return;
        }

        HandleInventoryControls();
        FindClosestInteractable();

        if (interactAction != null && interactAction.WasPressedThisFrame())
        {
            if (currentInteractable != null)
            {
                currentInteractable.Interact(gameObject);
            }
            else if (IsHoldingItem)
            {
                DropHeldItem();
            }
        }
    }

    private void ApplyGravity()
    {
        if (characterController.isGrounded)
        {
            velocityY = -0.5f;
        }
        else
        {
            velocityY += Physics.gravity.y * Time.deltaTime;
        }
    }

    private void HandleLook()
    {
        if (!isInputEnabled || lookAction == null || mainCameraTransform == null)
        {
            return;
        }

        Vector2 lookInput = lookAction.ReadValue<Vector2>();
        transform.Rotate(Vector3.up * lookInput.x * mouseSensitivity);

        cameraPitch -= lookInput.y * mouseSensitivity;
        cameraPitch = Mathf.Clamp(cameraPitch, -85f, 85f);
        mainCameraTransform.localRotation = Quaternion.Euler(cameraPitch, 0f, 0f);
    }

    private void HandleMovement()
    {
        Vector3 moveDirection = Vector3.zero;

        if (isInputEnabled && moveAction != null)
        {
            Vector2 input = moveAction.ReadValue<Vector2>();
            moveDirection = transform.forward * input.y + transform.right * input.x;

            if (moveDirection.sqrMagnitude > 1f)
            {
                moveDirection.Normalize();
            }
        }

        Vector3 velocity = moveDirection * moveSpeed;
        velocity.y = velocityY;
        characterController.Move(velocity * Time.deltaTime);
    }

    private void HandleInventoryControls()
    {
        if (inventory.Length == 0)
        {
            return;
        }

        if (Mouse.current != null)
        {
            float scrollY = Mouse.current.scroll.y.ReadValue();

            if (scrollY > 0.1f)
            {
                SelectSlot((selectedSlotIndex - 1 + inventory.Length) % inventory.Length);
            }
            else if (scrollY < -0.1f)
            {
                SelectSlot((selectedSlotIndex + 1) % inventory.Length);
            }
        }

        if (Keyboard.current == null)
        {
            return;
        }

        int pressedSlot = GetPressedNumberSlot();
        if (pressedSlot >= 0 && pressedSlot < inventory.Length)
        {
            if (pressedSlot == selectedSlotIndex)
            {
                ToggleSelectedItemHolster();
            }
            else
            {
                SelectSlot(pressedSlot);
            }
        }
    }

    private int GetPressedNumberSlot()
    {
        if (Keyboard.current.digit1Key.wasPressedThisFrame) return 0;
        if (Keyboard.current.digit2Key.wasPressedThisFrame) return 1;
        if (Keyboard.current.digit3Key.wasPressedThisFrame) return 2;
        if (Keyboard.current.digit4Key.wasPressedThisFrame) return 3;
        if (Keyboard.current.digit5Key.wasPressedThisFrame) return 4;
        if (Keyboard.current.digit6Key.wasPressedThisFrame) return 5;
        if (Keyboard.current.digit7Key.wasPressedThisFrame) return 6;
        if (Keyboard.current.digit8Key.wasPressedThisFrame) return 7;
        if (Keyboard.current.digit9Key.wasPressedThisFrame) return 8;
        return -1;
    }

    private void SelectSlot(int slotIndex)
    {
        if (slotIndex < 0 || slotIndex >= inventory.Length)
        {
            return;
        }

        selectedSlotIndex = slotIndex;

        // Selecting a different slot equips its item. Empty slots remain holstered.
        isSelectedItemHolstered = inventory[selectedSlotIndex] == null;
        RefreshInventoryState();
    }

    private void ToggleSelectedItemHolster()
    {
        if (inventory[selectedSlotIndex] == null)
        {
            isSelectedItemHolstered = true;
            RefreshInventoryState();
            return;
        }

        isSelectedItemHolstered = !isSelectedItemHolstered;
        RefreshInventoryState();
    }

    private void RefreshInventoryState()
    {
        for (int i = 0; i < inventory.Length; i++)
        {
            Carriable item = inventory[i];
            if (item == null)
            {
                continue;
            }

            bool shouldBeHeld = i == selectedSlotIndex && !isSelectedItemHolstered;

            if (shouldBeHeld)
            {
                EquipItem(item);
            }
            else
            {
                HolsterItem(item);
            }
        }

        if (GameManager.Instance != null)
        {
            GameManager.Instance.UpdateInventoryUI(inventory, selectedSlotIndex);
        }

        UpdateInteractionUI();
    }

    private void EquipItem(Carriable item)
    {
        if (holdPoint == null)
        {
            Debug.LogWarning("HoldPoint is missing. The selected inventory item cannot be equipped.");
            return;
        }

        item.transform.SetParent(holdPoint);
        item.transform.localPosition = Vector3.zero;
        item.transform.localRotation = Quaternion.identity;
        SetItemPhysics(item, false, true);
        item.gameObject.SetActive(true);
        item.OnEquipped();
    }

    private void HolsterItem(Carriable item)
    {
        item.OnHolstered();
        SetItemPhysics(item, false, true);
        item.gameObject.SetActive(false);
    }

    private static void SetItemPhysics(Carriable item, bool colliderEnabled, bool isKinematic)
    {
        Collider[] colliders = item.GetComponentsInChildren<Collider>(true);
        foreach (Collider itemCollider in colliders)
        {
            itemCollider.enabled = colliderEnabled;
        }

        Rigidbody itemRigidbody = item.GetComponent<Rigidbody>();
        if (itemRigidbody != null)
        {
            itemRigidbody.isKinematic = isKinematic;
            itemRigidbody.linearVelocity = Vector3.zero;
            itemRigidbody.angularVelocity = Vector3.zero;
        }
    }

    private void FindClosestInteractable()
    {
        Interactable closest = null;
        float minimumDistance = interactionRadius;

        foreach (Interactable interactable in Interactable.ActiveInteractables)
        {
            if (interactable == null || !interactable.CanInteract(gameObject))
            {
                continue;
            }

            float distance = Vector3.Distance(transform.position, interactable.transform.position);
            if (distance < minimumDistance)
            {
                minimumDistance = distance;
                closest = interactable;
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
        if (GameManager.Instance == null)
        {
            return;
        }

        if (currentInteractable != null)
        {
            GameManager.Instance.ShowInteractionPrompt(
                "Press [E] to " + currentInteractable.GetPrompt(gameObject));
        }
        else
        {
            GameManager.Instance.HideInteractionPrompt();
        }
    }

    public bool CarryItem(Carriable item)
    {
        if (item == null)
        {
            return false;
        }

        // A new carriable cannot be picked up while an item is visibly held.
        if (IsHoldingItem)
        {
            if (GameManager.Instance != null)
            {
                GameManager.Instance.UpdateObjectiveText(
                    "HOLSTER YOUR CURRENT ITEM BEFORE PICKING UP ANOTHER ITEM.");
            }

            return false;
        }

        int targetSlot = FindFirstEmptySlot();
        if (targetSlot < 0)
        {
            if (GameManager.Instance != null)
            {
                GameManager.Instance.UpdateObjectiveText(
                    "INVENTORY FULL: Drop an item before picking up another!");
            }

            return false;
        }

        inventory[targetSlot] = item;
        selectedSlotIndex = targetSlot;
        isSelectedItemHolstered = !equipNewlyPickedItem;
        RefreshInventoryState();
        return true;
    }

    private int FindFirstEmptySlot()
    {
        for (int i = 0; i < inventory.Length; i++)
        {
            if (inventory[i] == null)
            {
                return i;
            }
        }

        return -1;
    }

    // Returns only the item that is currently visible in the player's hands.
    public Carriable GetCarriedItem()
    {
        return GetHeldItem();
    }

    public Carriable GetHeldItem()
    {
        if (isSelectedItemHolstered || selectedSlotIndex < 0 || selectedSlotIndex >= inventory.Length)
        {
            return null;
        }

        return inventory[selectedSlotIndex];
    }

    public Carriable GetSelectedInventoryItem()
    {
        if (selectedSlotIndex < 0 || selectedSlotIndex >= inventory.Length)
        {
            return null;
        }

        return inventory[selectedSlotIndex];
    }

    public void ClearCarriedItem()
    {
        if (selectedSlotIndex < 0 || selectedSlotIndex >= inventory.Length)
        {
            return;
        }

        inventory[selectedSlotIndex] = null;
        isSelectedItemHolstered = true;
        RefreshInventoryState();
    }

    private void DropHeldItem()
    {
        Carriable item = GetHeldItem();
        if (item == null)
        {
            return;
        }

        inventory[selectedSlotIndex] = null;
        isSelectedItemHolstered = true;

        item.gameObject.SetActive(true);
        item.transform.SetParent(null);
        item.transform.position = transform.position + transform.forward * 1.5f + Vector3.up * 0.2f;
        item.transform.rotation = Quaternion.identity;

        Collider[] colliders = item.GetComponentsInChildren<Collider>(true);
        foreach (Collider itemCollider in colliders)
        {
            itemCollider.enabled = true;
        }

        Rigidbody itemRigidbody = item.GetComponent<Rigidbody>();
        if (itemRigidbody != null)
        {
            itemRigidbody.isKinematic = false;
            itemRigidbody.AddForce(transform.forward * 2f, ForceMode.Impulse);
        }

        item.OnDropped();
        RefreshInventoryState();
    }

    private void OnValidate()
    {
        inventorySlotCount = Mathf.Max(1, inventorySlotCount);
    }

    private void OnDestroy()
    {
        ReleaseCamera();
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }
}