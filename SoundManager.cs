using UnityEngine;



public enum SoundType
{
    JumpSaleem,
    JumpSalmaa,
    DoubleJumpSalama,
    RunningSaleem,
    RunningSalmaa,
    SwitchCharacter
}

/// <summary>
/// Singleton manager for all game audio.
/// Handles sound effects (SFX) via a simple enum-based system.
/// Manages a music playlist that loops through tracks.
/// </summary>
public class SoundManager : MonoBehaviour
{
    /// <summary>
    /// Singleton instance for global access.
    /// </summary>
    public static SoundManager Instance { get; private set; }

    [Header("Clips")]
    [SerializeField] private AudioClip[] soundList;

    [Header("Music Playlist")]
    [SerializeField] private AudioClip[] musicList;

    [Header("Audio Sources")]
    [SerializeField] private AudioSource sfxSource;
    [SerializeField] private AudioSource musicSource;

    private int currentMusicIndex;
    private bool playlistActive;
    private bool musicStopped = false;


    private void Awake()
    {
        if (Instance != null)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    private void Start()
    {
        StartPlaylist();
    }

    private void Update()
    {
        HandlePlaylist();
    }

    /* ===================== PLAYLIST ===================== */

    private void StartPlaylist()
    {
        if (musicList.Length == 0)
            return;

        playlistActive = true;
        currentMusicIndex = Mathf.Clamp(currentMusicIndex, 0, musicList.Length - 1);

        PlayCurrentTrack();
    }

    private void PlayCurrentTrack()
    {
        musicSource.clip = musicList[currentMusicIndex];
        musicSource.loop = false;
        musicSource.Play();
    }

    /// <summary>
    /// Monitors the music source and advances to the next track when the current one finishes.
    /// </summary>
    private void HandlePlaylist()
    {
        if (!playlistActive || musicStopped)
            return;

        if (!musicSource.isPlaying && musicSource.time == 0f)
        {
            PlayNextTrack();
        }
    }

    private void PlayNextTrack()
    {
        currentMusicIndex = (currentMusicIndex + 1) % musicList.Length;
        PlayCurrentTrack();
    }

    /* ===================== SFX ===================== */

    /// <summary>
    /// Plays a specific sound effect.
    /// </summary>
    /// <param name="sound">The type of sound to play (mapped to the internal array).</param>
    /// <param name="volume">Optional volume override.</param>
    public static void PlaySound(SoundType sound, float volume = 1f)
    {
        if (Instance == null) return;

        Instance.sfxSource.PlayOneShot(
            Instance.soundList[(int)sound],
            volume
        );
    }

    /* ===================== VOLUME ===================== */

    public static void SetMusicVolume(float volume)
    {
        if (Instance == null) return;
        Instance.musicSource.volume = volume;
    }

    public static void SetSFXVolume(float volume)
    {
        if (Instance == null) return;
        Instance.sfxSource.volume = volume;
    }


    /// <summary>
    /// Stops the music and disables the playlist (e.g., for Game Over).
    /// </summary>
    public static void StopMusic()
    {
        if (Instance == null) return;

        Instance.musicStopped = true;
        Instance.playlistActive = false;

        Instance.musicSource.Stop();
    }

    public static void ResumeMusic()
    {
        if (Instance == null) return;

        Instance.musicStopped = false;
        Instance.playlistActive = true;
        Instance.PlayCurrentTrack();
    }



    public float InstanceMusicVolume => musicSource.volume;
    public float InstanceSFXVolume => sfxSource.volume;
}