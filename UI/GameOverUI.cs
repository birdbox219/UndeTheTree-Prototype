using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

/// <summary>
/// Manages the Game Over screen interactions.
/// Handles restarting the level or returning to the main menu.
/// </summary>
public class GameOverUI : MonoBehaviour
{
    [Header("UI")]
    [SerializeField] private Button restartButton;
    [SerializeField] private Button exitButton;

    private void Awake()
    {
        restartButton.onClick.AddListener(OnRestartClicked);
        exitButton.onClick.AddListener(OnExitClicked);
    }

    private void Start()
    {
        // Safety: ensure the time scale is reset so buttons work and animations play
        Time.timeScale = 1f;
    }

    /* ===================== BUTTON ACTIONS ===================== */

    private void OnRestartClicked()
    {
        // Reset audio state
        //SoundManager.StopMusic();
        SoundManager.ResumeMusic();

        SceneManager.LoadScene(1); // Reload the main game scene
    }

    private void OnExitClicked()
    {
        // Stop gameplay music when leaving
        SoundManager.StopMusic();

        SceneManager.LoadScene(0); // Return to main menu
    }
}