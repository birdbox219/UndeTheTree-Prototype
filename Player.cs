using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.UIElements;

/// <summary>
/// Controls the player character, handling movement, jumping, and the unique mechanics of switching between two characters (Saleem and Salma).
/// Manages physical interactions, state transitions, and safety checks for respawning.
/// </summary>
public class Player : MonoBehaviour
{
    /// <summary>
    /// Singleton instance for global access to the player.
    /// </summary>
    public static Player Instance { get; private set; }



    //Events
    public event EventHandler OnSpriting;
    public event EventHandler OnJump;
    public event EventHandler OnDoubleJumpSalma;
    public event EventHandler OnCharachterChnaged;
    public event EventHandler OnPlayerDied;
    public event EventHandler OnPausePressed;

    // Character survival states
    public bool IsSalmaAlive { get; private set; } = true;
    public bool IsSaleemAlive { get; private set; } = true;


    [Header("Character1Seleem")]
    [SerializeField] private float speed;
    [SerializeField] private int maxJumps = 1;

    [Header("Character2Salma")]
    [SerializeField] private float speed2;
    [SerializeField] private int maxJumps2 = 2;


    [Header("MovmentTouch")]
    [SerializeField] private float acceleration ; 
    [SerializeField] private float deceleration ;


    [SerializeField] private float airAcceleration ;
    [SerializeField] private float airDeceleration;

    [Header("Shared Settings")]
    [SerializeField] private Color char1Color = Color.red;
    [SerializeField] private Color char2Color = Color.blue;

    [Header("Jump Settings")]
    [SerializeField] private float jumpHeight;
    private float coyoteTime = 0.2f; // Time after leaving ground where jump is still valid
    private float coyoteTimeCounter;

    private float jumpBufferTime = 0.2f; // Time before landing where jump input is registered
    private float jumpBufferCounter;
    private float lastSwitchTime = -10f;




    [SerializeField] private float jumpCutMultiplier = 0.5f; // Multiplier for jump height when button is released early

    [Header("Combo Feel")]
    [SerializeField] private float switchGracePeriod = 1f;

    [Header("Constrains")]
    [SerializeField] private float gravity = 4;

    [Header("Ground Check")]
    [SerializeField] private Transform groundCheckPoint;
    [SerializeField] private float groundCheckRadius = 0.6f;
    [SerializeField] private LayerMask groundLayer;



    [Header("ParticalSettings")]
    [SerializeField] private List<ParticleSystem> switchParticals;


    [Header("DeathSettings")]
    [SerializeField] private float switchLockDuration = 3f;

    [Header("LastSafeSpawnPostion")]
    private float safetyCheckWidth = 2f;


    private float safetyCheckDistance = 1.0f;


    private float maxSafeSlopeAngle = 45f;

    // Separate LayerMask to prevent respawning on hazards or enemies
    //private LayerMask safeGroundLayer;




    private enum ParticalType
    {
        SwitchToSalma,
        SwitchToSeleem
    }





    private Rigidbody2D body;
    private SpriteRenderer sr;
    private RespawnFlcikerController RespawnFlcikerController;

    //Character State
    //Character 1 = Saleem | Character 2 = Salma
    private bool isCharacter1Active = true;
    private bool canSwitchCharacterInput = true;
    private Coroutine switchLockCoroutine;
    private int jumpUsed = 0;

    private bool isGrounded = true;
    private bool wasGroundedLastFrame;



    private float currentSpeed;
    private int currentMaxJumps;

    private bool wasMoving = false;
    private Vector3 lastSafeGroundPosition;





    private void Awake()
    {
        Instance = this;

        body = GetComponent<Rigidbody2D>();
        body.gravityScale = gravity;
        sr = GetComponent<SpriteRenderer>();
        RespawnFlcikerController = GetComponent<RespawnFlcikerController>();
        //Debug.Log("Gravity set to: " + gravity);
    }



    private void Start()
    {
        Debug.Log("Game Started");
        UpdateCharachter();
        


    }

    

