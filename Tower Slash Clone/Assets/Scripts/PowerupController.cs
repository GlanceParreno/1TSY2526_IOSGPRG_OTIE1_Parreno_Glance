using UnityEngine;

public class PowerupController : MonoBehaviour
{
    [Header("Falling Settings")]
    public float fallSpeed = 2f;

    [Header("Collection Settings")]
    [Tooltip("How close to the player's Y-position before auto-collection occurs.")]
    public float collectLineOffset = 0.3f;

    private PlayerController player;

    private void Start()
    {
        player = FindFirstObjectByType<PlayerController>();
    }

    private void Update()
    {
        // Make the powerup fall downward
        transform.Translate(Vector2.down * fallSpeed * Time.deltaTime);

        // ✅ Auto-collect when it passes the player's Y line (plus offset)
        if (player != null && transform.position.y <= player.transform.position.y + collectLineOffset)
        {
            player.AddHealth(1);
            Debug.Log("💖 Powerup collected automatically!");
            Destroy(gameObject);
        }

        // Destroy if it goes offscreen (failsafe)
        if (transform.position.y < -6f)
            Destroy(gameObject);
    }
}
