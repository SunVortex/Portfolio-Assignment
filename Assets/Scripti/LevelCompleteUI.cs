using UnityEngine;
using UnityEngine.SceneManagement;

public class LevelCompleteUI : MonoBehaviour
{
    /// <summary>
    /// Call this when the Home Button is clicked.
    /// </summary>
    public void GoToHomeMenu()
    {
        // Unfreeze time before switching scenes or returning to menu
        Time.timeScale = 1f;

        // Play Button Click Sound
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlayUIClick();
        }

        // Option A: Reload current level scene
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);

        // Option B: If you have a separate Menu Scene (uncomment line below and replace "MainMenu"):
        // SceneManager.LoadScene("MainMenu");
    }
}