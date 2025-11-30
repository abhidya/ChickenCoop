using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// AudioManager - Manages all game audio including sound effects and background music.
/// Implements singleton pattern for global access.
/// </summary>
public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance { get; private set; }

    [Header("Audio Sources")]
    [SerializeField] private AudioSource sfxSource;
    [SerializeField] private AudioSource musicSource;

    [Header("Sound Effects")]
    [SerializeField] private AudioClip harvestSound;
    [SerializeField] private AudioClip collectSound;
    [SerializeField] private AudioClip eggSound;
    [SerializeField] private AudioClip sellSound;
    [SerializeField] private AudioClip coinSound;
    [SerializeField] private AudioClip upgradeSound;
    [SerializeField] private AudioClip eatSound;
    [SerializeField] private AudioClip clickSound;

    [Header("Music")]
    [SerializeField] private AudioClip backgroundMusic;

    [Header("Settings")]
    [SerializeField] private float sfxVolume = 1f;
    [SerializeField] private float musicVolume = 0.5f;

    // Dictionary for quick sound lookup
    private Dictionary<string, AudioClip> soundClips;

    // Cache for procedural sounds to avoid repeated allocations
    private Dictionary<string, AudioClip> proceduralSoundCache;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            InitializeAudio();
        }
        else
        {
            Destroy(gameObject);
        }
    }

    /// <summary>
    /// Initialize audio system and create audio sources if needed
    /// </summary>
    private void InitializeAudio()
    {
        // Create audio sources if not assigned
        if (sfxSource == null)
        {
            sfxSource = gameObject.AddComponent<AudioSource>();
            sfxSource.playOnAwake = false;
        }

        if (musicSource == null)
        {
            musicSource = gameObject.AddComponent<AudioSource>();
            musicSource.playOnAwake = false;
            musicSource.loop = true;
        }

        // Build sound dictionary
        soundClips = new Dictionary<string, AudioClip>
        {
            { "harvest", harvestSound },
            { "collect", collectSound },
            { "egg", eggSound },
            { "sell", sellSound },
            { "coin", coinSound },
            { "upgrade", upgradeSound },
            { "eat", eatSound },
            { "click", clickSound }
        };

        // Apply volume settings
        sfxSource.volume = sfxVolume;
        musicSource.volume = musicVolume;

        // Start background music if available
        if (backgroundMusic != null)
        {
            PlayMusic(backgroundMusic);
        }
    }

    /// <summary>
    /// Play a sound effect by name
    /// </summary>
    public void PlaySound(string soundName)
    {
        if (soundClips != null && soundClips.TryGetValue(soundName, out AudioClip clip))
        {
            if (clip != null)
            {
                sfxSource.PlayOneShot(clip, sfxVolume);
            }
            else
            {
                // Play a procedural beep if no clip assigned
                PlayProceduralSound(soundName);
            }
        }
        else
        {
            // Play a procedural beep for unassigned sounds
            PlayProceduralSound(soundName);
        }
    }

    /// <summary>
    /// Play a specific audio clip
    /// </summary>
    public void PlayClip(AudioClip clip)
    {
        if (clip != null)
        {
            sfxSource.PlayOneShot(clip, sfxVolume);
        }
    }

    /// <summary>
    /// Play background music
    /// </summary>
    public void PlayMusic(AudioClip music)
    {
        if (music != null)
        {
            musicSource.clip = music;
            musicSource.Play();
        }
    }

    /// <summary>
    /// Stop background music
    /// </summary>
    public void StopMusic()
    {
        musicSource.Stop();
    }

    /// <summary>
    /// Set SFX volume
    /// </summary>
    public void SetSFXVolume(float volume)
    {
        sfxVolume = Mathf.Clamp01(volume);
        sfxSource.volume = sfxVolume;
        PlayerPrefs.SetFloat("SFXVolume", sfxVolume);
    }

    /// <summary>
    /// Set music volume
    /// </summary>
    public void SetMusicVolume(float volume)
    {
        musicVolume = Mathf.Clamp01(volume);
        musicSource.volume = musicVolume;
        PlayerPrefs.SetFloat("MusicVolume", musicVolume);
    }

    /// <summary>
    /// Load volume settings from PlayerPrefs
    /// </summary>
    public void LoadVolumeSettings()
    {
        sfxVolume = PlayerPrefs.GetFloat("SFXVolume", 1f);
        musicVolume = PlayerPrefs.GetFloat("MusicVolume", 0.5f);

        sfxSource.volume = sfxVolume;
        musicSource.volume = musicVolume;
    }

    /// <summary>
    /// Generate and play a procedural sound effect when no audio clip is available
    /// This provides audio feedback even without actual sound files
    /// Sounds are cached after first creation to avoid repeated allocations
    /// </summary>
    private void PlayProceduralSound(string soundType)
    {
        // Initialize cache if needed
        if (proceduralSoundCache == null)
        {
            proceduralSoundCache = new Dictionary<string, AudioClip>();
        }

        // Check if we already have this sound cached
        if (proceduralSoundCache.TryGetValue(soundType, out AudioClip cachedClip))
        {
            sfxSource.PlayOneShot(cachedClip, sfxVolume);
            return;
        }

        // Create a simple procedural sound
        int sampleRate = 44100;
        int sampleLength = sampleRate / 10; // 0.1 seconds

        AudioClip clip = AudioClip.Create("procedural_" + soundType, sampleLength, 1, sampleRate, false);
        float[] samples = new float[sampleLength];

        // Different sound profiles based on type
        float frequency = 440f;
        float amplitude = 0.3f;

        switch (soundType)
        {
            case "harvest":
                frequency = 523.25f; // C5
                break;
            case "collect":
                frequency = 659.25f; // E5
                break;
            case "egg":
                frequency = 783.99f; // G5
                break;
            case "sell":
                frequency = 880f; // A5
                break;
            case "coin":
                frequency = 1046.5f; // C6
                break;
            case "upgrade":
                frequency = 587.33f; // D5
                break;
            case "eat":
                frequency = 392f; // G4
                break;
            case "click":
                frequency = 1000f;
                amplitude = 0.2f;
                break;
        }

        // Generate simple sine wave with envelope
        for (int i = 0; i < sampleLength; i++)
        {
            float t = (float)i / sampleRate;
            float envelope = 1f - ((float)i / sampleLength); // Simple decay
            samples[i] = amplitude * envelope * Mathf.Sin(2f * Mathf.PI * frequency * t);
        }

        clip.SetData(samples, 0);

        // Cache the generated clip for future use
        proceduralSoundCache[soundType] = clip;

        sfxSource.PlayOneShot(clip, sfxVolume);
    }

    /// <summary>
    /// Play a quick UI click sound
    /// </summary>
    public void PlayClick()
    {
        PlaySound("click");
    }

    /// <summary>
    /// Mute/unmute all audio
    /// </summary>
    public void SetMute(bool muted)
    {
        sfxSource.mute = muted;
        musicSource.mute = muted;
    }

    /// <summary>
    /// Toggle mute
    /// </summary>
    public void ToggleMute()
    {
        bool isMuted = !sfxSource.mute;
        SetMute(isMuted);
    }
}
