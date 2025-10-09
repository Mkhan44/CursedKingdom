using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.Audio;
using System.Collections.Generic;
using System.Linq;

public class SettingsManager : MonoBehaviour
{
    [Header("Graphics")]
    public TMP_Dropdown qualityDropdown;
    public TMP_Dropdown resolutionDropdown;
    public TMP_Dropdown screenModeDropdown;

    [Header("Audio")]
    public AudioMixer audioMixer;
    public Slider masterSlider;
    public Slider musicSlider;
    public Slider sfxSlider;

    [Header("General")]
    public Button applyButton;

    private List<Resolution> uniqueResolutions;
    private int currentQuality;
    private int currentResolution;
    private int currentScreenMode;
    private float currentMasterVol;
    private float currentMusicVol;
    private float currentSfxVol;

    private const string QualityKey = "QualitySetting";
    private const string ResolutionKey = "ResolutionIndex";
    private const string ScreenModeKey = "ScreenMode";
    private const string MasterKey = "MasterVol";
    private const string MusicKey = "MusicVol";
    private const string SfxKey = "SfxVol";

    void OnEnable()
    {
        SetupDropdowns();
        LoadSettings();

        qualityDropdown.onValueChanged.RemoveAllListeners();
        resolutionDropdown.onValueChanged.RemoveAllListeners();
        screenModeDropdown.onValueChanged.RemoveAllListeners();
        masterSlider.onValueChanged.RemoveAllListeners();
        musicSlider.onValueChanged.RemoveAllListeners();
        sfxSlider.onValueChanged.RemoveAllListeners();
        applyButton.onClick.RemoveAllListeners();

        // Assign listeners (update pending values only)
        qualityDropdown.onValueChanged.AddListener(i => currentQuality = i);
        resolutionDropdown.onValueChanged.AddListener(i => currentResolution = i);
        screenModeDropdown.onValueChanged.AddListener(i => currentScreenMode = i);

        masterSlider.onValueChanged.AddListener(v => {
            currentMasterVol = v;
            SetMixerVolume("MasterVol", v); // preview live
        });

        musicSlider.onValueChanged.AddListener(v => {
            currentMusicVol = v;
            SetMixerVolume("MusicVol", v);
        });

        sfxSlider.onValueChanged.AddListener(v => {
            currentSfxVol = v;
            SetMixerVolume("SfxVol", v);
        });

        applyButton.onClick.AddListener(ApplySettings);
    }

    private void SetupDropdowns()
    {
        // Quality
        qualityDropdown.ClearOptions();
        qualityDropdown.AddOptions(new List<string>(QualitySettings.names));

        // Resolutions (unique width x height only)
        uniqueResolutions = Screen.resolutions
            .GroupBy(r => new { r.width, r.height })
            .Select(g => g.First())
            .OrderBy(r => r.width).ThenBy(r => r.height)
            .ToList();

        resolutionDropdown.ClearOptions();
        resolutionDropdown.AddOptions(
            uniqueResolutions.ConvertAll(r => $"{r.width} x {r.height}")
        );

        // Screen modes
        screenModeDropdown.ClearOptions();
        screenModeDropdown.AddOptions(new List<string> { "Fullscreen", "Borderless", "Windowed" });
    }

    public void LoadSettings()
    {
        // Graphics
        currentQuality = PlayerPrefs.GetInt(QualityKey, QualitySettings.GetQualityLevel());
        currentResolution = PlayerPrefs.GetInt(ResolutionKey, uniqueResolutions.Count - 1);
        currentScreenMode = PlayerPrefs.GetInt(ScreenModeKey, ScreenModeFromUnity(Screen.fullScreenMode));

        currentQuality = Mathf.Clamp(currentQuality, 0, QualitySettings.names.Length - 1);
        currentResolution = Mathf.Clamp(currentResolution, 0, uniqueResolutions.Count - 1);
        currentScreenMode = Mathf.Clamp(currentScreenMode, 0, screenModeDropdown.options.Count - 1);

        qualityDropdown.value = currentQuality;
        resolutionDropdown.value = currentResolution;
        screenModeDropdown.value = currentScreenMode;

        qualityDropdown.RefreshShownValue();
        resolutionDropdown.RefreshShownValue();
        screenModeDropdown.RefreshShownValue();

        // Audio
        currentMasterVol = PlayerPrefs.GetFloat(MasterKey, 1f);
        currentMusicVol = PlayerPrefs.GetFloat(MusicKey, 1f);
        currentSfxVol = PlayerPrefs.GetFloat(SfxKey, 1f);

        masterSlider.value = currentMasterVol;
        musicSlider.value = currentMusicVol;
        sfxSlider.value = currentSfxVol;

        // Apply to mixer immediately
        SetMixerVolume("MasterVol", currentMasterVol);
        SetMixerVolume("MusicVol", currentMusicVol);
        SetMixerVolume("SfxVol", currentSfxVol);
    }

    private void ApplySettings()
    {
        // Apply graphics
        QualitySettings.SetQualityLevel(currentQuality, true);

        var res = uniqueResolutions[currentResolution];
        var mode = UnityScreenMode(currentScreenMode);
        Screen.SetResolution(res.width, res.height, mode);

        PlayerPrefs.SetInt(QualityKey, currentQuality);
        PlayerPrefs.SetInt(ResolutionKey, currentResolution);
        PlayerPrefs.SetInt(ScreenModeKey, currentScreenMode);

        // Apply audio (set mixer values)
        SetMixerVolume("MasterVol", currentMasterVol);
        SetMixerVolume("MusicVol", currentMusicVol);
        SetMixerVolume("SfxVol", currentSfxVol);

        PlayerPrefs.SetFloat(MasterKey, currentMasterVol);
        PlayerPrefs.SetFloat(MusicKey, currentMusicVol);
        PlayerPrefs.SetFloat(SfxKey, currentSfxVol);

        PlayerPrefs.Save();
    }

    private void SetMixerVolume(string param, float sliderValue)
    {
        // Slider (0–1) mapped to decibels (-80dB to 0dB)
        float dB = Mathf.Log10(Mathf.Clamp(sliderValue, 0.0001f, 1f)) * 20f;
        audioMixer.SetFloat(param, dB);

        // The AudioSettingsApplier script located on AudioManager ensures this is set on game startup.
    }

    private int ScreenModeFromUnity(FullScreenMode mode) =>
        mode == FullScreenMode.ExclusiveFullScreen ? 0 :
        mode == FullScreenMode.FullScreenWindow ? 1 : 2;

    private FullScreenMode UnityScreenMode(int index) =>
        index == 0 ? FullScreenMode.ExclusiveFullScreen :
        index == 1 ? FullScreenMode.FullScreenWindow :
        FullScreenMode.Windowed;
    private void OnDisable()
    {
        LoadSettings();
    }

}