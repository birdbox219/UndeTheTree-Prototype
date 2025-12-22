using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// Wrapper class for Unity's Input System.
/// Provides a unified, easy-to-access interface for all game inputs (Movement, Jump, Switch, Pause).
/// Acts as a Singleton.
/// </summary>
public class GameInput : MonoBehaviour
{
    /// <summary>
    /// Singleton instance for global access.
    /// </summary>
    public static GameInput Instance { get; private set; }

    // Reference to the generated Input Action C# class
    private InputSystem_Actions inputActions;

    private void Awake()
    {
        //Instance = this;
        //inputActions = new InputSystem_Actions();
        //inputActions.Player.Enable();


        // Singleton protection to ensure only one input manager exists
        if (Instance != null)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        //DontDestroyOnLoad(gameObject);

        inputActions = new InputSystem_Actions();
    }

    private void OnEnable()
    {
        inputActions.Player.Enable();
    }

    private void OnDisable()
    {
        inputActions.Player.Disable();
    }

    private void OnDestroy()
    {
        inputActions.Player.Disable(); // safety check
    }




    /// <summary>
    /// Returns the normalized movement vector (Vector2) from player input.
    /// Used for character movement.
    /// </summary>
    public Vector2 GetMovementVectorNormalized()
    {
        Vector2 inputVector = inputActions.Player.Move.ReadValue<Vector2>();
        inputVector = inputVector.normalized;
        return inputVector;
    }

    /// <summary>
    /// Returns true if the jump button was pressed this frame.
    /// </summary>
    public bool IsJumpPressed()
    {
        return inputActions.Player.Jump.triggered;
    }

    /// <summary>
    /// Returns true if the jump button is currently being held down.
    /// Useful for variable jump height.
    /// </summary>
    public bool IsJumpHeld()
    {
        //return inputActions.Player.Jump.ReadValue<float>() > 0;
        return inputActions.Player.Jump.IsPressed();
    }

    /// <summary>
    /// Returns true if the jump button was released this frame.
    /// Used to cut the jump short (variable jump height).
    /// </summary>
    public bool IsJumpReleased()
    {
        return inputActions.Player.Jump.WasReleasedThisFrame();
    }

    /// <summary>
    /// Returns true if the "Interact" (Character Switch) button was pressed this frame.
    /// </summary>
    public bool IsSwitchPressed()
    {
        return inputActions.Player.Interact.triggered;
    }

    /// <summary>
    /// Returns true if the "Pause" (Escape) button was pressed this frame.
    /// </summary>
    public bool IsPausePressed()
    {
        return inputActions.Player.ESCAPE.triggered;
    }

}