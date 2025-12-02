using UnityEngine;
using TMPro;
using System.Collections;

[RequireComponent(typeof(Collider2D))]
public class AmmoPickup : MonoBehaviour
{
    [Header("Pickup Settings")]
    public AmmoType ammoType = AmmoType.NineMM;
    public int minAmount = 8;
    public int maxAmount = 20;
    public bool randomBetweenMinMax = true;

    [Header("Optional UI")]
    public GameObject pickupPopupPrefab;
    public float popupLifetime = 1.0f;

    [Header("Behaviour")]
    public bool destroyOnPickup = true;
    public bool onlyPlayerCanPickup = true;

    int GetAmount()
    {
        if (!randomBetweenMinMax) return minAmount;
        return Random.Range(minAmount, maxAmount + 1);
    }

    void Reset()
    {

        var col = GetComponent<Collider2D>();
        if (col != null) col.isTrigger = true;
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (onlyPlayerCanPickup && !other.CompareTag("Player")) return;

        Inventory inv = other.GetComponent<Inventory>();
        if (inv == null)
        {

            inv = other.GetComponentInParent<Inventory>();
        }

        if (inv == null)
        {

            return;
        }

        int amount = GetAmount();
        inv.AddAmmo(ammoType, amount);


        if (pickupPopupPrefab != null)
        {
            StartCoroutine(SpawnPopup(amount));
        }

        if (destroyOnPickup)
            Destroy(gameObject);
    }

    IEnumerator SpawnPopup(int amount)
    {
        if (pickupPopupPrefab == null) yield break;
        GameObject go = Instantiate(pickupPopupPrefab, transform.position, Quaternion.identity);

        var tmp = go.GetComponentInChildren<TextMeshProUGUI>();
        if (tmp != null) tmp.text = $"+{amount}";

        yield return new WaitForSeconds(popupLifetime);
        if (go != null) Destroy(go);
    }
}
