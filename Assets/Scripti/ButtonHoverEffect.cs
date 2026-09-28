using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;

[RequireComponent(typeof(AudioSource))]
public class ButtonHoverEffect : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler
{
    [Header("Background Settings")]
    [SerializeField] private Image buttonBackgroundImage;

    [Header("Text Color Settings")]
    [SerializeField] private Color normalTextColor = new Color(0.9f, 0.4f, 0.1f);
    [SerializeField] private Color hoverTextColor = Color.black;

    [Header("Hover Scale Settings")]
    [SerializeField] private Vector3 hoverScale = new Vector3(1.05f, 1.05f, 1f);
    [SerializeField] private float speed = 15f;

    [Header("Audio Settings")]
    [SerializeField] private AudioClip hoverSound; // Slot 1: Played on mouse hover
    [SerializeField] private AudioClip clickSound; // Slot 2: Played on button click

    private TMP_Text tmpText;
    private Text legacyText;
    private AudioSource audioSource;

    private Vector3 originalScale;
    private Vector3 targetScale;

    private void Awake()
    {
        originalScale = transform.localScale;
        targetScale = originalScale;

        // Auto-assign Image if not set
        if (buttonBackgroundImage == null)
        {
            buttonBackgroundImage = GetComponent<Image>();
        }

        // Auto-get or add AudioSource
        audioSource = GetComponent<AudioSource>();
        audioSource.playOnAwake = false;

        // Get Text component from children
        tmpText = GetComponentInChildren<TMP_Text>();
        if (tmpText == null)
        {
            legacyText = GetComponentInChildren<Text>();
        }

        SetBackgroundVisibility(false);
        SetTextColor(normalTextColor);
    }

    private void Update()
    {
        transform.localScale = Vector3.Lerp(transform.localScale, targetScale, Time.unscaledDeltaTime * speed);
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        targetScale = Vector3.Scale(originalScale, hoverScale);
        SetBackgroundVisibility(true);
        SetTextColor(hoverTextColor);

        // Play Hover Sound
        if (hoverSound != null && audioSource != null)
        {
            audioSource.PlayOneShot(hoverSound);
        }
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        targetScale = originalScale;
        SetBackgroundVisibility(false);
        SetTextColor(normalTextColor);
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        // Play Click Sound
        if (clickSound != null && audioSource != null)
        {
            audioSource.PlayOneShot(clickSound);
        }
    }

    private void SetBackgroundVisibility(bool isVisible)
    {
        if (buttonBackgroundImage != null)
        {
            buttonBackgroundImage.enabled = isVisible;
        }
    }

    private void SetTextColor(Color targetColor)
    {
        if (tmpText != null)
        {
            tmpText.color = targetColor;
        }
        else if (legacyText != null)
        {
            legacyText.color = targetColor;
        }
    }

    private void OnDisable()
    {
        transform.localScale = originalScale;
        targetScale = originalScale;
        SetBackgroundVisibility(false);
        SetTextColor(normalTextColor);
    }
}