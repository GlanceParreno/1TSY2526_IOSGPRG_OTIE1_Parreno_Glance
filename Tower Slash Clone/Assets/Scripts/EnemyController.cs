using UnityEngine;

public enum EnemyType { Green, Red, Rotating }

public class EnemyController : MonoBehaviour
{
    [Header("Enemy Settings")]
    public EnemyType enemyType;
    public float fallSpeed = 2f;
    public float slashLineY = -2.5f;
    private bool canBeSlashed = false;
    private PlayerController player;

    [Header("Rotating Arrow Settings")]
    public bool isRotating = false;
    public float rotateSpeed = 360f; // degrees per second
    private float rotationAngle = 0f;
    private Quaternion originalRotation;

    [Header("Powerup Settings")]
    public GameObject powerupPrefab; // assign in Inspector

    private bool hasPassedPlayer = false; // ✅ track if enemy already passed

    private void Start()
    {
        player = FindFirstObjectByType<PlayerController>();
        originalRotation = transform.rotation;

        // ✅ Enable rotation for rotating type
        if (enemyType == EnemyType.Rotating)
            isRotating = true;

        // ✅ Always slashable immediately (static-player setup)
        canBeSlashed = true;
    }

    private void OnEnable() => SwipeManager.OnSwipe += OnSwipe;
    private void OnDisable() => SwipeManager.OnSwipe -= OnSwipe;

    private void Update()
    {
        // Move enemy downward (or upward if fallSpeed < 0)
        transform.Translate(Vector2.down * fallSpeed * Time.deltaTime);

        // ✅ Handle rotating behavior
        if (isRotating)
        {
            float deltaAngle = rotateSpeed * Time.deltaTime;
            rotationAngle += deltaAngle;
            transform.Rotate(0f, 0f, deltaAngle);

            if (rotationAngle >= 360f)
            {
                isRotating = false;
                transform.rotation = originalRotation;
            }
        }

        // ✅ Check if enemy passes the player's Y position
        if (!hasPassedPlayer && player != null)
        {
            bool passedDown = fallSpeed > 0 && transform.position.y <= player.transform.position.y;
            bool passedUp = fallSpeed < 0 && transform.position.y >= player.transform.position.y;

            if (passedDown || passedUp)
            {
                hasPassedPlayer = true;
                player.TakeDamage(1);
                Debug.Log($"💥 {name} passed the player!");
                Destroy(gameObject);
            }
        }

        // Cleanup (off-screen)
        if (transform.position.y < -8f || transform.position.y > 8f)
            Destroy(gameObject);
    }

    private void OnSwipe(Vector2 dir)
    {
        if (!canBeSlashed) return;

        Vector2 expectedDir = GetExpectedDirection();

        // Red enemies = opposite swipe direction
        if (enemyType == EnemyType.Red)
            expectedDir *= -1;

        // Rotating enemies = use their current facing
        if (enemyType == EnemyType.Rotating)
            expectedDir = transform.up.normalized;

        // ✅ Check swipe correctness
        if (Vector2.Dot(dir, expectedDir) > 0.7f)
        {
            Debug.Log($"✅ Correct swipe on {name} ({enemyType})");

            // 💖 3% chance to spawn powerup
            if (UnityEngine.Random.value <= 0.03f && powerupPrefab != null)
                Instantiate(powerupPrefab, transform.position, Quaternion.identity);

            // ⚡ Add dash gauge charge (10% per kill)
            FindFirstObjectByType<DashGauge>()?.AddCharge(0.1f);

            // 🏆 Optional: Add score
            GameManager.Instance?.AddScore(10);

            Destroy(gameObject);
        }
        else
        {
            // ❌ Wrong swipe — no penalty now
            Debug.Log($"⚠️ Wrong swipe on {name} ({enemyType}), no damage.");
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
