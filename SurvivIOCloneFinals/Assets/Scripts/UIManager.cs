using UnityEngine;
using UnityEngine.SceneManagement;

public class UIManager : MonoBehaviour
{
    [Header("Panels (assign in Inspector)")]
    public GameObject gameOverPanel;
    public GameObject chickenPanel;

    void Start()
    {

        if (gameOverPanel) gameOverPanel.SetActive(false);
        if (chickenPanel) chickenPanel.SetActive(false);
    }


    public void ShowGameOver()
    {
        if (gameOverPanel) gameOverPanel.SetActive(true);

        Time.timeScale = 0f;
    }


    public void Retry()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }


    public void ShowChickenDinner()
    {
        if (chickenPanel) chickenPanel.SetActive(true);
        Time.timeScale = 0f;
    }


    public void ReturnToMenu(int menuSceneIndex = 0)
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(menuSceneIndex);
    }
}
