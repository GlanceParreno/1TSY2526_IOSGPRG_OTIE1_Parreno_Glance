using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{

    private UIManager uiManager;

    public static GameManager Instance;

    [Header("Gameplay")]
    public int score = 0;

    [Header("UI References")]
    public GameObject gameOverUI; // Assign this in Inspector (disabled by default)

    private void Awake()
    {
        // Singleton pattern (makes sure only one GameManager exists)
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
            return;
        }

        uiManager = FindFirstObjectByType<UIManager>();

    }

    // ✅ --- SCORING ---
    public void AddScore(int points)
{
    score += points;
    uiManager?.UpdateScore(score);
    Debug.Log("🏆 Score: " + score);
}

public void GameOver()
{
    Debug.Log("💀 GAME OVER!");
    Time.timeScale = 0f;
    if (gameOverUI != null)
        gameOverUI.SetActive(true);
}


    // ✅ --- RETRY ---
    public void Retry()
    {
        Debug.Log("🔄 Restarting Game...");
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }
}
