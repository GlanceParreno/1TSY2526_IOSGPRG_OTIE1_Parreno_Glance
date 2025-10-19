using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class DashGauge : MonoBehaviour
{
    [Header("UI Elements")]
    public Image fillBar; // Assign your DashFill UI Image here

    [Header("Dash Settings")]
    [Tooltip("How much charge is added per enemy kill.")]
    public float chargePerKill = 0.1f;
    public float currentFill = 0f;
    public float maxFill = 1f;
    public bool isDashing = false;

    [Header("Visual Settings")]
    [Tooltip("Color when the gauge is empty.")]
    public Color emptyColor = Color.blue;
    [Tooltip("Color when the gauge is full.")]
    public Color fullColor = Color.yellow;
    [Tooltip("Flash speed when gauge is full.")]
    public float flashSpeed = 2f;

    private bool flashActive = false;
    private Coroutine flashRoutine;

    // ✅ Adds charge from successful slashes
    public void AddCharge(float amount)
    {
        if (isDashing) return;

        currentFill = Mathf.Clamp01(currentFill + amount);
        UpdateUI();

        // Start flashing if full
        if (currentFill >= maxFill && !flashActive)
        {
            flashRoutine = StartCoroutine(FlashFullGauge());
        }
    }

    // ✅ Use dash when gauge is full
    public void UseDash()
    {
        if (currentFill >= maxFill && !isDashing)
        {
            StartCoroutine(DashRoutine());
        }
        else
        {
            Debug.Log("⚡ Dash not ready yet!");
        }
    }

    // ✅ Dash routine clears enemies and resets gauge
    private IEnumerator DashRoutine()
    {
        isDashing = true;
        Debug.Log("⚡ DASH ACTIVATED!");

        // Stop flashing when used
        if (flashRoutine != null)
        {
            StopCoroutine(flashRoutine);
            flashRoutine = null;
            flashActive = false;
        }

        currentFill = 0f;
        UpdateUI();

        // Destroy all enemies
        EnemyController[] enemies = FindObjectsByType<EnemyController>(FindObjectsSortMode.None);
        foreach (EnemyController e in enemies)
        {
            Destroy(e.gameObject);
        }

        yield return new WaitForSeconds(1.5f); // short delay before can recharge
        isDashing = false;

        Debug.Log("⚡ Dash ended, gauge reset.");
    }

    // ✅ Updates the fill bar and color smoothly
    private void UpdateUI()
    {
        if (fillBar == null) return;

        fillBar.fillAmount = currentFill / maxFill;

        if (!flashActive)
        {
            fillBar.color = Color.Lerp(emptyColor, fullColor, fillBar.fillAmount);
        }
    }

    // ✨ Flash the bar when full to show it's ready
    private IEnumerator FlashFullGauge()
    {
        flashActive = true;
        Debug.Log("⚡ Dash ready — gauge flashing!");

        while (currentFill >= maxFill)
        {
            float t = (Mathf.Sin(Time.time * flashSpeed) + 1f) / 2f;
            fillBar.color = Color.Lerp(fullColor, Color.white, t);
            yield return null;
        }

        flashActive = false;
        fillBar.color = emptyColor;
    }
}
