using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    public UIManager uiManager;
    public int killTarget = 10;
    private int kills = 0;

    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    void Start()
    {
        if (uiManager == null)
            uiManager = FindObjectOfType<UIManager>();
    }

    // Called by enemy death
    public void ReportKill()
    {
        kills++;

        if (kills >= killTarget)
        {
            OnPlayerWin();
        }
    }

    public void OnPlayerDeath()
    {
        Debug.Log("[GameManager] Player died -> Game Over");
        if (uiManager != null)
            uiManager.ShowGameOver();
    }

    public void OnPlayerWin()
    {
        Debug.Log("[GameManager] Player won -> Chicken Dinner!");

        if (uiManager != null)
            uiManager.ShowChickenDinner();

        // Stop gameplay
        Time.timeScale = 0f;
    }
}
