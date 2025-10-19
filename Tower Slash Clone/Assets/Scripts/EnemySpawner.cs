using UnityEngine;
using System.Collections;

public enum SpawnDirection { Downward, Upward }

public class EnemySpawner : MonoBehaviour
{
    [Header("Spawner Settings")]
    public SpawnDirection spawnDirection = SpawnDirection.Downward;

    [Tooltip("Assign Green, Red, and Rotating enemy prefabs here")]
    public GameObject[] enemyPrefabs;

    [Header("Spawn Timing")]
    public float minSpawnInterval = 0.8f;
    public float maxSpawnInterval = 2.0f;

    private void Start()
{
    StartCoroutine(WaitForPlayerAndSpawn());
}

private IEnumerator WaitForPlayerAndSpawn()
{
    // Wait until the player exists before spawning anything
    yield return new WaitUntil(() => FindFirstObjectByType<PlayerController>() != null);

    Debug.Log($"✅ Player detected by {name}, starting spawn routine.");

    // Now start normal spawning
    while (true)
    {
        yield return new WaitForSeconds(UnityEngine.Random.Range(minSpawnInterval, maxSpawnInterval));
        SpawnRandomEnemy();
    }
}

    private IEnumerator SpawnRoutine()
    {
        while (true)
        {
            yield return new WaitForSeconds(UnityEngine.Random.Range(minSpawnInterval, maxSpawnInterval));
            SpawnRandomEnemy();
        }
    }

    private void SpawnRandomEnemy()
    {
        // ✅ Pick a random prefab (Green, Red, or Rotating)
        GameObject prefab = enemyPrefabs[UnityEngine.Random.Range(0, enemyPrefabs.Length)];

        // ✅ Spawn exactly at this spawner's position
        GameObject enemy = Instantiate(prefab, transform.position, Quaternion.identity);

        // ✅ Adjust enemy movement direction based on spawner
        EnemyController ec = enemy.GetComponent<EnemyController>();
        if (ec != null)
        {
            if (spawnDirection == SpawnDirection.Upward)
            {
                ec.fallSpeed *= -1f; // Reverse movement
                ec.slashLineY = 2.5f; // Slash zone above player
            }
            else
            {
                ec.slashLineY = -2.5f; // Slash zone below player
            }
        }
    }

#if UNITY_EDITOR
    private void OnDrawGizmos()
    {
        // Scene view visualization
        Gizmos.color = (spawnDirection == SpawnDirection.Downward) ? Color.red : Color.cyan;
        Gizmos.DrawCube(transform.position, new Vector3(0.4f, 0.4f, 0.4f));

        // Draw arrow indicating spawn direction
        Vector3 dir = (spawnDirection == SpawnDirection.Downward) ? Vector3.down : Vector3.up;
        Gizmos.DrawLine(transform.position, transform.position + dir * 1.5f);
        Gizmos.DrawSphere(transform.position + dir * 1.5f, 0.1f);
    }
#endif
}
