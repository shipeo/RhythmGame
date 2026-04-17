using UnityEngine;

/// <summary>
/// AudioManager - Handles all audio playback and synchronization
/// 
/// Cross-platform: Works on Windows, iOS, Android
/// </summary>
[RequireComponent(typeof(AudioSource))]
public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance { get; private set; }

    private AudioSource audioSource;

    [Header("Audio Settings")]
    [SerializeField, Range(0f, 1f)] private float musicVolume = 0.8f;
    [SerializeField] private float audioOffset = 0f;

    public bool IsMusicPlaying() => audioSource != null && audioSource.isPlaying;
    public float CurrentMusicTime => audioSource != null ? audioSource.time + audioOffset : 0f;
    public float MusicLength => audioSource != null && audioSource.clip != null ? audioSource.clip.length : 0f;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    private void Start()
    {
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
            audioSource = gameObject.AddComponent<AudioSource>();

        audioSource.playOnAwake = false;
        audioSource.loop = false;
        audioSource.volume = musicVolume;
    }

    public void PlaySong(AudioClip clip)
    {
        if (clip == null || audioSource == null) return;
        audioSource.clip = clip;
        audioSource.time = 0f;
        audioSource.Play();
    }

    public void PauseMusic() => audioSource?.Pause();
    public void ResumeMusic() => audioSource?.UnPause();
    public void StopMusic() => audioSource?.Stop();
    public void SetVolume(float volume) => audioSource.volume = Mathf.Clamp01(volume);
    public void SetAudioOffset(float offset) => audioOffset = offset;
}
