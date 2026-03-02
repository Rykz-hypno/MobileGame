using UnityEngine;

public class SoundManager : MonoBehaviour
{
    public static SoundManager Instance { get; private set; }

    [Header("Common Clips")]
    [SerializeField] private AudioClip attackSfx;
    [SerializeField] private AudioClip hurtSfx;
    [SerializeField] private AudioClip gameplayMusic;

    [Header("Audio Sources")]
    [SerializeField] private AudioSource musicSource;
    [SerializeField] private AudioSource sfxSource;

    [Header("Default Volumes")]
    [Range(0f, 1f)]
    [SerializeField] private float musicVolume = 0.7f;
    [Range(0f, 1f)]
    [SerializeField] private float sfxVolume = 1f;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        SetupAudioSources();
        PlayGameplayMusic();
    }

    public void PlayOneShot(AudioClip clip, float volumeScale = 1f)
    {
        if (clip == null)
        {
            return;
        }

        sfxSource.PlayOneShot(clip, Mathf.Clamp01(volumeScale));
    }

    public void PlayAttackSfx(float volumeScale = 1f)
    {
        PlayOneShot(attackSfx, volumeScale);
    }

    public void PlayHurtSfx(float volumeScale = 1f)
    {
        PlayOneShot(hurtSfx, volumeScale);
    }

    public void PlayMusic(AudioClip clip, bool loop = true)
    {
        if (clip == null)
        {
            return;
        }

        if (musicSource.clip == clip && musicSource.isPlaying)
        {
            return;
        }

        musicSource.clip = clip;
        musicSource.loop = loop;
        musicSource.Play();
    }

    public void PlayGameplayMusic(bool loop = true)
    {
        PlayMusic(gameplayMusic, loop);
    }

    public void StopMusic()
    {
        musicSource.Stop();
        musicSource.clip = null;
    }

    public void SetMusicVolume(float volume)
    {
        musicSource.volume = Mathf.Clamp01(volume);
    }

    public void SetSfxVolume(float volume)
    {
        sfxSource.volume = Mathf.Clamp01(volume);
    }

    private void SetupAudioSources()
    {
        if (musicSource == null)
        {
            musicSource = gameObject.AddComponent<AudioSource>();
        }

        if (sfxSource == null)
        {
            sfxSource = gameObject.AddComponent<AudioSource>();
        }

        musicSource.playOnAwake = false;
        musicSource.loop = true;
        musicSource.volume = musicVolume;

        sfxSource.playOnAwake = false;
        sfxSource.loop = false;
        sfxSource.volume = sfxVolume;
    }
}
