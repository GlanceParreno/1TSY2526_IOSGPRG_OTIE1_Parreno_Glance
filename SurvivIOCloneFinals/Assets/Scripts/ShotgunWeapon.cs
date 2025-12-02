using UnityEngine;

/// <summary>
/// Fires multiple pellet projectiles in a cone. Copies projectile damage and speed like ProjectileWeapon.
/// </summary>
public class ShotgunWeapon : Weapon
{
    [Header("Shotgun Settings")]
    public GameObject projectilePrefab;
    public float projectileSpeed = 18f;
    public int projectileDamage = 6;
    [Tooltip("Number of pellets per shot")]
    public int pellets = 6;
    [Tooltip("Total cone angle in degrees")]
    public float spreadAngle = 30f;
    public float muzzleOffset = 0.12f;

    protected override void FireOneShot()
    {
        if (projectilePrefab == null || muzzle == null) return;

        float half = spreadAngle * 0.5f;

        for (int i = 0; i < pellets; i++)
        {
            // distribute pellets across the cone
            float t = (pellets == 1) ? 0f : ((float)i / (pellets - 1));
            float angle = Mathf.Lerp(-half, half, t);
            Quaternion rot = muzzle.rotation * Quaternion.Euler(0f, 0f, angle);
            Vector3 spawnPos = muzzle.position + (rot * Vector3.right * muzzleOffset);

            GameObject pgo = Instantiate(projectilePrefab, spawnPos, rot);
            if (pgo == null) continue;

            var proj = pgo.GetComponent<Projectile>();
            if (proj != null)
            {
                if (ownerInventory != null && ownerInventory.gameObject != null)
                    proj.ownerTag = ownerInventory.gameObject.tag;

                if (projectileDamage > 0) proj.damage = projectileDamage;
                if (projectileSpeed > 0f) proj.speed = projectileSpeed;
            }

            var rb = pgo.GetComponent<Rigidbody2D>();
            if (rb != null)
            {
                rb.bodyType = RigidbodyType2D.Dynamic;
                rb.gravityScale = 0f;
                rb.linearVelocity = rot * Vector2.right * projectileSpeed;
            }
        }
    }
}
