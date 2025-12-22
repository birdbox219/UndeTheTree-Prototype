using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

/// <summary>
/// Controls the visual feedback for the "Fire Danger" mechanic.
/// Modifies Post-Processing effects (Color, Bloom, Vignette, White Balance) dynamically 
/// based on the danger level received from the GameManager.
/// </summary>
public class FireDamageController : MonoBehaviour
{

    //[SerializeField , Range(0f , 1f)] private float testDangerLevel = 0f;

    private Volume targetVolume;

    // Post-Processing Overrides
    private ColorAdjustments color;
    private Bloom bloom;
    private Vignette vignette;
    private WhiteBalance whiteBalance;

    



    private void Awake()
    {
        //targetVolume = GetComponent<Volume>();
        //targetVolume.profile.TryGet<ColorAdjustments>(out color);
        //targetVolume.profile.TryGet<Bloom>(out bloom);
        //targetVolume.profile.TryGet<Vignette>(out vignette);
        //targetVolume.profile.TryGet<WhiteBalance>(out whiteBalance);



        if (targetVolume == null)
            targetVolume = GetComponent<Volume>();

        // Validation to ensure Volume component exists
        if (targetVolume == null)
        {
            Debug.LogError($"[FireDamageController] CRITICAL: No 'Volume' component found on {gameObject.name}!");
            enabled = false; // Disable script to prevent errors
            return;
        }

        if (targetVolume.profile == null)
        {
            Debug.LogError($"[FireDamageController] CRITICAL: The Volume on {gameObject.name} has no Profile assigned!");
            enabled = false;
            return;
        }

        // 2. LOGGING: Check which overrides exist in the profile
        // We use if(!TryGet) to warn if something is missing.
        // Retrieves references to the specific post-processing overrides

        if (!targetVolume.profile.TryGet(out color))
        {
            Debug.LogWarning($"[FireDamageController] Warning: 'Color Adjustments' is missing from the Volume Profile.");
        }

        if (!targetVolume.profile.TryGet(out whiteBalance))
        {
            Debug.LogWarning($"[FireDamageController] Warning: 'White Balance' is missing. (You need this for Temperature changes).");
        }

        if (!targetVolume.profile.TryGet(out bloom))
        {
            Debug.LogWarning($"[FireDamageController] Warning: 'Bloom' is missing from the Volume Profile.");
        }

        if (!targetVolume.profile.TryGet(out vignette))
        {
            Debug.LogWarning($"[FireDamageController] Warning: 'Vignette' is missing from the Volume Profile.");
        }
    }


    private void Update()
    {
       // Continuously update visuals based on current fire danger
       ApplyViuslas(GameManager.Instance.GetFireDangerLevel());
    }


    /// <summary>
    /// Interpolates post-processing values based on the danger intensity (0 to 1).
    /// </summary>
    /// <param name="danger">Normalized value where 0 is safe and 1 is maximum danger.</param>
    private void ApplyViuslas(float danger)
    {
        //whiteBalance.temperature.value = Mathf.Lerp(0f, 25f, danger);

        //// Color Grading
        //color.colorFilter.value = Color.Lerp(Color.white, new Color(1f, 0.6f, 0.4f), danger);

        //// Bloom
        //bloom.intensity.value = Mathf.Lerp(0.5f, 3f, danger);

        //// Vignette (pressure)
        //vignette.intensity.value = Mathf.Lerp(0.1f, 0.35f, danger);

        //Debug.Assert(danger >= 0f && danger <= 1f, "Danger level out of bounds: " + danger);
        //Debug.Log("Applying Visuals - Danger Level: " + danger);


        // Temperature (Must use WhiteBalance, not ColorAdjustments)
        // Increases 'heat' feel by shifting temperature
        if (whiteBalance != null)
        {
            whiteBalance.temperature.value = Mathf.Lerp(0f, 25f, danger);
        }

        // Color Filter (Fixed the static Color.Lerp error)
        // Tints the screen towards orange/red
        if (color != null)
        {
            // Note: Use Color.Lerp (static), not colorAdjustments.Lerp
            color.colorFilter.value = Color.Lerp(Color.white, new Color(1f, 0.6f, 0.4f), danger);
        }

        // Bloom
        // Increases glow intensity
        if (bloom != null)
        {
            bloom.intensity.value = Mathf.Lerp(0.5f, 3f, danger);
        }

        // Vignette
        // Darkens edges to create tunnel vision/pressure
        if (vignette != null)
        {
            vignette.intensity.value = Mathf.Lerp(0.1f, 0.35f, danger);
        }
    }









}