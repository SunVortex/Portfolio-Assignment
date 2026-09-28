using System.Collections;
using UnityEngine;
using UnityEngine.Video;

public class TutorialPanelController : MonoBehaviour
{
    [Header("UI & Video References")]
    [SerializeField] private GameObject tutorialPanel;
    [SerializeField] private VideoPlayer videoPlayer;

    private void Awake()
    {
        if (tutorialPanel != null)
            tutorialPanel.SetActive(false);
    }

    /// <summary>
    /// Hook this to your TUTORIAL Button OnClick()
    /// </summary>
    public void OpenTutorial()
    {
        if (AudioManager.Instance != null)
            AudioManager.Instance.PlayUIClick();

        if (tutorialPanel != null)
            tutorialPanel.SetActive(true);

        if (videoPlayer != null)
        {
            StartCoroutine(PlayVideoRoutine());
        }
    }

    private IEnumerator PlayVideoRoutine()
    {
        videoPlayer.Stop();
        videoPlayer.Prepare();

        while (!videoPlayer.isPrepared)
        {
            yield return null;
        }

        videoPlayer.Play();
    }

    /// <summary>
    /// Hook this to the Top-Right Close Button (X) OnClick()
    /// </summary>
    public void CloseTutorial()
    {
        if (AudioManager.Instance != null)
            AudioManager.Instance.PlayUIClick();

        if (videoPlayer != null)
        {
            videoPlayer.Stop();
        }

        if (tutorialPanel != null)
            tutorialPanel.SetActive(false);
    }
}