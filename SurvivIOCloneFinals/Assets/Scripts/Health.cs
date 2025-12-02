using UnityEngine;
using System;
using System.Collections;

public class Health : MonoBehaviour
{
    [Header("Health")]
    public int maxHealth = 100;
    public int currentHealth;
    public bool isEnemy = false;

    // Fired whenever HP changes (Player HUD uses this)
    public event Action<int, int> OnHealthChanged;

    // Fired when this unit dies (EnemyKillReporter listens to this)
    public event Action<GameObject> OnDied;

    void Awake()
    {
        currentHealth = maxHealth;
    }

    /// <summary>
    /// Apply damage to this unit.
    /// </summary>
    public void TakeDamage(int amount, GameObject instigator = null)
    {
        if (currentHealth <= 0) return;

        currentHealth -= amount;

        // Update HUD or listeners
        OnHealthChanged?.Invoke(currentHealth, maxHealth);

        if (currentHealth <= 0)
            Die(instigator);
    }

    /// <summary>
    /// Handles unit death (Player or Enemy).
    /// </summary>
    void Die(GameObject instigator)
    {
        // Notify listeners (GameManager via EnemyKillReporter)
        OnDied?.Invoke(instigator);

        if (isEnemy)
        {
            // Enemy death
            Destroy(gameObject);
        }
        else
        {
            // Player death
            PlayerController pc = GetComponent<PlayerController>();
            if (pc != null)
                pc.OnDeath();
        }
    }

    /// <summary>
    /// Restore HP (optional helper)
    /// </summary>
    public void Heal(int amount)
    {
        currentHealth = Mathf.Min(currentHealth + amount, maxHealth);
        OnHealthChanged?.Invoke(currentHealth, maxHealth);
    }
}
