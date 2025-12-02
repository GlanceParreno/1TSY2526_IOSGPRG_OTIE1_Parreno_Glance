using UnityEngine;
using System;
using System.Collections.Generic;

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


    Dictionary<AmmoType, int> ammoMap = new Dictionary<AmmoType, int>();

    [Header("Weapon Slots")]

    public List<Weapon> weaponSlots = new List<Weapon>();
    public int currentIndex = -1;


    public Weapon currentWeapon
    {
        get
        {
            if (currentIndex >= 0 && currentIndex < weaponSlots.Count)
                return weaponSlots[currentIndex];
            return null;
        }
    }


    [Header("Pickup")]
    public int ammoBonusOnPickup = 0;


    public event Action OnAmmoChanged;

    void Awake()
    {
        ammoMap.Clear();


        foreach (AmmoType t in Enum.GetValues(typeof(AmmoType)))
            ammoMap[t] = 0;


        foreach (var s in startingAmmo)
            ammoMap[s.type] = s.amount;
    }


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

    public void AddAndEquipWeaponInstance(Weapon w)
    {
        if (w == null) return;


        Transform holder = transform.Find("WeaponHolder");
        if (holder == null)
        {
            GameObject go = new GameObject("WeaponHolder");
            go.transform.SetParent(transform, false);
            go.transform.localPosition = Vector3.zero;
            go.transform.localRotation = Quaternion.identity;
            holder = go.transform;
        }


        w.transform.SetParent(holder, false);


        var srs = w.GetComponentsInChildren<SpriteRenderer>(true);
        foreach (var sr in srs) sr.enabled = false;
        var canvases = w.GetComponentsInChildren<UnityEngine.CanvasRenderer>(true);
        foreach (var c in canvases) c.gameObject.SetActive(false);


        w.transform.localPosition = Vector3.zero;
        w.transform.localRotation = Quaternion.identity;
        w.transform.localScale = Vector3.one;


        w.StopFiring();


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


        Transform muzzle = transform.Find("Muzzle");
        if (muzzle == null) muzzle = holder;
        w.Initialize(this, muzzle);


        RefreshActiveWeapon();

        NotifyAmmoChanged();
    }

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


        for (int i = 0; i < weaponSlots.Count; i++)
        {
            var existing = weaponSlots[i];
            if (existing != null && existing.weaponName == newName)
            {

                AddAmmo(ammoType, defaultGive);
                Debug.Log($"Inventory: Already have {newName}. Added {defaultGive} ammo to reserve.");


                int need = existing.clipSize - existing.ammoInClip;
                if (need > 0)
                {
                    int take = Mathf.Min(need, GetAmmo(ammoType));
                    if (take > 0)
                    {

                        ConsumeAmmo(ammoType, take);
                        existing.ammoInClip += take;
                        Debug.Log($"Inventory: Refilled {take} bullets into existing {newName} clip.");
                    }
                }

                NotifyAmmoChanged();
                return;
            }
        }


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


    public void RemoveWeaponAtIndex(int idx)
    {
        if (weaponSlots == null || idx < 0 || idx >= weaponSlots.Count) return;

        var w = weaponSlots[idx];
        if (w != null)
        {
            Destroy(w.gameObject);
        }

        weaponSlots[idx] = null;


        if (currentIndex == idx)
        {

            int next = -1;
            for (int i = 0; i < weaponSlots.Count; i++)
                if (weaponSlots[i] != null) { next = i; break; }

            if (next >= 0) SwitchToIndex(next);
            else currentIndex = -1;
        }

        NotifyAmmoChanged();
    }


    public void SwitchNext()
    {
        if (weaponSlots == null || weaponSlots.Count == 0) return;


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


        if (currentWeapon != null)
        {
            currentWeapon.StopFiring();

            currentWeapon.gameObject.SetActive(false);
        }

        currentIndex = idx;


        if (currentWeapon != null)
        {
            currentWeapon.gameObject.SetActive(true);
            Transform muzzle = transform.Find("Muzzle");
            if (muzzle == null) muzzle = transform;
            currentWeapon.Initialize(this, muzzle);
        }

        NotifyAmmoChanged();
    }


    void RefreshActiveWeapon()
    {
        for (int i = 0; i < weaponSlots.Count; i++)
        {
            var w = weaponSlots[i];
            if (w == null) continue;
            w.gameObject.SetActive(i == currentIndex);
        }
    }



    public void NotifyAmmoChanged()
    {
        OnAmmoChanged?.Invoke();
    }
}
