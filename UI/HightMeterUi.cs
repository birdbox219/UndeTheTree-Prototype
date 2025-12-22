using TMPro;
using UnityEngine;

/// <summary>
/// Displays the player's current vertical progress (height) on the HUD.
/// Converts raw Unity units into a "meters" display string.
/// </summary>
public class HeightMeterUI : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI heightMeter;

    [Header("Scale Settings")]
    [SerializeField] private float metersPerUnit = 1f;
    [SerializeField] private float groundYOffset = 1f;

    private void Update()
    {
        CalculateHeightUI();
    }

    /// <summary>
    /// Calculates the player's height relative to the starting offset and updates the text.
    /// </summary>
    private void CalculateHeightUI()
    {
        float playerY = Player.Instance.GetPlayerPositionY();

        // Convert Unity units -> virtual "meters"
        float meters = (playerY - groundYOffset) * metersPerUnit;

        // Ensure we don't show negative height
        meters = Mathf.Max(0f, meters);

        heightMeter.text = Mathf.FloorToInt(meters) + " m";
    }
}