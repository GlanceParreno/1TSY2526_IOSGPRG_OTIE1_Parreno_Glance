using UnityEngine;
using UnityEngine.SceneManagement;

public class MenuManager : MonoBehaviour
{
    // Index of Game scene in Build Settings (add MainMenu=0, Game=1)
    public int gameSceneBuildIndex = 1;

    // Called by Start button
    public void StartGame()
    {
        SceneManager.LoadScene(gameSceneBuildIndex);
    }

    // Called by Quit button
    public void QuitGame()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }
}
