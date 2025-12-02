using UnityEngine;
using System.Collections;
public enum FireMode
{
    Semi,
    Auto
}

public class Weapon : MonoBehaviour
{
    [Header("Weapon Info")]
    public string weaponName = "Weapon";
    public AmmoType ammoType = AmmoType.NineMM;

    [Header("Stats")]
    public int clipSize = 12;
    public float fireRate = 0.3f;       // seconds between shots (also used as min tap interval for Semi)
    public float reloadTime = 1.0f;
    public FireMode fireMode = FireMode.Semi;

    [Header("Runtime")]
    public int ammoInClip;
    public bool isFiring = false;
    public bool isReloading = false;

    protected Inventory ownerInventory;
    protected Transform muzzle;

    // internal timing guards
    private float _lastSingleFireTime = -10f;
    private const float kMicroGuard = 0.06f; // tiny guard for autos/accidental duplicates

    protected virtual void Awake()
    {
        ammoInClip = clipSize;
    }

    public virtual void Initialize(Inventory inv, Transform muzzlePoint)
    {
        ownerInventory = inv;
        muzzle = muzzlePoint;
    }

    // Entry point for start firing (called by PlayerController for hold)
    public virtual void StartFiring()
    {
        if (fireMode == FireMode.Auto)
        {
            if (!isFiring)
            {
                isFiring = true;
                StartCoroutine(FireLoop());
            }
        }
        else
        {
            // If someone calls StartFiring() for Semi (not recommended for tap),
            // treat it as a single shot to avoid starting a loop.
            TryFireSingleShot();
        }
    }

    public virtual void StopFiring()
    {
        isFiring = false;
    }

    protected virtual IEnumerator FireLoop()
    {
        while (isFiring)
        {
            if (ammoInClip <= 0)
            {
                StartCoroutine(Reload());
                yield break;
            }

            FireOneShot();
            ammoInClip--;
            ownerInventory?.NotifyAmmoChanged();

            yield return new WaitForSeconds(fireRate);
        }
    }

    // Attempt a single shot (used internally)
    protected void TryFireSingleShot()
    {
        if (isReloading) return;

        if (ammoInClip <= 0)
        {
            OwnerStartReload();
            return;
        }

        FireOneShot();
        ammoInClip--;
        ownerInventory?.NotifyAmmoChanged();
    }

    protected void OwnerStartReload()
    {
        if (isReloading) return;
        StartCoroutine(Reload());
    }

    public void FireSingle()
    {
        float now = Time.time;
        // For semi weapons we enforce the weapon's fireRate as the min interval between taps.
        float minInterval = (fireMode == FireMode.Semi) ? Mathf.Max(0.0001f, fireRate) : kMicroGuard;

        if (now - _lastSingleFireTime < minInterval)
        {
            // Too soon — ignore duplicate tap
            return;
        }
        _lastSingleFireTime = now;

        if (isReloading) return;

        if (ammoInClip <= 0)
        {
            StartCoroutine(Reload());
            return;
        }

        // perform shot
        FireOneShot();
        ammoInClip--;
        ownerInventory?.NotifyAmmoChanged();
    }

    // Override in derived weapons to implement projectile/spread behavior
    protected virtual void FireOneShot() { }

    public virtual IEnumerator Reload()
    {
        if (isReloading) yield break;
        isReloading = true;

        yield return new WaitForSeconds(reloadTime);

        if (ownerInventory != null)
        {
            int available = ownerInventory.GetAmmo(ammoType);
            int need = clipSize - ammoInClip;
            int take = Mathf.Min(available, need);
            ammoInClip += take;
            ownerInventory.ConsumeAmmo(ammoType, take);
        }

        isReloading = false;
        ownerInventory?.NotifyAmmoChanged();
    }
}
