using UnityEngine;
using System;
using System.Collections;

public class Health : MonoBehaviour
{
    [Header("Health")]
    public int maxHealth = 100;
    public int currentHealth;
    public bool isEnemy = false;


    public event Action<int, int> OnHealthChanged;


    public event Action<GameObject> OnDied;

    void Awake()
    {
        currentHealth = maxHealth;
    }

    public void TakeDamage(int amount, GameObject instigator = null)
    {
        if (currentHealth <= 0) return;

        currentHealth -= amount;


        OnHealthChanged?.Invoke(currentHealth, maxHealth);

        if (currentHealth <= 0)
            Die(instigator);
    }


    void Die(GameObject instigator)
    {

        OnDied?.Invoke(instigator);

        if (isEnemy)
        {

            Destroy(gameObject);
        }
        else
        {

            PlayerController pc = GetComponent<PlayerController>();
            if (pc != null)
                pc.OnDeath();
        }
    }

    public void Heal(int amount)
    {
        currentHealth = Mathf.Min(currentHealth + amount, maxHealth);
        OnHealthChanged?.Invoke(currentHealth, maxHealth);
    }
}
