using UnityEngine;

public class LevelFinishTrigger : MonoBehaviour
{
    [Header("Level Complete UI")]
    [SerializeField] private GameObject levelCompletePanel;

    [Header("Audio")]
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip levelCompleteSFX;

    private bool levelFinished = false;

    private void Awake()
    {
        if (audioSource == null)
            audioSource = GetComponent<AudioSource>();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (levelFinished) return;

        if (other.CompareTag("Player") || other.GetComponent<NarutoDashController>() != null)
        {
            levelFinished = true;

            // Open victory panel UI
            if (levelCompletePanel != null)
            {
                levelCompletePanel.SetActive(true);
            }

            // Play level complete SFX
            if (levelCompleteSFX != null && audioSource != null)
            {
                audioSource.PlayOneShot(levelCompleteSFX);
            }

            // Pause time
            Time.timeScale = 0f;
        }
    }
}