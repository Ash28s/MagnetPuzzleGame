using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.UI;

public class SettingsMenu : MonoBehaviour
{
    // Reference to the AudioMixer
    [SerializeField] private AudioMixer audioMixer;

    // UI Sliders for volume control
    [SerializeField] private Slider masterVolumeSlider;
    [SerializeField] private Slider musicVolumeSlider;
    [SerializeField] private Slider sfxVolumeSlider;

    // PlayerPrefs keys for saving volume settings
    private const string MasterVolumeKey = "MasterVolume";
    private const string MusicVolumeKey = "MusicVolume";
    private const string SFXVolumeKey = "SFXVolume";

    public GameObject settingPanel;

    void Start()
    {
        // Initialize sliders with saved values or defaults
        InitializeSlider(masterVolumeSlider, MasterVolumeKey, "MasterVolume", 0.75f);
        InitializeSlider(musicVolumeSlider, MusicVolumeKey, "MusicVolume", 0.75f);
        InitializeSlider(sfxVolumeSlider, SFXVolumeKey, "SFXVolume", 0.75f);
    }

    // Initialize a slider with saved value or default
    private void InitializeSlider(Slider slider, string playerPrefsKey, string mixerParameter, float defaultValue)
    {
        if (slider != null)
        {
            // Load saved volume or use default
            float savedVolume = PlayerPrefs.GetFloat(playerPrefsKey, defaultValue);
            slider.value = savedVolume;
            SetVolume(mixerParameter, savedVolume);

            // Add listener for real-time updates
            slider.onValueChanged.AddListener(value => SetVolume(mixerParameter, value));
        }
    }

    // Set volume for a specific AudioMixer parameter and save to PlayerPrefs
    public void SetVolume(string mixerParameter, float volume)
    {
        // Convert linear slider value (0 to 1) to logarithmic scale for AudioMixer (-80 to 0 dB)
        float dB = Mathf.Lerp(-80f, 0f, volume);
        audioMixer.SetFloat(mixerParameter, dB);

        // Save the volume setting
        PlayerPrefs.SetFloat(GetPlayerPrefsKey(mixerParameter), volume);
        PlayerPrefs.Save();
    }

    // Get PlayerPrefs key based on mixer parameter
    private string GetPlayerPrefsKey(string mixerParameter)
    {
        switch (mixerParameter)
        {
            case "MasterVolume": return MasterVolumeKey;
            case "MusicVolume": return MusicVolumeKey;
            case "SFXVolume": return SFXVolumeKey;
            default: return "";
        }
    }

    // Optional: Method to close settings (called from UI button)
    public void CloseSettings()
    {
        settingPanel.SetActive(false);
    }
}