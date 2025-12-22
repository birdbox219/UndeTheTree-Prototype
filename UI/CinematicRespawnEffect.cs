using System.Collections;
using UnityEngine;

/// <summary>
/// Handles the "screen fade to black" effect when a significant event (like respawning or level transition) occurs.
/// Uses AnimationCurves for smooth, non-linear fading.
/// </summary>
public class CinematicRespawnEffect : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private CanvasGroup fadeGroup;

    [Header("Polish Settings")]
    [SerializeField] float fadeDuration = 1.0f;
    [SerializeField] float blackScreenHoldTime = 0.5f;

    // SEPARATE CURVES
    [Header("Animation Curves")]
    // Curve for Clear -> Black (0 to 1)
    [SerializeField] AnimationCurve fadeOutCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);
    // Curve for Black -> Clear (1 to 0)
    [SerializeField] AnimationCurve fadeInCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);



    private Coroutine respawnCoroutine;


    private void Start()
    {
        GameManager.Instance.OnCollisonWithFadeInSensor += Instance_OnCollisonWithFadeInSensor;
    }

    private void Instance_OnCollisonWithFadeInSensor(object sender, System.EventArgs e)
    {
        TriggerRespawn();

    }

    public void TriggerRespawn()
    {
        StartCoroutine(RespawnSequence());
    }

    /// <summary>
    /// Executes the full fade sequence: Fade Out -> Wait -> Fade In.
    /// Used to hide the camera cut or character teleportation.
    /// </summary>
    private IEnumerator RespawnSequence()
    {
        // 1. FADE OUT (To Black)
        // Uses fadeOutCurve
        float timer = 0f;
        while (timer < fadeDuration)
        {
            timer += Time.unscaledDeltaTime;
            float percentage = timer / fadeDuration;

            fadeGroup.alpha = fadeOutCurve.Evaluate(percentage);

            yield return null;
        }
        fadeGroup.alpha = 1;

        // 2. WAIT (Pacing)
        yield return new WaitForSeconds(blackScreenHoldTime);

        // 3. FADE IN (Back to Game)
        // Uses fadeInCurve
        timer = 0f;
        while (timer < fadeDuration)
        {
            timer += Time.unscaledDeltaTime;
            float percentage = timer / fadeDuration;

            // Notice we use the NEW curve here. 
            // We still use (1 - percentage) so you can draw the curve from 0 to 1 in the editor
            fadeGroup.alpha = fadeInCurve.Evaluate(1 - percentage);

            yield return null;
        }
        fadeGroup.alpha = 0;
    }
}