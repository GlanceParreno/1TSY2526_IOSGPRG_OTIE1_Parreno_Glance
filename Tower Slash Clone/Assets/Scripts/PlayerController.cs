using UnityEngine;

public class PlayerController : MonoBehaviour
{
    public int maxHealth = 3;
    public int currentHealth;

    private void Start()
    {
        currentHealth = maxHealth;
    }

    public void TakeDamage(int amount)
{
    currentHealth -= amount;
    currentHealth = Mathf.Max(0, currentHealth);

    FindFirstObjectByType<UIManager>()?.UpdateHP(currentHealth);

    if (currentHealth <= 0)
        GameManager.Instance?.GameOver();
}

public void AddHealth(int amount)
{
    currentHealth += amount;
    currentHealth = Mathf.Min(currentHealth, maxHealth);
    FindFirstObjectByType<UIManager>()?.UpdateHP(currentHealth);
}
}
