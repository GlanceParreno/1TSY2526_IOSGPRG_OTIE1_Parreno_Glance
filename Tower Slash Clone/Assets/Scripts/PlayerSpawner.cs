using UnityEngine;

public class PlayerSpawner : MonoBehaviour
{
    private void Start()
    {
        if (CharacterSelectManager.Instance != null)
        {
            int selectedIndex = CharacterSelectManager.Instance.selectedCharacterIndex;
            GameObject prefab = CharacterSelectManager.Instance.characterPrefabs[selectedIndex];
            Instantiate(prefab, transform.position, Quaternion.identity);
        }
        else
        {
            Debug.LogWarning("No character selected — spawning default player.");

            // Optional fallback
            GameObject defaultPlayer = Resources.Load<GameObject>("DefaultPlayer");
            if (defaultPlayer != null)
                Instantiate(defaultPlayer, transform.position, Quaternion.identity);
        }
    }
}
