using Unity.VisualScripting;
using UnityEngine;

/// <summary>
/// Manages the visual representation (Animation, Sprites, SFX) for the "Saleem" character.
/// Handles visibility toggling when switching characters.
/// </summary>
public class PlayerVisulasSaleem : MonoBehaviour
{
    private Player player;
    private Animator animator;
    private SpriteRenderer sr;
    private ParticleSystem ParticleSystem;

    [SerializeField] private SoundType sound;
    [SerializeField] private float volume = 1f;


    //[SerializeField] private ParticleSystem switchParticals;


    private void Awake()
    {
        player = GetComponentInParent<Player>();
        animator = GetComponent<Animator>();
        sr = GetComponent<SpriteRenderer>();
        ParticleSystem = GetComponent<ParticleSystem>();
    }


    void Start()
    {
        player.OnCharachterChnaged += Player_OnCharachterChnaged;
        player.OnSpriting += Player_OnSpriting; ;
        player.OnJump += Player_OnJump;
    }

    private void Player_OnJump(object sender, System.EventArgs e)
    {
        animator.SetTrigger("SaleemJump");

        if(Player.Instance.IsSaleemActive)
        {
            SoundManager.PlaySound(sound, volume);
        }
        

    }

    private void Player_OnSpriting(object sender, System.EventArgs e)
    {
        animator.SetBool("IsSaleemSprinting", player.IsMoving);
    }

    private void Player_OnCharachterChnaged(object sender, System.EventArgs e)
    {
        if (player.IsSaleemActive)
        {
            Show();
            //if (switchParticals != null)
            //    switchParticals.Play();

        }
        else
        {
            Hide();
        }
    }

    void Update()
    {
        FlipCharachter();
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
}