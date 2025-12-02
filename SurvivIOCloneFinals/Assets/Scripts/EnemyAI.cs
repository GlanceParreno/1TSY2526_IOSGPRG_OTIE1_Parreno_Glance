using UnityEngine;
using System.Collections;

[RequireComponent(typeof(Rigidbody2D))]
public class EnemyAI : MonoBehaviour
{
    public enum State { Patrol, Chase, Attack }

    [Header("Movement")]
    public float patrolSpeed = 1.0f;
    public float chaseSpeed = 2.4f;

    [Header("Detection")]
    public float detectionRadius = 6f;
    public float attackRange = 4f;
    public LayerMask playerLayer; // set to Player layer in Inspector

    [Header("Patrol")]
    public Transform[] patrolPoints; // optional waypoints
    int patrolIndex = 0;

    [Header("Weapon")]
    public Weapon enemyWeapon; // assigned at spawn (inventory currentWeapon)

    [Header("Attack")]
    public float attackCooldown = 0.5f; // fallback if weapon missing
    float nextAttackTime = 0f;

    Transform player;
    Rigidbody2D rb;
    State state = State.Patrol;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    void Start()
    {
        var pObj = GameObject.FindGameObjectWithTag("Player");
        if (pObj != null) player = pObj.transform;
    }

    void Update()
    {
        DecideState();
        ActState();
    }

    void DecideState()
    {
        if (player == null) { state = State.Patrol; return; }
        float dist = Vector2.Distance(transform.position, player.position);

        if (dist <= attackRange) state = State.Attack;
        else if (dist <= detectionRadius) state = State.Chase;
        else state = State.Patrol;
    }

    void ActState()
    {
        switch (state)
        {
            case State.Patrol: DoPatrol(); break;
            case State.Chase: DoChase(); break;
            case State.Attack: DoAttack(); break;
        }
    }

    void DoPatrol()
    {
        if (patrolPoints == null || patrolPoints.Length == 0)
        {
            rb.linearVelocity = Vector2.zero;
            return;
        }

        Transform target = patrolPoints[patrolIndex];
        MoveTowards(target.position, patrolSpeed);
        if (Vector2.Distance(transform.position, target.position) < 0.25f)
            patrolIndex = (patrolIndex + 1) % patrolPoints.Length;
    }

    void DoChase()
    {
        if (player == null) return;
        MoveTowards(player.position, chaseSpeed);
    }

    void DoAttack()
    {
        if (player == null) return;

        // face player
        Vector2 dir = (player.position - transform.position).normalized;
        float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.Euler(0f, 0f, angle);

        // decide when to shoot — prefer weapon.fireRate if available
        float cooldown = (enemyWeapon != null) ? Mathf.Max(0.05f, enemyWeapon.fireRate) : attackCooldown;
        if (Time.time >= nextAttackTime)
        {
            // semi weapons: FireSingle(); autos: StartFiring for continuous attack while in Attack state
            if (enemyWeapon != null)
            {
                if (enemyWeapon.fireMode == FireMode.Semi)
                {
                    enemyWeapon.FireSingle();
                }
                else
                {
                    // start auto if not already
                    enemyWeapon.StartFiring();
                }
            }

            nextAttackTime = Time.time + cooldown;
        }

        // if player moves out of attack range, stop auto fire
        float dist = Vector2.Distance(transform.position, player.position);
        if (dist > attackRange && enemyWeapon != null)
        {
            enemyWeapon.StopFiring();
        }
    }

    void MoveTowards(Vector2 worldPos, float speed)
    {
        Vector2 pos = rb.position;
        Vector2 dir = (worldPos - pos).normalized;
        rb.MovePosition(pos + dir * speed * Time.deltaTime);
    }

    void OnDisable()
    {
        if (enemyWeapon != null) enemyWeapon.StopFiring();
    }
}
