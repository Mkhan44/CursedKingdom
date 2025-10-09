//Audio mixer settings aren't applied on startup in the SettingsManager script, this script ensures the last saved audio settings are applied.
using UnityEngine;
using UnityEngine.Audio;

public class AudioSettingsApplier : MonoBehaviour
{
    public AudioMixer audioMixer;
    private const string MasterKey = "MasterVol";
    private const string MusicKey = "MusicVol";
    private const string SfxKey = "SfxVol";

    private void Awake()
    {
        ApplySavedVolumes();
    }

    private void ApplySavedVolumes()
    {
        SetMixerVolume("MasterVol", PlayerPrefs.GetFloat(MasterKey, 1f));
        SetMixerVolume("MusicVol", PlayerPrefs.GetFloat(MusicKey, 1f));
        SetMixerVolume("SfxVol", PlayerPrefs.GetFloat(SfxKey, 1f));
    }

    private void SetMixerVolume(string param, float sliderValue)
    {
        float dB = Mathf.Log10(Mathf.Clamp(sliderValue, 0.0001f, 1f)) * 20f;
        audioMixer.SetFloat(param, dB);
    }
}