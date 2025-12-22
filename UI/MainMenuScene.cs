using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

/// <summary>
/// Manages the Main Menu scene.
/// Handles starting the game, quitting, and initial UI focus for controller support.
/// </summary>
public class MainMenuScene : MonoBehaviour
{
    [Header("UI")]
    [SerializeField] private Button startButton;
    [SerializeField] private Button quitButton;

    private void Awake()
    {
        startButton.onClick.AddListener(OnStartClicked);
        quitButton.onClick.AddListener(OnQuitClicked);
    }

    private void Start()
    {
        // Ensure normal time scale in menu (in case we came from a paused game)
        Time.timeScale = 1f;
        HighlightStartButton();
    }

    /// <summary>
    /// Forces the EventSystem to select the Start button.
    /// Crucial for gamepad/keyboard navigation.
    /// </summary>
    private void HighlightStartButton()
    {
        // Clear previous selection (important)
        EventSystem.current.SetSelectedGameObject(null);

        // Select Start button for controller
        EventSystem.current.SetSelectedGameObject(startButton.gameObject);
    }

    /* ===================== BUTTON ACTIONS ===================== */

    private void OnStartClicked()
    {
        // Optional: play click sound
        // SoundManager.PlaySound(SoundType.SwitchCharacter);

        SceneManager.LoadScene(1);
        SoundManager.ResumeMusic();
    }

    private void OnQuitClicked()
    {
        Debug.Log("Quit Game");

        Application.Quit();

#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#endif
    }
}