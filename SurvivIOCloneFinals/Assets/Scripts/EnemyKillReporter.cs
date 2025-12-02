using UnityEngine;

[RequireComponent(typeof(Health))]
public class EnemyKillReporter : MonoBehaviour
{
    Health health;

    void Awake()
    {
        health = GetComponent<Health>();

        // Subscribe to death event
        health.OnDied += HandleEnemyDeath;
    }

    void HandleEnemyDeath(GameObject instigator)
    {
        // Tell the GameManager that one enemy has died
        if (GameManager.Instance != null)
            GameManager.Instance.ReportKill();
    }

    void OnDestroy()
    {
        // IMPORTANT: avoid memory leaks
        if (health != null)
            health.OnDied -= HandleEnemyDeath;
    }
}
