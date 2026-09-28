using UnityEngine;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance { get; private set; }

    [Header("Audio Sources")]
    [SerializeField] private AudioSource bgmSource;
    [SerializeField] private AudioSource windSource;
    [SerializeField] private AudioSource sfx2DSource;

    [Header("Audio Clips - Music & Ambience")]
    [SerializeField] private AudioClip backgroundBGM;
    [SerializeField] private AudioClip backgroundWind;

    [Header("Audio Clips - UI")]
    [SerializeField] private AudioClip uiHoverSFX;
    [SerializeField] private AudioClip uiClickSFX;

    [Header("Inspector Volume Boost")]
    [Range(1f, 5f)][SerializeField] private float uiVolumeBoost = 1.0f;

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
        if (bgmSource != null)
        {
            bgmSource.Stop();
            bgmSource.playOnAwake = false;
        }

        // Force preload UI hover clip into RAM memory
        if (uiHoverSFX != null)
        {
            uiHoverSFX.LoadAudioData();
        }

        if (uiClickSFX != null)
        {
            uiClickSFX.LoadAudioData();
        }

        PlayWindAmbience(backgroundWind);
    }

    public void StartGameplayBGM()
    {
        if (backgroundBGM == null || bgmSource == null) return;

        bgmSource.clip = backgroundBGM;
        bgmSource.loop = true;
        bgmSource.Play();
    }

    public void PlayWindAmbience(AudioClip clip)
    {
        if (clip == null || windSource == null) return;

        windSource.clip = clip;
        windSource.loop = true;
        windSource.Play();
    }

    public void PlayUIHover()
    {
        if (uiHoverSFX != null && sfx2DSource != null)
        {
            sfx2DSource.PlayOneShot(uiHoverSFX, uiVolumeBoost);
        }
    }

    public void PlayUIClick()
    {
        if (uiClickSFX != null && sfx2DSource != null)
        {
            sfx2DSource.PlayOneShot(uiClickSFX, uiVolumeBoost);
        }
    }
}