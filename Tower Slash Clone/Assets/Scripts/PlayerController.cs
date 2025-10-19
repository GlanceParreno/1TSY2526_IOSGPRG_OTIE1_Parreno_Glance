using UnityEngine;

public class PlayerController : MonoBehaviour
{
    public int maxHealth = 3;
    public int currentHealth;

    private void Start()
    {
        currentHealth = maxHealth;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Enemy"))
        {
            TakeDamage(1);
            Destroy(other.gameObject);
        }
    }

    public void TakeDamage(int damage)
    {
        currentHealth -= damage;
        Debug.Log("Player hit! Health: " + currentHealth);

        if (currentHealth <= 0)
        {
            Debug.Log("Game Over!");
            // Later: trigger GameManager.GameOver()
        }
    }
}
