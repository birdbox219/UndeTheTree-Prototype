using System;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Manages the core game state, including pause/resume, game over conditions, 
/// fire danger calculations, and handling player death/respawn sequences.
/// Acts as a Singleton for global access.
/// </summary>
public class GameManager : MonoBehaviour
{
    // Events triggered when specific game states change
    public event EventHandler OnCollisonWithFadeInSensor;
    public event EventHandler OnGamePaused;
    public event EventHandler OnGameUnPaused;

    /// <summary>
    /// Singleton instance of the GameManager.
    /// </summary>
    public static GameManager Instance { get; private set; }

    [SerializeField] private Sensor sensor;

    [Header("Fire Danger Settings")]
    private float fireDanger;
    [SerializeField] private float maxFireDanger = 50f;
    [SerializeField] private AnimationCurve fireDangerCurve;

    [SerializeField] private float deathCheckDuration = 5f;

    // Coroutine to track how long the player has been below the safety sensor
    private Coroutine belowSensorCoroutine;
    private bool isBelowSensor = false;

    private bool isFading = false;
    private bool isPaused = false;

    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        isFading = false;
        // Subscribe to player pause input
        Player.Instance.OnPausePressed += Instance_OnPausePressed;
    }

    /// <summary>
    /// Handles the pause input event from the player.
    /// Toggles the pause state of the game.
    /// </summary>
    private void Instance_OnPausePressed(object sender, EventArgs e)
    {
        // Prevent pausing during critical sequences like fading
        if (isFading)
            return;

        if (!isPaused)
        {
            PauseTheGame();
            OnGamePaused?.Invoke(this, EventArgs.Empty);
        }
        else
        {
            ResumeTheGame();
            OnGameUnPaused?.Invoke(this, EventArgs.Empty);
        }
    }

    private void Update()
    {
        HandleBelowSensorCheck();

        // Check if the player has hit the fade-in sensor to trigger a scene transition or respawn effect
        if (sensor.HasBeenColidedWithFadeInSensor() && !isFading )
        {
            Debug.Log("Collided with FadeInSensor - Starting Sequence");

            isFading = true;
            OnCollisonWithFadeInSensor?.Invoke(this, EventArgs.Empty);
            StartCoroutine(LoseAfterDelay(1.0f)); // Delay to allow fade animation
        }

        // Check for direct collision with the dangerous sensor (fire/death zone)
        if (sensor.HasBeenColidedWith() && !isFading)
        {
            Lose();
        }

        ClacluateFireDangerLevel();
    }

    /// <summary>
    /// Triggers the "Lose" condition (character switch/respawn) after a specified delay.
    /// Useful for syncing with visual effects like fading.
    /// </summary>
    /// <param name="delay">Time in seconds to wait before triggering logic.</param>
    private IEnumerator LoseAfterDelay(float delay)
    {
        // Wait for the screen to go fully black or effect to complete
        yield return new WaitForSeconds(delay);

        // Now safe to teleport/switch characters
        Lose();

        // Reset the flag after a short delay so the game can continue
        // allowing for the "Fade In" to finish
        yield return new WaitForSeconds(1.0f);
        isFading = false;
    }

    /// <summary>
    /// Handles the immediate consequence of "dying" or failing a section.
    /// Typically respawns the player as the other character.
    /// </summary>
    private void Lose()
    {
        Debug.Log("SomeOneDied");
        Player.Instance.RespawnAsOtherCharacter();
    }

    /// <summary>
    /// Triggers a complete Game Over, stopping music and loading the Game Over scene.
    /// </summary>
    public void GameOver()
    {
        //Debug.LogError("Game Over!");
        SoundManager.StopMusic();
        SceneManager.LoadScene(2);
    }

    /// <summary>
    /// Calculates the current fire danger level based on the player's vertical distance from the sensor.
    /// </summary>
    private void ClacluateFireDangerLevel()
    {
        // Calculate distance: Positive if player is above, decreasing as player gets closer to sensor
        float verticalDistance = Player.Instance.GetPlayerPositionY() - sensor.GetSensorPostion() ;

        // Normalize danger: 1.0 is max danger (at sensor), 0.0 is safe (far away)
        fireDanger = Mathf.Clamp01(1f - (verticalDistance / maxFireDanger));
        //Debug.Log("Fire Danger: " + fireDanger + " distance" + verticalDistance);
    }

    /// <summary>
    /// Returns the current normalized fire danger level (0 to 1).
    /// </summary>
    public float GetFireDangerLevel()
    {
        return fireDanger;
    }

    /// <summary>
    /// Checks if a screen fade sequence is currently active.
    /// </summary>
    public bool isGameFading()
    {
        return isFading;
    }

    /// <summary>
    /// Pauses the game by setting time scale to 0.
    /// </summary>
    private void PauseTheGame()
    {
        isPaused = true;
        Time.timeScale = 0f;
        Debug.Log("Game Paused");
    }

    /// <summary>
    /// Resumes the game by restoring time scale to 1.
    /// </summary>
    private void ResumeTheGame()
    {
        isPaused = false;
        Time.timeScale = 1f;
        Debug.Log("Game Resumed");
    }

    /// <summary>
    /// Public method to resume the game, typically called from UI buttons.
    /// </summary>
    public void ResumeGameFromUI()
    {
        if (!isPaused)
            return;

        isPaused = false;
        Time.timeScale = 1f;

        Debug.Log("Game Resumed (UI)");
    }

    private bool IsPlayerBelowSensor()
    {
        return Player.Instance.GetPlayerPositionY() < sensor.GetSensorPostion();
    }

    /// <summary>
    /// Monitors if the player has fallen below the safety sensor.
    /// Starts a countdown to Game Over if they stay below too long.
    /// </summary>
    private void HandleBelowSensorCheck()
    {
        bool below = IsPlayerBelowSensor();

        // Player just went below sensor -> start timer
        if (below && !isBelowSensor)
        {
            isBelowSensor = true;
            belowSensorCoroutine = StartCoroutine(BelowSensorCountdown());
        }
        // Player escaped -> cancel timer
        else if (!below && isBelowSensor)
        {
            isBelowSensor = false;

            if (belowSensorCoroutine != null)
            {
                StopCoroutine(belowSensorCoroutine);
                belowSensorCoroutine = null;
            }
        }
    }

    /// <summary>
    /// Countdown timer that triggers Game Over if the player remains below the sensor threshold.
    /// </summary>
    private IEnumerator BelowSensorCountdown()
    {
        float elapsed = 0f;

        while (elapsed < deathCheckDuration)
        {
            // If player returns to safety, stop the countdown
            if (!IsPlayerBelowSensor())
                yield break;

            elapsed += Time.unscaledDeltaTime;
            yield return null;
        }

        // Still below after duration -> GAME OVER
        GameOver();
    }
}