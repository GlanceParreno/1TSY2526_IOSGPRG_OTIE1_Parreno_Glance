using UnityEngine;

public enum EnemyType { Green, Red }

public class EnemyController : MonoBehaviour
{
    public EnemyType enemyType;
    public float fallSpeed = 2f;
    public float slashLineY = -2.5f; // ✅ Y position where enemy can be slashed
    private bool canBeSlashed = false;
    private PlayerController player;

    private void Start()
    {
        player = FindFirstObjectByType<PlayerController>();
    }

    private void OnEnable()
    {
        SwipeManager.OnSwipe += OnSwipe;
    }

    private void OnDisable()
    {
        SwipeManager.OnSwipe -= OnSwipe;
    }

    private void Update()
    {
        // Make the enemy fall down
        transform.Translate(Vector2.down * fallSpeed * Time.deltaTime);

        // Once the enemy reaches the slash line, it becomes slashable
        if (transform.position.y <= slashLineY && !canBeSlashed)
        {
            canBeSlashed = true;
            Debug.Log($"{name} is now slashable!");
        }

        // If it goes past the bottom, it counts as a missed slash
        if (transform.position.y < -6f)
        {
            if (canBeSlashed) player.TakeDamage(1);
            Destroy(gameObject);
        }
    }

    private void OnSwipe(Vector2 dir)
    {
        if (!canBeSlashed) return;

        Vector2 expectedDir = GetExpectedDirection();

        // Red enemies require opposite direction
        if (enemyType == EnemyType.Red)
            expectedDir *= -1;

        if (Vector2.Dot(dir, expectedDir) > 0.7f)
        {
            Debug.Log($"✅ Correct swipe on {name} ({enemyType})");
            Destroy(gameObject);
        }
        else
        {
            Debug.Log($"❌ Wrong swipe on {name} ({enemyType})");
            player.TakeDamage(1);
        }
    }

    private Vector2 GetExpectedDirection()
    {
        switch (gameObject.tag)
        {
            case "Up": return Vector2.up;
            case "Down": return Vector2.down;
            case "Left": return Vector2.left;
            case "Right": return Vector2.right;
            default: return Vector2.zero;
        }
    }
}
