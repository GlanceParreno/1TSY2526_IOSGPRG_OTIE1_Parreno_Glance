using UnityEngine;
using System;
using System.Collections.Generic;

/// <summary>
/// Inventory with weapon slots that:
/// - Adds ammo to existing weapon when picking up the same weapon prefab
/// - Places newly added weapons into first empty slot (if any)
/// - SwitchNext/SwitchPrevious skip empty slots and wrap correctly
/// - Provides RemoveWeaponAtIndex helper
/// </summary>
public enum AmmoType
{
    NineMM = 0,
    Shotgun12 = 1,
    Five56 = 2
}

public class Inventory : MonoBehaviour
{
    [Serializable]
    public struct AmmoSlot { public AmmoType type; public int amount; }

    [Header("Ammo Storage (configurable)")]
    public List<AmmoSlot> startingAmmo = new List<AmmoSlot>();

    // internal ammo dictionary
    Dictionary<AmmoType, int> ammoMap = new Dictionary<AmmoType, int>();

    [Header("Weapon Slots")]
    // weapon instances (hidden visuals) — you can equip many and switch
    public List<Weapon> weaponSlots = new List<Weapon>();
    public int currentIndex = -1;           // index into weaponSlots, -1 = none

    // convenience exposed current weapon
    public Weapon currentWeapon
    {
        get
        {
            if (currentIndex >= 0 && currentIndex < weaponSlots.Count)
                return weaponSlots[currentIndex];
            return null;
        }
    }

    // If set, this overrides the amount of ammo added when picking up a duplicate weapon.
    // If zero, the pickup will give 'clipSize' worth of ammo (from the prefab).
    [Header("Pickup")]
    public int ammoBonusOnPickup = 0;

    // notify HUD or other listeners
    public event Action OnAmmoChanged;

    void Awake()
    {
        ammoMap.Clear();

        // initialize map
        foreach (AmmoType t in Enum.GetValues(typeof(AmmoType)))
            ammoMap[t] = 0;

        // apply starting ammo
        foreach (var s in startingAmmo)
            ammoMap[s.type] = s.amount;
    }

    // ---- AMMO METHODS ----

    public void AddAmmo(AmmoType type, int amount)
    {
        if (!ammoMap.ContainsKey(type)) ammoMap[type] = 0;
        ammoMap[type] += amount;
        NotifyAmmoChanged();
    }

    public bool ConsumeAmmo(AmmoType type, int amount)
    {
        if (!ammoMap.ContainsKey(type)) return false;
        if (ammoMap[type] < amount) return false;
        ammoMap[type] -= amount;
        NotifyAmmoChanged();
        return true;
    }

    public int GetAmmo(AmmoType type)
    {
        if (!ammoMap.ContainsKey(type)) return 0;
        return ammoMap[type];
    }

    // ---- WEAPON SLOT / EQUIP API ----

    // Equip an existing Weapon instance (e.g., instantiated from a pickup prefab)
    // This will parent under WeaponHolder, hide visuals (Option A), and add to weaponSlots.
    public void AddAndEquipWeaponInstance(Weapon w)
    {
        if (w == null) return;

        // setup holder
        Transform holder = transform.Find("WeaponHolder");
        if (holder == null)
        {
            GameObject go = new GameObject("WeaponHolder");
            go.transform.SetParent(transform, false);
            go.transform.localPosition = Vector3.zero;
            go.transform.localRotation = Quaternion.identity;
            holder = go.transform;
        }

        // parent under holder
        w.transform.SetParent(holder, false);

        // hide visuals (Option A)
        var srs = w.GetComponentsInChildren<SpriteRenderer>(true);
        foreach (var sr in srs) sr.enabled = false;
        var canvases = w.GetComponentsInChildren<UnityEngine.CanvasRenderer>(true);
        foreach (var c in canvases) c.gameObject.SetActive(false);

        // reset transform
        w.transform.localPosition = Vector3.zero;
        w.transform.localRotation = Quaternion.identity;
        w.transform.localScale = Vector3.one;

        // ensure it doesn't start firing accidentally
        w.StopFiring();

        // --- Insert into first empty slot if available, otherwise append ---
        int insertIndex = -1;
        for (int i = 0; i < weaponSlots.Count; i++)
        {
            if (weaponSlots[i] == null)
            {
                insertIndex = i;
                break;
            }
        }

        if (insertIndex >= 0)
        {
            weaponSlots[insertIndex] = w;
            currentIndex = insertIndex;
        }
        else
        {
            weaponSlots.Add(w);
            currentIndex = weaponSlots.Count - 1;
        }

        // initialize with muzzle
        Transform muzzle = transform.Find("Muzzle");
        if (muzzle == null) muzzle = holder;
        w.Initialize(this, muzzle);

        // ensure only currentWeapon is enabled; disable others
        RefreshActiveWeapon();

        NotifyAmmoChanged();
    }

