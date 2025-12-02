using UnityEngine;
using UnityEngine.UI;
using TMPro; // optional - if you don't use TMP, you can remove this using and change fields to Text

public class HUDController : MonoBehaviour
{
    [Header("Health UI (assign one or more)")]
    public Slider healthSlider;           // optional: assign your Slider
    public Image healthFillImage;         // optional: assign Image (type=Filled)
    public TextMeshProUGUI healthTextTMP; // optional numeric display

    [Header("Ammo & Weapon UI")]
    public TextMeshProUGUI weaponNameText; // optional (TMP). If you use legacy Text, replace type.
    public TextMeshProUGUI ammoTextTMP;    // ammo display (clip / reserve)
    public Text ammoTextLegacy;            // legacy Text fallback (if not using TMP)

    [Header("Other UI")]
    public GameObject gameOverPanel;       // optional panel to show on death

    void Start()
    {
        // initial state checks can be performed by PlayerController via subscriptions
        if (gameOverPanel != null) gameOverPanel.SetActive(false);
    }

    // Called by Health.OnHealthChanged
    public void UpdateHealth(int current, int max)
    {
        if (healthSlider != null)
        {
            healthSlider.maxValue = Mathf.Max(1, max);
            healthSlider.value = Mathf.Clamp(current, 0, max);
        }

        if (healthFillImage != null)
        {
            float t = (max > 0) ? (float)current / (float)max : 0f;
            healthFillImage.fillAmount = Mathf.Clamp01(t);
        }

        if (healthTextTMP != null)
        {
            healthTextTMP.text = $"{current} / {max}";
        }
    }

    // Called by Inventory / PlayerController to update ammo+weapon display
    public void UpdateAmmoFromInventory(Inventory inv)
    {
        if (inv == null || inv.currentWeapon == null)
        {
            SetAmmoText("—", 0, 0);
            return;
        }

        var w = inv.currentWeapon;
        int clip = w.ammoInClip;
        int reserve = inv.GetAmmo(w.ammoType);

        SetAmmoText(w.weaponName, clip, reserve);
    }

    void SetAmmoText(string weaponName, int clip, int reserve)
    {
        if (weaponNameText != null)
            weaponNameText.text = weaponName;

        string ammoStr = $"{clip} / {reserve}";
        if (ammoTextTMP != null)
            ammoTextTMP.text = ammoStr;

        if (ammoTextLegacy != null)
            ammoTextLegacy.text = ammoStr;
    }

    // Called by PlayerController when player dies (if wired) - show Game Over
    public void ShowGameOver()
    {
        if (gameOverPanel != null) gameOverPanel.SetActive(true);
    }
}
