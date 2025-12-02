using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class HUDController : MonoBehaviour
{
    [Header("Health UI (assign one or more)")]
    public Slider healthSlider;
    public Image healthFillImage;
    public TextMeshProUGUI healthTextTMP;

    [Header("Ammo & Weapon UI")]
    public TextMeshProUGUI weaponNameText;
    public TextMeshProUGUI ammoTextTMP;
    public Text ammoTextLegacy;

    [Header("Other UI")]
    public GameObject gameOverPanel;

    void Start()
    {

        if (gameOverPanel != null) gameOverPanel.SetActive(false);
    }


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


    public void ShowGameOver()
    {
        if (gameOverPanel != null) gameOverPanel.SetActive(true);
    }
}