    private void Update()
    {
        // Check if we are within the grace period after switching characters
        bool isInGracePeriod = (Time.time - lastSwitchTime < switchGracePeriod);

        // Handle Character Switch Input
        if (GameInput.Instance.IsSwitchPressed())
        {

            if (!canSwitchCharacterInput)
            {
                Debug.Log("Switch Input Ignored - Lock Active");
                return;
            }


            isCharacter1Active = !isCharacter1Active;
            Debug.Log("Switching Character");

            //if (!CanSwitchCharacter())
            //{
            //    // Revert the switch if we can't switch
            //    isCharacter1Active = !isCharacter1Active;
            //    Debug.Log("Cannot Switch Character - Switch Reverted");
            //    return;
            //}
            UpdateCharachter();
            if (isCharacter1Active)
                {
                // Play switch to seleem particals
                PlayeParticals(ParticalType.SwitchToSeleem);
            }
            else
            {
                // Play switch to salma particals
                PlayeParticals(ParticalType.SwitchToSalma);
            }


            lastSwitchTime = Time.time;
            Debug.Log("Switch Grace Period Started");
        }

        // Handle Pause Input
        if(GameInput.Instance.IsPausePressed())
        {
            OnPausePressed?.Invoke(this, EventArgs.Empty);  
        }

        // Ground check logic
        bool wasGrounded = isGrounded;
        isGrounded = isPlayerColidedWithObstacle(groundLayer);
        //Debug.Log("Is gorunded" + isGrounded);
        //Debug.Log("Was gorunded" + wasGrounded);

        // Update safe spawn position only if grounded and not in a transition
        if (isGrounded && !GameManager.Instance.isGameFading() )
        {
            SetLastSafeGrounededPostion();
        }

        // Reset jumps if we just landed
        if (isGrounded && !wasGrounded)
        {
            jumpUsed = 0;
            //Debug.Log("Landed: Jumps Reset");
        }

        // Handle Coyote Time (allows jumping shortly after walking off a ledge)
        if (isGrounded)
        {
            coyoteTimeCounter = coyoteTime;
        }
        else
        {
            coyoteTimeCounter -= Time.deltaTime;
            //Debug.Log("Coyote Time Counter: " + coyoteTimeCounter);
        }

        


        if (GameInput.Instance.IsJumpPressed())
        {
            jumpBufferCounter = jumpBufferTime;
        }
        else
        {
            jumpBufferCounter -= Time.deltaTime;
        }
        // If falling and coyote time is gone, consume the 1st jump.
        // This allows Salma to use her 2nd jump (because jumpUsed > 0 becomes true),
        // but stops Seleem (because jumpUsed < maxJumps becomes false).
        if (!isGrounded && coyoteTimeCounter < 0f && jumpUsed == 0)
        {
            jumpUsed = 1;
        }

        HandleMovment();

        //Debug.Log("Jump used" + jumpUsed + " / " + currentMaxJumps);
        
        // Handle Jump Execution
        if (jumpBufferCounter > 0f && jumpUsed < currentMaxJumps && (isGrounded || coyoteTimeCounter > 0f || isInGracePeriod || jumpUsed > 0))
        {
            // Trigger double jump event for Salma animation/effects
            if(!isGrounded && !wasGrounded && jumpUsed == 1 && !isCharacter1Active)
            {
                OnDoubleJumpSalma?.Invoke(this, EventArgs.Empty);
            }

            PefromJump();
        }

        


        // Handle Variable Jump Height (holding button jumps higher)
        if (GameInput.Instance.IsJumpReleased() && body.linearVelocity.y > 0f)
        {

            if(!isInGracePeriod)
            {
                body.linearVelocity = new Vector2(body.linearVelocity.x, body.linearVelocity.y * jumpCutMultiplier);
            }

            else
            {
                Debug.Log("Jump Cut Skipped due to Switch Combo");
            }
            
            
        }
        wasGroundedLastFrame = isGrounded;




        //PrintSalmaActive();




    }




    
    /// <summary>
    /// Updates internal stats (speed, max jumps) based on the currently active character.
    /// </summary>
    private void UpdateCharachter()
    {
        if (isCharacter1Active)
        {
            currentSpeed = speed;
            currentMaxJumps = maxJumps;
            //sr.color = char1Color;
            Debug.Log("Switched to seleem");
        }
        else
        {
            currentSpeed = speed2;
            currentMaxJumps = maxJumps2;
            //sr.color = char2Color;
            Debug.Log("Switched to salma");
        }
        OnCharachterChnaged?.Invoke(this, EventArgs.Empty);
    }

