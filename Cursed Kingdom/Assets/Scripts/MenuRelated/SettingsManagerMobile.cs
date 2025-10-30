using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Audio;

public class SettingsManagerMobile : MonoBehaviour
{
    [Header("Graphics")]
    public Toggle performanceToggle;
    public Toggle qualityToggle;

    [Header("Audio")]
    public AudioMixer audioMixer;
    public Slider masterSlider;
    public Slider musicSlider;
    public Slider sfxSlider;

    [Header("General")]
    public Button applyButton;

    private const string QualityKey = "QualitySetting";
    private const string MasterKey = "MasterVol";
    private const string MusicKey = "MusicVol";
    private const string SfxKey = "SfxVol";

    private int currentQuality;       // currently saved (applied) quality
    private int pendingQuality;       // currently selected but not yet applied
    private float currentMasterVol;
    private float currentMusicVol;
    private float currentSfxVol;

    void OnEnable()
    {
        LoadSettings();

        // clear existing listeners
        performanceToggle.onValueChanged.RemoveAllListeners();
        qualityToggle.onValueChanged.RemoveAllListeners();
        masterSlider.onValueChanged.RemoveAllListeners();
        musicSlider.onValueChanged.RemoveAllListeners();
        sfxSlider.onValueChanged.RemoveAllListeners();
        applyButton.onClick.RemoveAllListeners();

        // toggles (mutually exclusive)
        performanceToggle.onValueChanged.AddListener(isOn =>
        {
            if (isOn)
            {
                qualityToggle.isOn = false;
                pendingQuality = 1; // Low preset
            }
            else if (!qualityToggle.isOn)
            {
                performanceToggle.isOn = true; // never allow both off
            }
        });

        qualityToggle.onValueChanged.AddListener(isOn =>
        {
            if (isOn)
            {
                performanceToggle.isOn = false;
                pendingQuality = 2; // High preset
            }
            else if (!performanceToggle.isOn)
            {
                qualityToggle.isOn = true; // never allow both off
            }
        });

        // audio
        masterSlider.onValueChanged.AddListener(v =>
        {
            currentMasterVol = v;
            SetMixerVolume("MasterVol", v);
        });
        musicSlider.onValueChanged.AddListener(v =>
        {
            currentMusicVol = v;
            SetMixerVolume("MusicVol", v);
        });
        sfxSlider.onValueChanged.AddListener(v =>
        {
            currentSfxVol = v;
            SetMixerVolume("SfxVol", v);
        });

        applyButton.onClick.AddListener(ApplySettings);
    }

    public void LoadSettings()
    {
        currentQuality = PlayerPrefs.GetInt(QualityKey, 1); // default to "Low"
        pendingQuality = currentQuality; // start synced

        currentMasterVol = PlayerPrefs.GetFloat(MasterKey, 1f);
        currentMusicVol = PlayerPrefs.GetFloat(MusicKey, 1f);
        currentSfxVol = PlayerPrefs.GetFloat(SfxKey, 1f);

        UpdateToggleStates(currentQuality);

        masterSlider.value = currentMasterVol;
        musicSlider.value = currentMusicVol;
        sfxSlider.value = currentSfxVol;

        SetMixerVolume("MasterVol", currentMasterVol);
        SetMixerVolume("MusicVol", currentMusicVol);
        SetMixerVolume("SfxVol", currentSfxVol);
    }

    private void UpdateToggleStates(int qualityLevel)
    {
        performanceToggle.isOn = (qualityLevel == 1);
        qualityToggle.isOn = (qualityLevel == 2);

        if (!performanceToggle.isOn && !qualityToggle.isOn)
            performanceToggle.isOn = true;
    }

    private void ApplySettings()
    {
        // apply pending quality selection
        currentQuality = pendingQuality;
        QualitySettings.SetQualityLevel(currentQuality, true);
        PlayerPrefs.SetInt(QualityKey, currentQuality);

        // save audio
        PlayerPrefs.SetFloat(MasterKey, currentMasterVol);
        PlayerPrefs.SetFloat(MusicKey, currentMusicVol);
        PlayerPrefs.SetFloat(SfxKey, currentSfxVol);
        PlayerPrefs.Save();
    }

    private void SetMixerVolume(string param, float sliderValue)
    {
        float dB = Mathf.Log10(Mathf.Clamp(sliderValue, 0.0001f, 1f)) * 20f;
        audioMixer.SetFloat(param, dB);
    }

    private void OnDisable()
    {
        // revert UI toggles if changes were not applied
        UpdateToggleStates(currentQuality);
    }
}
