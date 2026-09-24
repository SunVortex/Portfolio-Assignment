using UnityEngine;
using UnityEngine.UI;

public class DashCooldownUI : MonoBehaviour
{
    [Header("UI")]
    [SerializeField] private Image cooldownImage;

    [Header("Cooldown")]
    [SerializeField] private float cooldownTime = 0.6f;

    private float cooldownTimer;
    private bool isCoolingDown;

    private void Start()
    {
        SetReady();
    }

    private void Update()
    {
        // TEMPORARY TEST
        if (Input.GetKeyDown(KeyCode.Space))
        {
            StartCooldown();
        }

        if (!isCoolingDown)
            return;

        cooldownTimer -= Time.deltaTime;

        float progress = 1f - (cooldownTimer / cooldownTime);

        cooldownImage.fillAmount = Mathf.Clamp01(progress);

        if (cooldownTimer <= 0f)
        {
            SetReady();
        }
    }

    public void StartCooldown()
    {
        cooldownTimer = cooldownTime;
        isCoolingDown = true;

        cooldownImage.fillAmount = 0f;
    }

    private void SetReady()
    {
        cooldownTimer = 0f;
        isCoolingDown = false;

        cooldownImage.fillAmount = 1f;
    }
}