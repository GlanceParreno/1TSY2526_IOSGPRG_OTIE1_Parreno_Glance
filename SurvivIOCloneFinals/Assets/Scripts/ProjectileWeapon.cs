using UnityEngine;

/// <summary>
/// Fires a single projectile prefab straight from the muzzle (one bullet per shot).
/// Weapon base handles ammo, reloading and fire mode; this subclass spawns the projectile and configures it.
/// </summary>
public class ProjectileWeapon : Weapon
{
    [Header("Projectile Weapon")]
    public GameObject projectilePrefab;
    [Tooltip("Projectile speed (overrides projectile prefab speed if set)")]
    public float projectileSpeed = 22f;
    [Tooltip("Projectile damage (overrides projectile prefab damage if set)")]
    public int projectileDamage = 10;
    [Tooltip("Distance from muzzle to spawn the projectile to avoid self-collision")]
    public float muzzleOffset = 0.12f;

    protected override void FireOneShot()
    {
        if (projectilePrefab == null || muzzle == null) return;

        Vector3 spawnPos = muzzle.position + (muzzle.right * muzzleOffset);
        Quaternion spawnRot = muzzle.rotation;

        GameObject pgo = Instantiate(projectilePrefab, spawnPos, spawnRot);
        if (pgo == null) return;

        // Configure projectile if it has the Projectile component
        var proj = pgo.GetComponent<Projectile>();
        if (proj != null)
        {
            // set ownerTag so projectile doesn't hit shooter
            if (ownerInventory != null && ownerInventory.gameObject != null)
                proj.ownerTag = ownerInventory.gameObject.tag;

            // override projectile properties when a value is provided
            if (projectileDamage > 0) proj.damage = projectileDamage;
            if (projectileSpeed > 0f) proj.speed = projectileSpeed;
        }

        // Safety: ensure Rigidbody2D velocity is set (Projectile.Start also sets it, but set here just in case)
        var rb = pgo.GetComponent<Rigidbody2D>();
        if (rb != null)
        {
            rb.bodyType = RigidbodyType2D.Dynamic;
            rb.gravityScale = 0f;
            rb.linearVelocity = spawnRot * Vector2.right * projectileSpeed;
        }
    }
}
