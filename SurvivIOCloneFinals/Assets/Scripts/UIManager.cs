using UnityEngine;
using UnityEngine.SceneManagement;

public class UIManager : MonoBehaviour
{
    [Header("Panels (assign in Inspector)")]
    public GameObject gameOverPanel;   // contains Retry button; default inactive
    public GameObject chickenPanel;    // contains CHICKEN DINNER text; default inactive

    void Start()
    {
        // Ensure panels are hidden at start
        if (gameOverPanel) gameOverPanel.SetActive(false);
        if (chickenPanel) chickenPanel.SetActive(false);
    }

    // Show Game Over UI (Retry)
    public void ShowGameOver()
    {
        if (gameOverPanel) gameOverPanel.SetActive(true);
        // Stop gameplay if desired (optional)
        Time.timeScale = 0f;
    }

    // Called by Retry button
    public void Retry()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    // Show Chicken Dinner text and stop the game
    public void ShowChickenDinner()
    {
        if (chickenPanel) chickenPanel.SetActive(true);
        Time.timeScale = 0f;
    }

    // Optional: return to main menu (not used per request)
    public void ReturnToMenu(int menuSceneIndex = 0)
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(menuSceneIndex);
    }
}