    /// <summary>
    /// Calculates and applies horizontal movement forces.
    /// Handles acceleration, deceleration, and air control.
    /// </summary>
    private void HandleMovment()
    {
        Vector2 movementVector = GameInput.Instance.GetMovementVectorNormalized();

        float trgetSpeedX = movementVector.x * currentSpeed;

        float currentAccel;
        float currentDecel;


        if (isGrounded)
        {
            currentAccel = acceleration;
            currentDecel = deceleration;
        }
        else
        {
            currentAccel = airAcceleration;
            currentDecel = airDeceleration;
        }

        bool isAccelerating = (Mathf.Abs(trgetSpeedX) > 0.01f);
        bool isTurning = Mathf.Abs(body.linearVelocity.x) > 0.01f && Mathf.Sign(trgetSpeedX) != Mathf.Sign(body.linearVelocity.x);


        float speedChnageRate;

        if (isAccelerating && !isTurning)
        {
            speedChnageRate = currentAccel;
        }
        else
        {
            speedChnageRate = currentDecel;
        }

        float newSpeedX = Mathf.MoveTowards(body.linearVelocity.x, trgetSpeedX, speedChnageRate * Time.deltaTime);
        body.linearVelocity = new Vector2(newSpeedX, body.linearVelocity.y);

        // Spriting Event
        bool isMoving = Mathf.Abs(body.linearVelocity.x) > 0.1f;

        if (isMoving != wasMoving)
        {
            OnSpriting?.Invoke(this, EventArgs.Empty);
            wasMoving = isMoving;
        }

        //Debug.Log("Target Speed X: " + trgetSpeedX + " | Current Speed X: " + body.linearVelocity.x + " | New Speed X: " + newSpeedX);
    }

    /// <summary>
    /// Executes the physical jump force.
    /// </summary>
    private void PefromJump()
    {
        float jumpVelocity = CalculateJumpVelocity(jumpHeight);
        body.linearVelocity = new Vector2(body.linearVelocity.x, jumpVelocity);
        //isGrounded = false;
        jumpBufferCounter = 0f;
        Debug.Log("Jump Performed");
        coyoteTimeCounter = 0f;
        Debug.Log("Coyote Time Counter reset to 0");
        jumpUsed++;
        OnJump?.Invoke(this, EventArgs.Empty);
    }

    /// <summary>
    /// Calculates the required vertical velocity to reach a specific height.
    /// </summary>
    private float CalculateJumpVelocity(float jumpHeight)
    {
        return Mathf.Sqrt(2 * jumpHeight *  Mathf.Abs(Physics2D.gravity.y) * body.gravityScale);
    }

    private void OnDrawGizmosSelected()
    {
        if (groundCheckPoint != null)
        {
            Gizmos.color = Color.blue;
            Gizmos.DrawWireSphere(groundCheckPoint.position, groundCheckRadius);

            Gizmos.color = Color.red;
            Gizmos.DrawLine(
                groundCheckPoint.position,
                groundCheckPoint.position + Vector3.left * 1f
            );

            Gizmos.color = Color.green;
            Gizmos.DrawLine(
                groundCheckPoint.position,
                groundCheckPoint.position + Vector3.right * 1f
            );

        }
    }

    /// <summary>
    /// Checks for collision with specific layers (e.g., ground) using a circle overlap.
    /// </summary>
    public bool isPlayerColidedWithObstacle(LayerMask layerMask)
    {
        
        return Physics2D.OverlapCircle(groundCheckPoint.position, groundCheckRadius, layerMask); 
    }

    public bool IsMoving => Mathf.Abs(body.linearVelocity.x) > 0.1f;
    public float HorizontalVelocity => body.linearVelocity.x;

    

    public bool IsSaleemActive => isCharacter1Active;
    public bool IsSalmaActive => !isCharacter1Active;


    public void SetSaleemAliveStatus(bool isAlive)
    {
        IsSaleemAlive = isAlive;
    }

    public void SetSalmaAliveStatus(bool isAlive)
    {
        IsSalmaAlive = isAlive;
    }


    

    public bool PlayerIsGrounded() => isGrounded;



    private void PlayeParticals(ParticalType particalType)
    {

        

        var ps = switchParticals[(int)particalType];

        if (!ps) // Unity-safe destroyed check
        {
            Debug.LogWarning($"Particle {particalType} was destroyed!");
            return;
        }

        //ps.gameObject.SetActive(true);
        ps.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
        Debug.Log(
    $"Particle {particalType} | Exists: {switchParticals[(int)particalType]} | Active: {switchParticals[(int)particalType].gameObject.activeInHierarchy}"
        );
        ps.Play();

        Debug.Log("Playing Particles: " + particalType);
    }

    public Vector3 GetSafeGroundPosition()
    {
        return lastSafeGroundPosition;
    }


    public float GetSwitchLockDuration()
    {
        return switchLockDuration;
    }

    public float GetPlayerPositionY()
    {
        return transform.position.y;
    }

