using UnityEngine;

public class MainMenuManager : MonoBehaviour
{
    [Header("UI Panel Reference")]
    [SerializeField] private GameObject menuPanel;

    private void Start()
    {
        // Pause time while menu is active on launch
        Time.timeScale = 0f;
    }

    public void PlayGame()
    {
        if (menuPanel != null)
        {
            menuPanel.SetActive(false);
        }

        // Resume game time
        Time.timeScale = 1f;
    }

    public void QuitGame()
    {
        Debug.Log("Quit Game pressed.");

#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }
}