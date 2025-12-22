using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

/// <summary>
/// Manages the in-game Pause Menu.
/// Handles resuming, exiting, and adjusting volume settings (Music/SFX).
/// Listens to GameManager events to toggle visibility automatically.
/// </summary>
public class PauseMenuUI : MonoBehaviour
{
    [Header("UI")]
    
    [SerializeField] private Button resumeButton;
    [SerializeField] private Button exitButton;
    [SerializeField] private Slider musicVolumeSlider;
    [SerializeField] private Slider sfxVolumeSlider;

    private void Awake()
    {
        // Button bindings
        resumeButton.onClick.AddListener(OnResumeClicked);
        exitButton.onClick.AddListener(OnExitClicked);

        // Slider bindings
        musicVolumeSlider.onValueChanged.AddListener(OnMusicVolumeChanged);
        sfxVolumeSlider.onValueChanged.AddListener(OnSFXVolumeChanged);
    }

    private void Start()
    {
        // Initialize slider values from SoundManager current state
        musicVolumeSlider.value = SoundManager.Instance != null
            ? SoundManager.Instance.InstanceMusicVolume
            : 0.5f;

        sfxVolumeSlider.value = SoundManager.Instance != null
            ? SoundManager.Instance.InstanceSFXVolume
            : 1f;


        GameManager.Instance.OnGamePaused += Instance_OnGamePaused;
        GameManager.Instance.OnGameUnPaused += Instance_OnGameUnPaused;

        Hide();
    }



    private void Instance_OnGameUnPaused(object sender, System.EventArgs e)
    {
        Hide();
    }

    private void Instance_OnGamePaused(object sender, System.EventArgs e)
    {
        Show();
    }

    /* ===================== UI CONTROL ===================== */

    private void Show()
    {
        gameObject.SetActive(true);
    }

    private void Hide()
    {
        gameObject.SetActive(false);
    }

    /* ===================== BUTTONS ===================== */

    private void OnResumeClicked()
    {
        GameManager.Instance.ResumeGameFromUI();
        Hide();
    }

    private void OnExitClicked()
    {
        // Ensure time is running before leaving scene
        Time.timeScale = 1f;
        SoundManager.StopMusic();
        SceneManager.LoadScene(0);
    }

    /* ===================== SLIDERS ===================== */

    private void OnMusicVolumeChanged(float value)
    {
        SoundManager.SetMusicVolume(value);
        PlayerPrefs.SetFloat("MusicVolume", value);
    }

    private void OnSFXVolumeChanged(float value)
    {
        SoundManager.SetSFXVolume(value);
        PlayerPrefs.SetFloat("SFXVolume", value);
    }
}