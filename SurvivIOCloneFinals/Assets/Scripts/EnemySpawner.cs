using UnityEngine;

public class EnemySpawner : MonoBehaviour
{
    [Header("Spawn Area")]
    public WorldBounds bounds;               // Required
    public GameObject enemyPrefab;           // Enemy prefab (must contain EnemyAutoShooter + EnemyAI + Health)
    public int spawnCount = 6;

    [Header("Patrol Points")]
    public Transform[] patrolPointPool;      // Optional patrol points

    [Header("Healthbar")]
    public GameObject enemyHealthbarPrefab;  // Image-based world-space healthbar
    public Transform healthbarParent;        // Optional parent

    [Header("Obstacle Avoidance")]
    public LayerMask obstacleMask;
    public float spawnRadiusCheck = 0.6f;

    void Start()
    {
        if (bounds == null || enemyPrefab == null)
        {
            Debug.LogWarning("[EnemySpawner] Missing required references.");
            return;
        }

        Rect r = bounds.GetRect();
        int spawned = 0;
        int attempts = 0;
        int maxAttempts = spawnCount * 12;

        while (spawned < spawnCount && attempts < maxAttempts)
        {
            attempts++;

            Vector2 spawnPos = new Vector2(
                Random.Range(r.xMin, r.xMax),
                Random.Range(r.yMin, r.yMax)
            );

            // Avoid walls / obstacles
            if (Physics2D.OverlapCircle(spawnPos, spawnRadiusCheck, obstacleMask) != null)
                continue;

            GameObject enemy = Instantiate(enemyPrefab, spawnPos, Quaternion.identity);

            // Force tag to Enemy for shooter logic
            if (enemy.tag == "Untagged")
                enemy.tag = "Enemy";

            // ----------------------------------------------------
            // HEALTH + HEALTHBAR
            // ----------------------------------------------------
            Health h = enemy.GetComponent<Health>();
            if (h == null) h = enemy.AddComponent<Health>();
            h.isEnemy = true;

            if (enemyHealthbarPrefab != null)
            {
                GameObject hb = Instantiate(enemyHealthbarPrefab);

                if (healthbarParent != null)
                    hb.transform.SetParent(healthbarParent, true);

                EnemyHealthbar bar = hb.GetComponent<EnemyHealthbar>();
                if (bar != null)
                {
                }
                else
                {
                    Debug.LogWarning("[EnemySpawner] enemyHealthbarPrefab missing EnemyHealthbar component.");
                    Destroy(hb);
                }
            }

            // ----------------------------------------------------
            // ENEMY AI
            // ----------------------------------------------------
            EnemyAI ai = enemy.GetComponent<EnemyAI>();
            if (ai != null && patrolPointPool != null && patrolPointPool.Length > 0)
            {
                ai.patrolPoints = patrolPointPool;
            }


            spawned++;
        }
    }
}
