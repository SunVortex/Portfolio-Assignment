using UnityEngine;

public class BreakableObject : MonoBehaviour
{
    [Header("Break Settings")]
    [Tooltip("Assign child particle system, or leave empty to auto-detect.")]
    [SerializeField] private ParticleSystem breakParticles;
    [SerializeField] private bool disableColliderOnBreak = true;

    [Header("Cleanup Optimization")]
    [Tooltip("Extra time after particles land on ground before destroying the particle object.")]
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

        if (breakParticles == null)
        {
            breakParticles = GetComponentInChildren<ParticleSystem>(true);
        }

        if (breakParticles != null)
        {
            // Instantiate a clean, single-use particle clone at the object's position
            ParticleSystem spawnedParticles = Instantiate(breakParticles, transform.position, transform.rotation);
            spawnedParticles.gameObject.SetActive(true);

            // Ensure looping is strictly disabled programmatically
            var mainModule = spawnedParticles.main;
            mainModule.loop = false;

            // Trigger single explosion/burst
            spawnedParticles.Play();

            // Calculate total lifetime (particle duration + fall lifetime + extra delay on floor)
            float totalLifetime = mainModule.duration + mainModule.startLifetime.constantMax + extraCleanupDelay;
            Destroy(spawnedParticles.gameObject, totalLifetime);
        }

        // Disable collider immediately so player passes through cleanly
        if (disableColliderOnBreak)
        {
            Collider col = GetComponent<Collider>();
            if (col != null)
            {
                col.enabled = false;
            }
        }

        Debug.Log(gameObject.name + " was destroyed on Dash!");

        // Destroy original crate/bush GameObject immediately
        Destroy(gameObject);
    }
}