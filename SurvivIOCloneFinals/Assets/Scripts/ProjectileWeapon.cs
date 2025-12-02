using UnityEngine;
public class ProjectileWeapon : Weapon
{
    [Header("Projectile Weapon")]
    public GameObject projectilePrefab;
    [Tooltip("Projectile speed")]
    public float projectileSpeed = 22f;
    [Tooltip("Projectile damage")]
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
            rb.linearVelocity = spawnRot * Vector2.right * projectileSpeed;
        }
    }
}
