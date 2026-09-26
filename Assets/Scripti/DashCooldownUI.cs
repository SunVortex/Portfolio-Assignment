using UnityEngine;
using UnityEngine.UI;

public class DashCooldownUI : MonoBehaviour
{
    [Header("UI Reference")]
    [SerializeField] private Image cooldownImage;

    [Header("Cooldown Settings")]
    [SerializeField] private float cooldownTime = 0.6f;

    [Header("Visual Feedback Colors")]
    [SerializeField] private Color normalColor = Color.white;
    [SerializeField] private Color wallBlockedColor = new Color(0.35f, 0.35f, 0.35f, 0.6f);

    [Header("Audio")]
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip cooldownReadySFX;

    private float cooldownTimer;
    private bool isCoolingDown;
    private bool isBlockedByWall;

    private void Awake()
    {
        if (audioSource == null)
            audioSource = GetComponent<AudioSource>();
    }

    private void Start()
    {
        SetReady(false); // Don't play sound on level start
    }

    private void Update()
    {
        if (!isCoolingDown)
            return;

        cooldownTimer -= Time.deltaTime;

        float progress = 1f - (cooldownTimer / cooldownTime);

        if (cooldownImage != null)
        {
            cooldownImage.fillAmount = Mathf.Clamp01(progress);
        }

        if (cooldownTimer <= 0f)
        {
            SetReady(true);
        }
    }

    public void StartCooldown()
    {
        cooldownTimer = cooldownTime;
        isCoolingDown = true;

        if (cooldownImage != null)
        {
            cooldownImage.fillAmount = 0f;
        }
    }

    public void SetWallBlockedState(bool blocked)
    {
        isBlockedByWall = blocked;

        if (cooldownImage != null)
        {
            cooldownImage.color = isBlockedByWall ? wallBlockedColor : normalColor;
        }
    }

    private void SetReady(bool playSound = true)
    {
        cooldownTimer = 0f;
        isCoolingDown = false;

        if (cooldownImage != null)
        {
            cooldownImage.fillAmount = 1f;
            cooldownImage.color = isBlockedByWall ? wallBlockedColor : normalColor;
        }

        if (playSound && cooldownReadySFX != null && audioSource != null)
        {
            audioSource.PlayOneShot(cooldownReadySFX);
        }
    }
}