    /// <summary>
    /// Add weapon from prefab (creates instance internally). Use when pickup provides prefab, not instance.
    /// Behavior:
    /// - If a weapon of the same weaponName already exists in slots, adds ammo to that weapon instead of creating duplicate.
    /// - If not owned, instantiate and add as usual.
    /// </summary>
    public void AddAndEquipWeaponFromPrefab(GameObject weaponPrefab)
    {
        if (weaponPrefab == null) return;

        Weapon prefabWeapon = weaponPrefab.GetComponent<Weapon>();
        if (prefabWeapon == null)
        {
            Debug.LogWarning("Weapon prefab missing Weapon component.");
            return;
        }

        string newName = prefabWeapon.weaponName;
        AmmoType ammoType = prefabWeapon.ammoType;
        int defaultGive = (ammoBonusOnPickup > 0) ? ammoBonusOnPickup : prefabWeapon.clipSize;

        // Search for an existing owned weapon with same weaponName
        for (int i = 0; i < weaponSlots.Count; i++)
        {
            var existing = weaponSlots[i];
            if (existing != null && existing.weaponName == newName)
            {
                // Found same weapon type — add ammo to that weapon
                AddAmmo(ammoType, defaultGive);
                Debug.Log($"Inventory: Already have {newName}. Added {defaultGive} ammo to reserve.");

                // If the existing weapon instance exists, try to refill its clip immediately from reserve
                int need = existing.clipSize - existing.ammoInClip;
                if (need > 0)
                {
                    int take = Mathf.Min(need, GetAmmo(ammoType));
                    if (take > 0)
                    {
                        // consume from reserve, add to clip
                        ConsumeAmmo(ammoType, take);
                        existing.ammoInClip += take;
                        Debug.Log($"Inventory: Refilled {take} bullets into existing {newName} clip.");
                    }
                }

                NotifyAmmoChanged();
                return;
            }
        }

        // Not owned -> instantiate and add
        GameObject go = Instantiate(weaponPrefab);
        Weapon w = go.GetComponent<Weapon>();
        if (w == null)
        {
            Debug.LogWarning("Weapon prefab missing Weapon component.");
            Destroy(go);
            return;
        }

        AddAndEquipWeaponInstance(w);
    }

    // Remove weapon at index (drops / discards). Keeps list compact by setting slot null.
    public void RemoveWeaponAtIndex(int idx)
    {
        if (weaponSlots == null || idx < 0 || idx >= weaponSlots.Count) return;

        var w = weaponSlots[idx];
        if (w != null)
        {
            Destroy(w.gameObject);
        }

        weaponSlots[idx] = null;

        // If removed current, find next available
        if (currentIndex == idx)
        {
            // find next available slot
            int next = -1;
            for (int i = 0; i < weaponSlots.Count; i++)
                if (weaponSlots[i] != null) { next = i; break; }

            if (next >= 0) SwitchToIndex(next);
            else currentIndex = -1;
        }

        NotifyAmmoChanged();
    }

    // Switch next/previous weapon — improved: skip nulls and wrap. Works when you have 1,2,3+ weapons.
    public void SwitchNext()
    {
        if (weaponSlots == null || weaponSlots.Count == 0) return;

        // If nothing equipped, equip first available
        if (currentIndex < 0)
        {
            for (int i = 0; i < weaponSlots.Count; i++)
                if (weaponSlots[i] != null) { SwitchToIndex(i); return; }
            return;
        }

        int count = weaponSlots.Count;
        for (int i = 1; i <= count; i++)
        {
            int idx = (currentIndex + i) % count;
            if (weaponSlots[idx] != null)
            {
                SwitchToIndex(idx);
                return;
            }
        }
    }

    public void SwitchPrevious()
    {
        if (weaponSlots == null || weaponSlots.Count == 0) return;

        // If nothing equipped, equip last available
        if (currentIndex < 0)
        {
            for (int i = weaponSlots.Count - 1; i >= 0; i--)
                if (weaponSlots[i] != null) { SwitchToIndex(i); return; }
            return;
        }

        int count = weaponSlots.Count;
        for (int i = 1; i <= count; i++)
        {
            int idx = (currentIndex - i) % count;
            if (idx < 0) idx += count;
            if (weaponSlots[idx] != null)
            {
                SwitchToIndex(idx);
                return;
            }
        }
    }

    public void SwitchToIndex(int idx)
    {
        if (weaponSlots == null || weaponSlots.Count == 0) return;
        if (idx < 0 || idx >= weaponSlots.Count) return;
        if (idx == currentIndex) return;
        if (weaponSlots[idx] == null) return;

        // stop firing & any coroutines on current
        if (currentWeapon != null)
        {
            currentWeapon.StopFiring();
            // disable current weapon GameObject so it cannot fire / run coroutines
            currentWeapon.gameObject.SetActive(false);
        }

        currentIndex = idx;

        // enable new current
        if (currentWeapon != null)
        {
            currentWeapon.gameObject.SetActive(true);
            Transform muzzle = transform.Find("Muzzle");
            if (muzzle == null) muzzle = transform; // fallback
            currentWeapon.Initialize(this, muzzle);
        }

        NotifyAmmoChanged();
    }

    // helper to make sure only the current weapon is active (call after Add/Equip)
    void RefreshActiveWeapon()
    {
        for (int i = 0; i < weaponSlots.Count; i++)
        {
            var w = weaponSlots[i];
            if (w == null) continue;
            w.gameObject.SetActive(i == currentIndex);
        }
    }

    // ---- EVENT ----

    public void NotifyAmmoChanged()
    {
        OnAmmoChanged?.Invoke();
    }
}
