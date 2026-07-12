using UnityEngine;
using UnityEngine.UI;

public class RoleSelector : MonoBehaviour
{
    [SerializeField] private int roleIndex; // 0 = Child, 1 = Teen, 2 = Adult

    private void Start()
    {
        Button button = GetComponent<Button>();
        if (button != null)
        {
            button.onClick.AddListener(OnButtonClick);
        }
    }

    private void OnButtonClick()
    {
        if (GameManager.Instance != null)
        {
            GameManager.Instance.SelectRole(roleIndex);
        }
    }
}
