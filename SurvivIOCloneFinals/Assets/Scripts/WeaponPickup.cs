using UnityEngine;

public class WeaponPickup : MonoBehaviour
{
    [Tooltip("Assign the Weapon prefab (Pistol/Shotgun/Auto)")]
    public GameObject weaponPrefab;
    [Tooltip("If player already has this weapon, give ammo instead")]
    public int ammoBonusOnPickup = 0;

    void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;

        Inventory inv = other.GetComponent<Inventory>();
        if (inv == null)
        {
            Debug.LogWarning("Player has no Inventory to receive weapon.");
            Destroy(gameObject);
            return;
        }


        string prefabWeaponName = GetWeaponNameFromPrefab();

        if (inv.currentWeapon != null && inv.currentWeapon.weaponName == prefabWeaponName)
        {

            if (inv.currentWeapon != null)
            {
                inv.AddAmmo(inv.currentWeapon.ammoType, (ammoBonusOnPickup > 0) ? ammoBonusOnPickup : inv.currentWeapon.clipSize);
            }
        }
        else
        {

            GameObject wgo = Instantiate(weaponPrefab);
            Weapon w = wgo.GetComponent<Weapon>();
            if (w == null)
            {
                Debug.LogWarning("Weapon prefab has no Weapon component.");
                Destroy(wgo);
            }
            else
            {
                inv.AddAndEquipWeaponInstance(w);

            }
        }

        Destroy(gameObject);
    }

    string GetWeaponNameFromPrefab()
    {
        if (weaponPrefab == null) return "";
        Weapon w = weaponPrefab.GetComponent<Weapon>();
        return w != null ? w.weaponName : weaponPrefab.name;
    }
}
