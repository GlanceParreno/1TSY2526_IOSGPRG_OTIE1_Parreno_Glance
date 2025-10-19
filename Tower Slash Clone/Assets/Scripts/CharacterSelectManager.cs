using UnityEngine;
using UnityEngine.SceneManagement;

public class CharacterSelectManager : MonoBehaviour
{
    public static CharacterSelectManager Instance;

    [Header("Character Options")]
    public GameObject[] characterPrefabs;
    [HideInInspector] public int selectedCharacterIndex = 0;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void SelectCharacter(int index)
    {
        selectedCharacterIndex = index;
        Debug.Log($"Selected character: {characterPrefabs[index].name}");
    }

    public void StartGame()
    {
        SceneManager.LoadScene("GameScene");
    }
}
