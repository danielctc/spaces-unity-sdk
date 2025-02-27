using UnityEngine;
using Fusion;

public class PulseGlow : NetworkBehaviour
{
    public Material originalMaterial; // Assign the shared material here
    private Material carMaterial; // Unique material instance for this car
    public Color baseEmissionColor; // Base color of the glow
    public float pulseSpeed = 1.0f; // Speed of the pulsing
    public float maxEmissionIntensity = 2.0f; // Maximum intensity of emission
    public float lingerDuration = 0.5f; // Duration to linger on the base color

    private float lingerTimer = 0f;
    private bool isEmissionEnabled = false;

    private Renderer objectRenderer;
    private bool isInitialized = false;

    private void Awake()
    {
        InitializeComponents();
    }

    private void Start()
    {
        if (!isInitialized) return;

        // Only enable emission for the player with state authority
        if (!Object.HasStateAuthority)
        {
            DisableEmission();
        }
        else
        {
            EnableEmission();
        }
    }

    private void OnEnable()
    {
        InitializeComponents();

//        if (!isInitialized || !Object.HasStateAuthority) return;

        EnableEmission();
    }

    private void OnDisable()
    {
        if (!isInitialized || !Object.HasStateAuthority) return;

        DisableEmission();
    }

private void Update()
{
    if (!isInitialized || !Object.HasStateAuthority || !isEmissionEnabled)
        return;

    lingerTimer += Time.deltaTime;

    // Calculate a smoothed intensity value
    float emissionIntensity = Mathf.SmoothStep(0f, maxEmissionIntensity, Mathf.PingPong(lingerTimer * pulseSpeed, 1f));

    // Reset lingerTimer to create a lingering effect on the base color
    if (emissionIntensity == 0f && lingerTimer > lingerDuration)
    {
        lingerTimer = 0f;
    }

    Color finalColor = baseEmissionColor * Mathf.LinearToGammaSpace(emissionIntensity);
    carMaterial.SetColor("_EmissionColor", finalColor);

    // Remove DynamicGI for WebGL
    // DynamicGI.SetEmissive(objectRenderer, finalColor);
}

    private void InitializeComponents()
    {
        if (isInitialized) return;

        objectRenderer = GetComponent<Renderer>();
        if (objectRenderer == null)
        {
            Debug.LogWarning("Renderer component is missing on the GameObject.");
            return;
        }

        if (originalMaterial == null)
        {
            Debug.LogWarning("Original material is not assigned!");
            return;
        }

        carMaterial = new Material(originalMaterial);
        objectRenderer.material = carMaterial;

        isInitialized = true;
    }

    private void EnableEmission()
    {
        if (!isInitialized) return;

        isEmissionEnabled = true;
        carMaterial.EnableKeyword("_EMISSION");
        DynamicGI.SetEmissive(objectRenderer, baseEmissionColor);
    }

    private void DisableEmission()
    {
        if (!isInitialized) return;

        isEmissionEnabled = false;
        carMaterial.DisableKeyword("_EMISSION");
        DynamicGI.SetEmissive(objectRenderer, Color.black);
    }
}
