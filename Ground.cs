using UnityEngine;
using System.Collections;

/// <summary>
/// Controls platform behaviors including moving platforms and falling platforms.
/// Handles detecting the player specifically when they stand ON TOP of the platform.
/// Manages parenting the player to moving platforms for smooth movement.
/// </summary>
[RequireComponent(typeof(BoxCollider2D))] // We need the collider to know how big the platform is
public class Ground : MonoBehaviour
{
    public enum PlatformType { Normal, Moving, Falling }

    [Header("Main Settings")]
    [SerializeField] private PlatformType type = PlatformType.Normal;

    [Header("Sensor Sensitivity")]
    [Tooltip("How high above the platform the feet can be and still count as touching")]
    [SerializeField] private float detectionHeight = 0.2f;

    [Header("Moving Settings")]
    [SerializeField] private float moveRangeY = 2f;
    [SerializeField] private float moveSpeed = 2f;
    [SerializeField] private bool startMovingUp = true;

    [Header("Falling Settings")]
    [SerializeField] private float fallDelay = 0.5f;
    [SerializeField] private float destroyDelay = 2f;
    [SerializeField] private float shakeAmount = 0.05f;

    // --- Private Variables ---
    private Vector3 startPos;
    private Rigidbody2D rb; // Optional if you don't need physics falling, but good for Falling type
    private BoxCollider2D boxCollider;
    private bool isFalling = false;
    private float directionMultiplier;

    // Track if we have already parented the player
    private bool playerIsAttached = false;

    void Start()
    {
        startPos = transform.position;
        boxCollider = GetComponent<BoxCollider2D>();

        // If you are using Falling platforms, you need a Rigidbody
        rb = GetComponent<Rigidbody2D>();
        if (rb != null)
        {
            rb.bodyType = RigidbodyType2D.Kinematic;
            rb.constraints = RigidbodyConstraints2D.FreezeRotation;
        }

        directionMultiplier = startMovingUp ? 1f : -1f;
    }

    void Update()
    {
        // 1. Moving Logic
        if (type == PlatformType.Moving && !isFalling)
        {
            MovePlatform();
        }

        // 2. Sensor Logic (Every Frame)
        CheckForPlayerFeet();
    }

    /// <summary>
    /// Oscillates the platform up and down using a sine wave.
    /// </summary>
    private void MovePlatform()
    {
        float offset = Mathf.Sin(Time.time * moveSpeed) * moveRangeY * directionMultiplier;
        transform.position = new Vector3(startPos.x, startPos.y + offset, startPos.z);
    }

    /// <summary>
    /// Custom collision detection to check if the player is standing on top of this platform.
    /// Used instead of standard OnCollisionEnter to allow precise "feet" detection without physics glitches.
    /// </summary>
    private void CheckForPlayerFeet()
    {
        if (Player.Instance == null) return;

        // A. Get the exact position of the Player's Ground Check
        Vector3 playerFeetPos = Player.Instance.GetGroundCheckPosition();

        // B. Get the boundaries of THIS platform
        Bounds bounds = boxCollider.bounds;

        // C. Check Width: Are the feet within the Left and Right edges?
        bool isWithinWidth = (playerFeetPos.x >= bounds.min.x) && (playerFeetPos.x <= bounds.max.x);

        // D. Check Height: Are the feet resting on top?
        // We check if feet are between the Top Edge and (Top Edge + detectionHeight)
        bool isOnTop = (playerFeetPos.y >= bounds.max.y - 0.05f) && (playerFeetPos.y <= bounds.max.y + detectionHeight);

        // COMBINE: Player is "Touching" if width and height match
        if (isWithinWidth && isOnTop)
        {
            HandlePlayerContact();
        }
        else
        {
            HandlePlayerExit();
        }
    }

    /// <summary>
    /// Called when the player successfully stands on the platform.
    /// Handles parenting (for moving) or triggering the fall (for falling).
    /// </summary>
    private void HandlePlayerContact()
    {
        // 1. Moving Platform: Stick Player
        // Parenting ensures the player moves WITH the platform automatically
        if (type == PlatformType.Moving && !playerIsAttached)
        {
            Player.Instance.transform.SetParent(this.transform);
            playerIsAttached = true;
        }

        // 2. Falling Platform: Trigger Fall
        if (type == PlatformType.Falling && !isFalling)
        {
            StartCoroutine(FallRoutine());
        }
    }

    /// <summary>
    /// Called when the player leaves the platform's top surface.
    /// Unparents the player.
    /// </summary>
    private void HandlePlayerExit()
    {
        // If we were attached, un-attach now
        if (type == PlatformType.Moving && playerIsAttached)
        {
            Player.Instance.transform.SetParent(null);
            playerIsAttached = false;
        }
    }

    // --- Falling Logic ---
    /// <summary>
    /// Shakes the platform briefly, then enables gravity to make it fall.
    /// </summary>
    IEnumerator FallRoutine()
    {
        isFalling = true;
        float timer = 0f;

        while (timer < fallDelay)
        {
            // Shake
            transform.position = startPos + (Vector3)(Random.insideUnitCircle * shakeAmount);
            timer += Time.deltaTime;
            yield return null;
        }

        // Unparent player immediately before falling so they don't fall with it awkwardly
        if (playerIsAttached)
        {
            Player.Instance.transform.SetParent(null);
            playerIsAttached = false;
        }

        // Enable Gravity
        if (rb != null)
        {
            rb.bodyType = RigidbodyType2D.Dynamic;
            rb.gravityScale = 2.5f;
        }

        // Disable collider so player falls through
        boxCollider.enabled = false;

        Destroy(gameObject, destroyDelay);
    }

    // Debug Visualization to see the zone
    private void OnDrawGizmos()
    {
        if (GetComponent<BoxCollider2D>() != null)
        {
            Bounds b = GetComponent<BoxCollider2D>().bounds;
            Gizmos.color = Color.yellow;
            // Draw a line representing the detection zone on top
            Vector3 topLeft = new Vector3(b.min.x, b.max.y + detectionHeight, 0);
            Vector3 topRight = new Vector3(b.max.x, b.max.y + detectionHeight, 0);
            Vector3 bottomLeft = new Vector3(b.min.x, b.max.y, 0);
            Vector3 bottomRight = new Vector3(b.max.x, b.max.y, 0);

            Gizmos.DrawLine(bottomLeft, topLeft);
            Gizmos.DrawLine(topLeft, topRight);
            Gizmos.DrawLine(topRight, bottomRight);
            Gizmos.DrawLine(bottomRight, bottomLeft);
        }
    }
}