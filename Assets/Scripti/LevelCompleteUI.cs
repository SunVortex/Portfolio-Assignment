using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;

public class LevelCompleteUI : MonoBehaviour
{
    [Header("Menu Reference")]
    [Tooltip("Drag your 'menu' panel GameObject here.")]
    [SerializeField] private GameObject menuPanel;

    private bool isGameStarted = false;

    private void Start()
    {
        // Start on Main Menu
        if (menuPanel != null)
        {
            menuPanel.SetActive(true);
            isGameStarted = false;
            Time.timeScale = 0f;
            Debug.Log("[LevelCompleteUI] Game launched: Main Menu active, Time.timeScale set to 0, isGameStarted = false.");
        }
        else
        {
            Debug.LogError("[LevelCompleteUI] ERROR: 'menuPanel' slot is missing in the Inspector!");
        }
    }

    private void Update()
    {
        // FAILSAFE: If the menu was closed or time was resumed externally, force isGameStarted = true
        if (!isGameStarted && menuPanel != null && (!menuPanel.activeSelf || Time.timeScale > 0f))
        {
            isGameStarted = true;
            Debug.Log("[LevelCompleteUI] Detected gameplay running! Auto-set isGameStarted = true.");
        }

        // Listen for ESC key press
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            Debug.Log($"[LevelCompleteUI] 'ESC' Key Pressed! Current State -> isGameStarted: {isGameStarted}, Time.timeScale: {Time.timeScale}");

            if (!isGameStarted)
            {
                Debug.Log("[LevelCompleteUI] ESC ignored because isGameStarted is still false (Main Menu active).");
                return;
            }

            TogglePauseMenu();
        }
    }

    /// <summary>
    /// Hook this to your PLAY Button OnClick() in the Main Menu
    /// </summary>
    public void StartGame()
    {
        Debug.Log("[LevelCompleteUI] PLAY Button Clicked! Starting gameplay...");

        isGameStarted = true;

        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlayUIClick();
            AudioManager.Instance.StartGameplayBGM();
        }

        if (menuPanel != null)
        {
            menuPanel.SetActive(false);
            Debug.Log("[LevelCompleteUI] Main Menu panel hidden.");
        }

        // Unfreeze time for gameplay
        Time.timeScale = 1f;
        Debug.Log($"[LevelCompleteUI] Gameplay active. Time.timeScale = {Time.timeScale}");

        ClearUIFocus();
    }

    /// <summary>
    /// Handles ESC key toggling while in active gameplay
    /// </summary>
    public void TogglePauseMenu()
    {
        if (menuPanel == null)
        {
            Debug.LogError("[LevelCompleteUI] Cannot toggle menu: 'menuPanel' is Null!");
            return;
        }

        bool isCurrentlyActive = menuPanel.activeSelf;
        bool newState = !isCurrentlyActive;

        menuPanel.SetActive(newState);
        Time.timeScale = newState ? 0f : 1f;

        Debug.Log($"[LevelCompleteUI] Toggled Pause Menu! Panel Active: {newState}, Time.timeScale: {Time.timeScale}");

        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlayUIClick();
        }

        ClearUIFocus();
    }

    private void ClearUIFocus()
    {
        if (EventSystem.current != null)
        {
            EventSystem.current.SetSelectedGameObject(null);
            Debug.Log("[LevelCompleteUI] UI Button focus cleared.");
        }
    }

    /// <summary>
    /// Hook this to the HOME / RESTART button inside the menu
    /// </summary>
    public void GoToHomeMenu()
    {
        Debug.Log("[LevelCompleteUI] Reloading scene...");
        Time.timeScale = 1f;

        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlayUIClick();
        }

        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }
}