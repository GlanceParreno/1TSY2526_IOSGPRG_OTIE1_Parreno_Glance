using UnityEngine;

[RequireComponent(typeof(Health))]
public class EnemyKillReporter : MonoBehaviour
{
    Health health;

    void Awake()
    {
        health = GetComponent<Health>();


        health.OnDied += HandleEnemyDeath;
    }

    void HandleEnemyDeath(GameObject instigator)
    {

        if (GameManager.Instance != null)
            GameManager.Instance.ReportKill();
    }

    void OnDestroy()
    {

        if (health != null)
            health.OnDied -= HandleEnemyDeath;
    }
}
