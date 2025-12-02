using UnityEngine;

public class EnemySpawner : MonoBehaviour
{
    [Header("Spawn Area")]
    public WorldBounds bounds;
    public GameObject enemyPrefab;
    public int spawnCount = 6;

    [Header("Patrol Points")]
    public Transform[] patrolPointPool;

    [Header("Healthbar")]
    public GameObject enemyHealthbarPrefab;
    public Transform healthbarParent;

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


            if (Physics2D.OverlapCircle(spawnPos, spawnRadiusCheck, obstacleMask) != null)
                continue;

            GameObject enemy = Instantiate(enemyPrefab, spawnPos, Quaternion.identity);


            if (enemy.tag == "Untagged")
                enemy.tag = "Enemy";

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

            EnemyAI ai = enemy.GetComponent<EnemyAI>();
            if (ai != null && patrolPointPool != null && patrolPointPool.Length > 0)
            {
                ai.patrolPoints = patrolPointPool;
            }


            spawned++;
        }
    }
}
