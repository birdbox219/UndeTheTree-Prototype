using System.Collections;
using UnityEngine;

/// <summary>
/// Controls the visual "flicker" effect when a character respawns.
/// Uses Material Property Blocks or direct material modification to toggle a shader effect.
/// </summary>
public class RespawnFlcikerController : MonoBehaviour
{
    [SerializeField] private Color _flickerColor = Color.white;
    [SerializeField] private float _flickerDuration = 1.5f;

    private SpriteRenderer[] _spriteRenderers;
    private Material[] _materials;

    // Shader property IDs for performance optimization
    private static readonly int FlickerActiveID = Shader.PropertyToID("_FlickerActive");
    private static readonly int FlashColorID = Shader.PropertyToID("_FlashColour");
    private void Awake()
    {
        _spriteRenderers = GetComponentsInChildren<SpriteRenderer>();
        Init();
    }


    private void Start()
    {

    }


    /// <summary>
    /// Initializes material references and sets default shader values.
    /// </summary>
    private void Init()
    {
        _materials = new Material[_spriteRenderers.Length];
        for (int i = 0; i < _spriteRenderers.Length; i++)
        {
            _materials[i] = _spriteRenderers[i].material;
            _materials[i].SetColor(FlashColorID, _flickerColor);
            _materials[i].SetFloat(FlickerActiveID, 0f);
        }
    }

    /// <summary>
    /// Coroutine that enables the flicker effect, waits, and then disables it.
    /// </summary>
    private IEnumerator RespawnFlicker()
    {

        // Enable effect on all child renderers
        for (int i = 0; i < _materials.Length; i++)
        {
            _materials[i].SetFloat(FlickerActiveID, 1f);
        }

        yield return new WaitForSeconds(_flickerDuration);

        // Disable flicker
        for (int i = 0; i < _materials.Length; i++)
        {
            _materials[i].SetFloat(FlickerActiveID, 0f);
        }
    }


    /// <summary>
    /// Public trigger to start the respawn visual effect.
    /// </summary>
    public void StartRespawnFlicker()
    {
        StopAllCoroutines();
        StartCoroutine(RespawnFlicker());
    }
}