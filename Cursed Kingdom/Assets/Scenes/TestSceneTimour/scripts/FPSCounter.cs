using UnityEngine;
using TMPro; // Use TextMeshPro for better performance

public class FPSCounter : MonoBehaviour
{
    public TextMeshProUGUI fpsText;
    private float deltaTime;
    void Awake()
    {
        // Disable V-Sync (required on some mobile configurations)
        QualitySettings.vSyncCount = 0;

        // Set to high number to "unlock" or target specific refresh rate
        // -1 restores the platform's default (usually 30 on mobile)
        Application.targetFrameRate = 144; // Or (int)Screen.currentResolution.refreshRateRatio.value for high-refresh screens
    }
    void Update()
    {
        // Simple smoothing to prevent flickering
        deltaTime += (Time.unscaledDeltaTime - deltaTime) * 0.1f;
        float fps = 1.0f / deltaTime;
        fpsText.text = $"FPS: {Mathf.Ceil(fps)}";
    }
}