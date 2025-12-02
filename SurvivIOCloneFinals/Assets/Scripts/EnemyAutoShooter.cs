using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Transform))]
public class EnemyAutoShooter : MonoBehaviour
{
    [Header("Detection")]
    public string playerTag = "Player";
    public float detectionRadius = 6f;
    public float attackRange = 4f;
    [Tooltip("Layer mask used for LOS checks. Leave empty to skip LOS test.")]
    public LayerMask obstacleMask;

    [Header("Firing / Pooling")]
    public GameObject projectilePrefab;
    public Transform muzzle;               // where projectiles spawn (child transform)
    public int poolSize = 16;              // number of pooled projectiles
    public float fireRate = 1.0f;          // seconds between shots
    public float projectileSpeed = 20f;
    public int projectileDamage = 10;
    public float muzzleOffset = 0.12f;     // spawn offset from muzzle
    public string ownerTag = "Enemy";      // used so projectile won't hit the shooter

    [Header("Clip & Reload")]
    public int clipSize = 4;               // number of shots before reload
    public float reloadTime = 1.25f;       // reload pause duration

    [Header("Behavior")]
    public bool rotateToFacePlayer = true; // face the player when attacking
    public bool requireLineOfSight = true; // if true do a raycast to check LOS

    // runtime
    Transform player;
    Coroutine firingRoutine;
    float nextFireTime;
    int shotsThisClip = 0;
    bool isReloading = false;

    // pooling
    List<GameObject> pool;
    int poolCursor = 0;

    void Awake()
    {
        // prepare pool container
        pool = new List<GameObject>(poolSize);
    }

    void Start()
    {
        // find player by tag
        var p = GameObject.FindGameObjectWithTag(playerTag);
        if (p != null) player = p.transform;

        // if muzzle not assigned, try to locate a child named "Muzzle"
        if (muzzle == null)
        {
            var m = transform.Find("Muzzle");
            if (m != null) muzzle = m;
        }

        if (muzzle == null) muzzle = transform;

        // build pool
        BuildPool();
    }

    void BuildPool()
    {
        if (projectilePrefab == null)
        {
            Debug.LogWarning($"[{name}] EnemyAutoShooter: projectilePrefab not assigned.");
            return;
        }

        // instantiate disabled pooled projectiles
        for (int i = 0; i < poolSize; i++)
        {
            GameObject go = Instantiate(projectilePrefab, Vector3.zero, Quaternion.identity);
            go.SetActive(false);

            // mark projectile as pooled so it will disable itself rather than destroy
            var proj = go.GetComponent<Projectile>();
            if (proj != null)
            {
                proj.usePooling = true;
            }

            pool.Add(go);
        }
        poolCursor = 0;
    }

    GameObject GetPooledProjectile()
    {
        if (pool == null || pool.Count == 0) return null;

        // simple round-robin search for next inactive projectile
        for (int i = 0; i < pool.Count; i++)
        {
            poolCursor = (poolCursor + 1) % pool.Count;
            GameObject g = pool[poolCursor];
            if (g != null && !g.activeInHierarchy)
                return g;
        }

        // none available: optionally expand pool by instantiating one (guarded)
        // Instantiate extra if pool exhausted
        GameObject extra = Instantiate(projectilePrefab, Vector3.zero, Quaternion.identity);
        var proj = extra.GetComponent<Projectile>();
        if (proj != null) proj.usePooling = true;
        pool.Add(extra);
        return extra;
    }

    void Update()
    {
        if (player == null) return;

        float dist = Vector2.Distance(transform.position, player.position);

        bool inAttack = dist <= attackRange && dist <= detectionRadius;

        if (inAttack && (!requireLineOfSight || HasLineOfSight()))
        {
            if (firingRoutine == null)
            {
                firingRoutine = StartCoroutine(FireLoop());
            }
        }
        else
        {
            if (firingRoutine != null)
            {
                StopCoroutine(firingRoutine);
                firingRoutine = null;
            }
        }
    }

    bool HasLineOfSight()
    {
        if (player == null) return false;
        if (obstacleMask == 0) return true;

        Vector2 from = (Vector2)muzzle.position;
        Vector2 to   = (Vector2)player.position;
        Vector2 dir  = (to - from).normalized;
        float dist    = Vector2.Distance(from, to);

        RaycastHit2D hit = Physics2D.Raycast(from, dir, dist, obstacleMask);
        return hit.collider == null;
    }

    IEnumerator FireLoop()
    {
        while (true)
        {
            if (player == null) yield break;
            if (isReloading) { yield return null; continue; }

            // Face the player
            if (rotateToFacePlayer)
            {
                Vector2 dir = (player.position - transform.position).normalized;
                float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
                transform.rotation = Quaternion.Euler(0f, 0f, angle);
            }

            if (Time.time >= nextFireTime)
            {
                // shoot
                SpawnProjectilePooled();
                shotsThisClip++;

                // clip/reload handling
                if (shotsThisClip >= clipSize)
                {
                    // start reload
                    StartCoroutine(ReloadRoutine());
                }

                nextFireTime = Time.time + Mathf.Max(0.01f, fireRate);
            }

            yield return null;
        }
    }

    void SpawnProjectilePooled()
    {
        if (projectilePrefab == null || muzzle == null) return;

        GameObject pgo = GetPooledProjectile();
        if (pgo == null) return;

        // position & rotation
        pgo.transform.position = muzzle.position + muzzle.right * muzzleOffset;
        pgo.transform.rotation = muzzle.rotation;
        pgo.SetActive(true);

        // configure projectile values (ownerTag, damage, speed)
        var proj = pgo.GetComponent<Projectile>();
        if (proj != null)
        {
            proj.ownerTag = ownerTag;
            proj.damage = projectileDamage;
            proj.speed = projectileSpeed;
            proj.ResetLifetime(); // notify projectile to restart its lifetime coroutine if needed
        }

        // ensure velocity is correct right away (some projectiles use rb on Start/OnEnable)
        var rb = pgo.GetComponent<Rigidbody2D>();
        if (rb != null)
        {
            rb.bodyType = RigidbodyType2D.Dynamic;
            rb.gravityScale = 0f;
            rb.linearVelocity = pgo.transform.right * projectileSpeed;
        }
    }

    IEnumerator ReloadRoutine()
    {
        isReloading = true;
        // optional: stop firing sound/animation here
        yield return new WaitForSeconds(reloadTime);
        shotsThisClip = 0;
        isReloading = false;
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, detectionRadius);

        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackRange);

        if (muzzle != null)
        {
            Gizmos.color = Color.cyan;
            Gizmos.DrawLine(muzzle.position, muzzle.position + muzzle.right * 0.5f);
        }
    }

    // Optional helper to clear pool (editor)
    [ContextMenu("ClearPool")]
    void ClearPool()
    {
        if (pool == null) return;
        foreach (var g in pool)
        {
            if (g != null) DestroyImmediate(g);
        }
        pool.Clear();
    }
}