    /// <summary>
    /// Validates and records the last safe position on the ground where the player can be respawned.
    /// Checks width and slope angle to ensure the spot is truly safe.
    /// </summary>
    private void SetLastSafeGrounededPostion()
    {

        Vector2 pos = transform.position;
        // Use groundCheckPoint as base height, but Player X for width spread
        float checkY = groundCheckPoint != null ? groundCheckPoint.position.y : pos.y - 0.5f;

        Vector2 originLeft = new Vector2(pos.x - safetyCheckWidth, checkY);
        Vector2 originRight = new Vector2(pos.x + safetyCheckWidth, checkY);

        // 2. Perform Raycasts (Left, Right, Center)
        // We use the specific safeGroundLayer to ignore spikes/enemies
        RaycastHit2D hitLeft = Physics2D.Raycast(originLeft, Vector2.down, safetyCheckDistance, groundLayer);
        RaycastHit2D hitRight = Physics2D.Raycast(originRight, Vector2.down, safetyCheckDistance, groundLayer);

        // Center check is useful for slope validation
        RaycastHit2D hitCenter = Physics2D.Raycast(new Vector2(pos.x, checkY), Vector2.down, safetyCheckDistance, groundLayer);

        // 3. Validation
        bool isLeftSafe = hitLeft.collider != null;
        bool isRightSafe = hitRight.collider != null;
        bool isCenterSafe = hitCenter.collider != null;

        if (isLeftSafe && isRightSafe && isCenterSafe)
        {
            // 4. Slope Validation 
            float slopeAngle = Vector2.Angle(hitCenter.normal, Vector2.up);
            if (slopeAngle <= maxSafeSlopeAngle)
            {
                // 5. Update Position
                // This is where we would add Moving Platform logic if needed
                lastSafeGroundPosition = pos;
            }
        }

    }
    
    /// <summary>
    /// Forces a character switch (usually upon death) and locks input temporarily.
    /// </summary>
    public void ForceSwitchCharacter()
    {
        



        if (IsSaleemActive && IsSalmaAlive)
        {
            isCharacter1Active = false; // switch to Salma

        }
        else if (IsSalmaActive && IsSaleemAlive)
        {
            isCharacter1Active = true; // switch to Saleem
        }

        UpdateCharachter();

        if(switchLockCoroutine != null)
        StopCoroutine(switchLockCoroutine);

        switchLockCoroutine = StartCoroutine(SwitchLockCoroutine());


    }


    /// <summary>
    /// Respawns the player at the last safe position as the other character.
    /// Triggers game over if both characters are dead.
    /// </summary>
    public void RespawnAsOtherCharacter()
    {
        // Move to last safe spot
        transform.position = lastSafeGroundPosition;
        body.linearVelocity = Vector2.zero;
        body.angularVelocity = 0f;

        // Try switching
        ForceSwitchCharacter();

        if (RespawnFlcikerController != null)
        {
            RespawnFlcikerController.StartRespawnFlicker();
        }
        else
        {
            Debug.LogError("RespawnFlickerController is NOT assigned!");
        }
        
        if (ChecKPlayerDied())
        {
            OnPlayerDied?.Invoke(this, EventArgs.Empty);
            GameManager.Instance.GameOver();
            Debug.Log(ChecKPlayerDied());
        }


    }


    /// <summary>
    /// Checks if both characters are effectively dead (not alive).
    /// </summary>
    private bool ChecKPlayerDied()
    {
        if (!IsSalmaAlive && !IsSaleemAlive)
        {
            return true;
        }
        else
        {
            return false;
        }
    }
    //Stupid function that caused bugs
    //private bool CanSwitchCharacter()
    //{
    //    if ((IsSaleemActive && IsSalmaAlive) || (IsSalmaActive && IsSaleemAlive))
    //    {
    //        return true;
    //    }
    //    else
    //    {
    //        return false;
    //    }
    //}


    /// <summary>
    /// Coroutine to lock character switching for a set duration.
    /// Used after a forced switch/respawn penalty.
    /// </summary>
    private IEnumerator SwitchLockCoroutine()
    {
        canSwitchCharacterInput = false;
        Debug.Log("Switching locked");

        yield return new WaitForSeconds(switchLockDuration);

        canSwitchCharacterInput = true;
        Debug.Log("Switching unlocked");

    }

    public Vector3 GetGroundCheckPosition()
    {
        if (groundCheckPoint != null)
        {
            return groundCheckPoint.position;
        }
        return transform.position; // Fallback just in case
    }





    private void PrintSalmaActive()
    {
        if(!isCharacter1Active)
        {
            Debug.Log("Salma is Active");
        }
    }

}