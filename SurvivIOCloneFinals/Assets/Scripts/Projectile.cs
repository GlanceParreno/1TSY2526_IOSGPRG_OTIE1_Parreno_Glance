using System;
using System.Collections;
using System.Reflection;
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D), typeof(Collider2D))]
public class Projectile : MonoBehaviour
{
    [Header("Damage & Motion")]
    public int damage = 10;
    public float speed = 22f;
    public float lifeTime = 3f;

    [Header("Collision")]
    public LayerMask obstacleMask;

    [Header("Pooling")]
    [Tooltip("If true the projectile will be pooled and will disable itself after lifetime instead of Destroy.")]
    public bool usePooling = false;

    [Header("Optional")]
    public string ownerTag = "";

    Rigidbody2D rb;
    Coroutine lifeCoroutine;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        if (rb != null)
        {
            rb.gravityScale = 0f;
            rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
            rb.bodyType = RigidbodyType2D.Dynamic;
        }
    }

    void OnEnable()
    {

        ResetLifetime();
    }

    void OnDisable()
    {
        if (lifeCoroutine != null) StopCoroutine(lifeCoroutine);
        lifeCoroutine = null;
    }

    public void ResetLifetime()
    {
        if (lifeCoroutine != null) StopCoroutine(lifeCoroutine);
        lifeCoroutine = StartCoroutine(LifeTimer());

        if (rb != null)
        {
            rb.linearVelocity = transform.right * speed;
        }
    }

    IEnumerator LifeTimer()
    {
        yield return new WaitForSeconds(lifeTime);

        if (usePooling)
            gameObject.SetActive(false);
        else
            Destroy(gameObject);
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other == null || other.gameObject == null) return;


        if (!string.IsNullOrEmpty(ownerTag) && other.CompareTag(ownerTag))
            return;


        if (((1 << other.gameObject.layer) & obstacleMask) != 0)
        {
            if (usePooling) gameObject.SetActive(false); else Destroy(gameObject);
            return;
        }


        bool damaged = TryApplyDamage(other.gameObject, damage, this.gameObject);
        if (damaged)
        {
            if (usePooling) gameObject.SetActive(false); else Destroy(gameObject);
            return;
        }


    }

    bool TryApplyDamage(GameObject target, int dmg, GameObject instigator)
    {
        var comps = target.GetComponents<MonoBehaviour>();
        foreach (var comp in comps)
        {
            if (comp == null) continue;

            Type t = comp.GetType();
            MethodInfo mi = t.GetMethod("TakeDamage", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
            if (mi == null) continue;

            var pars = mi.GetParameters();
            try
            {
                if (pars.Length == 1 && pars[0].ParameterType == typeof(int))
                {
                    mi.Invoke(comp, new object[] { dmg });
                    return true;
                }
                else if (pars.Length == 2 && pars[0].ParameterType == typeof(int) && pars[1].ParameterType.IsAssignableFrom(typeof(GameObject)))
                {
                    mi.Invoke(comp, new object[] { dmg, instigator });
                    return true;
                }
            }
            catch (Exception ex)
            {
                Debug.LogWarning($"Projectile: error invoking TakeDamage on {t.Name}: {ex.Message}");
            }
        }

        return false;
    }
}
