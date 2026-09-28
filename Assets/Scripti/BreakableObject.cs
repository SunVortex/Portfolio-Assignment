using UnityEngine;

public class BreakableObject : MonoBehaviour
{
    [Header("Break Settings")]
    [Tooltip("Assign child particle system, or leave empty to auto-detect.")]
    [SerializeField] private ParticleSystem breakParticles;
    [SerializeField] private bool disableColliderOnBreak = true;

    [Header("Audio Settings")]
    [SerializeField] private AudioClip breakSFX;
    [Tooltip("Boost the breaking volume directly in Inspector (1.0 = Normal, 3.0 = 3x louder).")]
    [Range(0.1f, 5f)][SerializeField] private float breakVolumeMultiplier = 2.5f;

    [Header("Cleanup Optimization")]
    [SerializeField] private float extraCleanupDelay = 2.0f;

    private bool isBroken = false;

    private void Awake()
    {
        if (breakParticles == null)
        {
            breakParticles = GetComponentInChildren<ParticleSystem>(true);
        }
    }

    public void Break()
    {
        if (isBroken)
            return;

        isBroken = true;

        // Play high-volume break audio
        if (breakSFX != null)
        {
            PlayBreakAudio();
        }

        if (breakParticles == null)
        {
            breakParticles = GetComponentInChildren<ParticleSystem>(true);
        }

        if (breakParticles != null)
        {
            ParticleSystem spawnedParticles = Instantiate(breakParticles, transform.position, transform.rotation);
            spawnedParticles.gameObject.SetActive(true);

            var mainModule = spawnedParticles.main;
            mainModule.loop = false;

            spawnedParticles.Play();

            float totalLifetime = mainModule.duration + mainModule.startLifetime.constantMax + extraCleanupDelay;
            Destroy(spawnedParticles.gameObject, totalLifetime);
        }

        if (disableColliderOnBreak)
        {
            Collider col = GetComponent<Collider>();
            if (col != null)
            {
                col.enabled = false;
            }
        }

        Debug.Log(gameObject.name + " was destroyed on Dash!");

        Destroy(gameObject);
    }

    private void PlayBreakAudio()
    {
        // Instantiate temporary AudioSource GameObject so audio survives main object destruction
        GameObject audioObj = new GameObject("BreakAudioSFX");
        audioObj.transform.position = transform.position;

        AudioSource tempAudio = audioObj.AddComponent<AudioSource>();
        tempAudio.clip = breakSFX;
        tempAudio.volume = breakVolumeMultiplier;
        tempAudio.spatialBlend = 0.5f; // Blend between 2D and 3D so it stays crisp and loud from overhead camera
        tempAudio.playOnAwake = false;
        tempAudio.Play();

        Destroy(audioObj, breakSFX.length + 0.1f);
    }
}