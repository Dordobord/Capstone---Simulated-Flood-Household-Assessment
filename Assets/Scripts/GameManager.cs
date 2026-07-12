using UnityEngine;
using TMPro;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    [Header("UI Panels")]
    [SerializeField] private GameObject roleSelectionPanel;
    [SerializeField] private GameObject hudPanel;
    [SerializeField] private TextMeshProUGUI roleText;
    [SerializeField] private TextMeshProUGUI objectiveText;
    [SerializeField] private TextMeshProUGUI interactionPromptText;

    [Header("Inventory UI Settings")]
    [SerializeField] private GameObject inventoryPanel;
    [SerializeField] private TextMeshProUGUI[] slotTexts;
    [SerializeField] private UnityEngine.UI.Image[] slotBackgrounds;
    [SerializeField] private Color selectedSlotColor = new Color(0.15f, 0.6f, 0.9f, 0.85f); // highlighted blue
    [SerializeField] private Color normalSlotColor = new Color(0.15f, 0.15f, 0.18f, 0.75f);  // dark grey

    [Header("Spawn Settings")]
    [SerializeField] private Transform spawnPoint;
    [SerializeField] private GameObject playerPrefab;
    [SerializeField] private CameraFollow mainCameraFollow;

    [Header("Materials")]
    [SerializeField] private Material adultMat;
    [SerializeField] private Material teenagerMat;
    [SerializeField] private Material childMat;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        Application.targetFrameRate = 60;
        QualitySettings.vSyncCount = 1; 

        if (roleSelectionPanel != null) roleSelectionPanel.SetActive(true);
        if (hudPanel != null) hudPanel.SetActive(false);
        HideInteractionPrompt();
        
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    public void SelectRole(int roleIndex)
    {
        if (playerPrefab == null || spawnPoint == null)
        {
            Debug.LogError("Player Prefab or Spawn Point is not assigned in the GameManager.");
            return;
        }

        GameObject playerObj = Instantiate(playerPrefab, spawnPoint.position, spawnPoint.rotation);
  
        PlayerController controller = playerObj.GetComponent<PlayerController>();
        CharacterController cc = playerObj.GetComponent<CharacterController>();
        Transform visualTrans = playerObj.transform.Find("Visual");
        Renderer visualRenderer = visualTrans != null ? visualTrans.GetComponent<Renderer>() : null;

        string roleName = "Unknown";
        float scale = 1.0f;
        Material selectedMat = adultMat;

        switch (roleIndex)
        {
            case 0: // Child
                roleName = "Child";
                scale = 0.6f;
                selectedMat = childMat;
                break;
            case 1: // Teenager
                roleName = "Teenager";
                scale = 0.85f;
                selectedMat = teenagerMat;
                break;
            case 2: // Adult
                roleName = "Adult";
                scale = 1.0f;
                selectedMat = adultMat;
                break;
        }

        if (cc != null)
        {
            cc.height = 2.0f * scale;
            cc.radius = 0.5f * scale;
            cc.center = new Vector3(0f, cc.height / 2f, 0f);
        }

        if (visualTrans != null)
        {
            visualTrans.localScale = new Vector3(scale, scale, scale);
            visualTrans.localPosition = new Vector3(0f, (2.0f * scale) / 2f, 0f);
        }

        if (visualRenderer != null && selectedMat != null)
        {
            visualRenderer.sharedMaterial = selectedMat;
        }

        if (controller != null)
        {
            controller.SetInputEnabled(true);
        }

        if (mainCameraFollow != null)
        {
            mainCameraFollow.target = playerObj.transform;
        }

        if (roleSelectionPanel != null) roleSelectionPanel.SetActive(false);
        if (hudPanel != null) hudPanel.SetActive(true);

        if (roleText != null)
        {
            roleText.text = "Role: " + roleName;
        }
    }

    public void ShowInteractionPrompt(string text)
    {
        if (interactionPromptText != null)
        {
            interactionPromptText.text = text;
            interactionPromptText.gameObject.SetActive(true);
        }
    }

    public void HideInteractionPrompt()
    {
        if (interactionPromptText != null)
        {
            interactionPromptText.gameObject.SetActive(false);
        }
    }

    public void UpdateObjectiveText(string text)
    {
        if (objectiveText != null)
        {
            objectiveText.text = text;
        }
    }

    public void UpdateInventoryUI(Carriable[] inventory, int selectedSlotIndex)
    {
        if (slotTexts == null || slotBackgrounds == null) return;

        for (int i = 0; i < 4; i++)
        {
            if (i < slotTexts.Length && slotTexts[i] != null)
            {
                if (inventory != null && i < inventory.Length && inventory[i] != null)
                {
                    slotTexts[i].text = inventory[i].inventoryName;
                }
                else
                {
                    slotTexts[i].text = "[Empty]";
                }
            }

            if (i < slotBackgrounds.Length && slotBackgrounds[i] != null)
            {
                slotBackgrounds[i].color = (i == selectedSlotIndex) ? selectedSlotColor : normalSlotColor;
            }
        }
    }
}


