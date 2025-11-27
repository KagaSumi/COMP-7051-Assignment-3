using UnityEngine;

public class VisualEffectsController : MonoBehaviour
{
    [Header("References")]
    public Transform playerCamera;
    public Light sunLight; // <--- NEW: Drag your Directional Light here

    [Header("Settings")]
    public float dayIntensity = 1.0f;
    public float nightIntensity = 0.1f; // For the shader
    public float nightLightIntensity = 0.0f; // For the real sun (0 means total darkness)

    public float fogDensity = 0.1f;

    // State tracking
    public bool isNight = false;
    public bool isFoggy = false;
    public bool isFlashlightOn = false;

    void Start()
    {
        UpdateShaderState();
    }

    void Update()
    {
        HandleInputs();
        UpdateFlashlightPosition();
    }

    void HandleInputs()
    {
        if (Input.GetKeyDown(KeyCode.N)) { isNight = !isNight; UpdateShaderState(); }
        if (Input.GetKeyDown(KeyCode.M)) { isFoggy = !isFoggy; UpdateShaderState(); }
        if (Input.GetKeyDown(KeyCode.F)) { isFlashlightOn = !isFlashlightOn; UpdateShaderState(); }
    }

    public void UpdateShaderState()
    {
        // 1. Day / Night
        Shader.SetGlobalFloat("_AmbientIntensity", isNight ? nightIntensity : dayIntensity);

        // 2. Fog (Using Float 1.0 for true, 0.0 for false)
        Shader.SetGlobalFloat("_FogEnabled", isFoggy ? 1.0f : 0.0f);
        Shader.SetGlobalFloat("_FogDensity", fogDensity);
        Shader.SetGlobalColor("_FogColor", new Color(0.5f, 0.5f, 0.5f, 1f));

        // 3. Flashlight (Using Float)
        Shader.SetGlobalFloat("_FlashlightEnabled", isFlashlightOn ? 1.0f : 0.0f);

        // 4. Update Real Sun Light (Keep this from previous step)
        if (sunLight != null)
        {
            sunLight.intensity = isNight ? nightLightIntensity : 1.0f;
        }

        // 5. Sync Unity Fog
        RenderSettings.fog = isFoggy;
        RenderSettings.fogDensity = fogDensity;
        RenderSettings.fogColor = new Color(0.5f, 0.5f, 0.5f, 1f);
    }

    void UpdateFlashlightPosition()
    {
        if (playerCamera != null)
        {
            Shader.SetGlobalVector("_FlashlightPos", playerCamera.position);
            Shader.SetGlobalVector("_FlashlightDir", playerCamera.transform.forward);
        }
    }

    void OnApplicationQuit()
    {
        // Reset everything
        Shader.SetGlobalFloat("_AmbientIntensity", dayIntensity);
        Shader.SetGlobalInt("_FogEnabled", 0);
        Shader.SetGlobalInt("_FlashlightEnabled", 0);
    }
}