using UnityEngine;

public class LightBulb : MonoBehaviour
{
    [Header("Settings")]
    [SerializeField] private Light pointLight;
    [SerializeField] private Renderer bulbRenderer;
    [SerializeField] private Color emissionColorOn = new Color(1.0f, 0.95f, 0.85f, 1.0f);
    [SerializeField] private Color emissionColorOff = Color.black;
    [SerializeField] private float emissionIntensityOn = 2.0f;

    private Material bulbMaterial;

    private void Awake()
    {
        if (pointLight == null) pointLight = GetComponentInChildren<Light>();
        if (bulbRenderer == null) bulbRenderer = GetComponent<Renderer>();

        if (bulbRenderer != null)
        {
            bulbMaterial = bulbRenderer.material;
        }
    }

    private void Start()
    {
        // Default to on
        SetLightState(true);
    }

    public void SetLightState(bool isOn)
    {
        if (pointLight != null)
        {
            pointLight.enabled = isOn;
        }

        if (bulbMaterial != null)
        {
            if (isOn)
            {
                bulbMaterial.SetColor("_EmissionColor", emissionColorOn * emissionIntensityOn);
                bulbMaterial.EnableKeyword("_EMISSION");
            }
            else
            {
                bulbMaterial.SetColor("_EmissionColor", emissionColorOff);
                bulbMaterial.DisableKeyword("_EMISSION");
            }
        }
    }
}
