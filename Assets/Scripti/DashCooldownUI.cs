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
    [SerializeField] private Color wallBlockedColor = new Color(0.35f, 0.35f, 0.35f, 0.6f); // Shaded out grey

    private float cooldownTimer;
    private bool isCoolingDown;
    private bool isBlockedByWall;

    private void Start()
    {
        SetReady();
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
            SetReady();
        }
    }

    /// <summary>
    /// Call this from NarutoDashController to trigger the radial cooldown animation.
    /// </summary>
    public void StartCooldown()
    {
        cooldownTimer = cooldownTime;
        isCoolingDown = true;

        if (cooldownImage != null)
        {
            cooldownImage.fillAmount = 0f;
        }
    }

    /// <summary>
    /// Call this from NarutoDashController to shade out the icon when touching a boundary wall.
    /// </summary>
    public void SetWallBlockedState(bool blocked)
    {
        isBlockedByWall = blocked;

        if (cooldownImage != null)
        {
            cooldownImage.color = isBlockedByWall ? wallBlockedColor : normalColor;
        }
    }

    private void SetReady()
    {
        cooldownTimer = 0f;
        isCoolingDown = false;

        if (cooldownImage != null)
        {
            cooldownImage.fillAmount = 1f;
            cooldownImage.color = isBlockedByWall ? wallBlockedColor : normalColor;
        }
    }
}