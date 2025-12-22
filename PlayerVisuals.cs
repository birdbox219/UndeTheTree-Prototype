using UnityEngine;
using UnityEngine.VFX;

/// <summary>
/// Manages the visual representation (Animation, Sprites, SFX) for the "Salma" character.
/// Handles visibility toggling when switching characters.
/// </summary>
public class PlayerVisuals : MonoBehaviour
{
    private Player player;
    private Animator animator;
    private SpriteRenderer sr;
    //[SerializeField] ParticleSystem switchParticals;

    [Header("Audio Settings")]
    [SerializeField] private SoundType sound;
    [SerializeField] private SoundType sound1;
    [SerializeField] private float volume = 1f;

    private void Awake()
    {

        player = GetComponentInParent<Player>();
        animator = GetComponent<Animator>();
        animator.SetBool("IsSprinting", false);
        sr = GetComponent<SpriteRenderer>();
        

    }
    private void Start()
    {
        // Subscribe to player events to trigger animations/sounds
        player.OnSpriting += Player_OnSpriting; ;
        player.OnJump += Player_OnJump;
        player.OnDoubleJumpSalma += Player_OnDoubleJumpSalma;
        player.OnCharachterChnaged += Player_OnCharachterChnaged;
        
        Hide();
    }

    private void Player_OnCharachterChnaged(object sender, System.EventArgs e)
    {
        Debug.Log($"Character changed: IsSalmaActive = {player.IsSalmaActive}");
        if (player.IsSalmaActive)
        {
            Debug.Log("Showing Salma Visuals");
            Show();
            animator.SetTrigger("Switch");

            //if (switchParticals != null)
            //    switchParticals.Play();
        }
        else
        {
            Hide();
        }
    }

    private void Player_OnDoubleJumpSalma(object sender, System.EventArgs e)
    {
        animator.SetTrigger("Doublejumping");
        animator.ResetTrigger("Switch");

        if(Player.Instance.IsSalmaActive)
        {
            SoundManager.PlaySound(sound1, volume);
        }
        

    }

    private void Player_OnJump(object sender, System.EventArgs e)
    {
        animator.SetTrigger("IsJumpingTrigger");


        if (Player.Instance.IsSalmaActive)
            SoundManager.PlaySound(sound, volume);
    }

    private void Player_OnSpriting(object sender, System.EventArgs e)
    {
        animator.SetBool("IsSprinting", player.IsMoving);
        animator.speed = player.IsMoving ? 1.5f : 1f;

    }

    private void Update()
    {
        FlipCharachter();
        //IsJumping();
    }


    private void Show()
    {
        //sr.enabled = true;
        sr.enabled = true;

    }


    private void Hide()
    {
                 //sr.enabled = false;
        sr.enabled = false;
    }


    /// <summary>
    /// Flips the sprite on the X axis based on movement direction.
    /// </summary>
    private void FlipCharachter()
    {
        float horizontalVelocity = player.HorizontalVelocity;

        if (horizontalVelocity > 0.05f)
        {
            sr.flipX = false;
        }
        else if (horizontalVelocity < -0.05f)
        {
            sr.flipX = true;
        }
    }

    //private void IsJumping()
    //{
    //    if(player.PlayerIsGrounded())
    //    {
    //        animator.SetBool("IsJumping", false);
    //    }
    //    else
    //    {
    //        animator.SetBool("IsJumping", true);
    //    }
    //}
}