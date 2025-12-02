using UnityEngine;
using UnityEngine.UI;
using System;
using System.Collections;

[RequireComponent(typeof(Rigidbody2D))]
public class PlayerController : MonoBehaviour
{
    [Header("Movement & Input")]
    public float moveSpeed = 5f;
    public FixedJoystick moveJoystick;
    public FixedJoystick aimJoystick;

    [Header("References")]
    public Rigidbody2D rb;
    public Transform muzzle;
    public Button shootButton;
    public Button reloadButton;
    public Button swapPrevButton;
    public Button swapNextButton;
    public Inventory inventory;
    public HUDController hud;
    public WorldBounds worldBounds;
    public Health health;

    [Header("Gameplay")]
    public float aimDeadzone = 0.18f;
    public bool allowHoldToFire = true;


    Vector2 moveInput;
    Vector2 aimInput;
    bool isHoldingFire = false;


    private Action onAmmoChangedHandler;

    void Awake()
    {
        if (rb == null) rb = GetComponent<Rigidbody2D>();
    }

    void Start()
    {

        if (moveJoystick == null) Debug.LogWarning($"[{name}] moveJoystick not assigned.");
        if (aimJoystick == null) Debug.LogWarning($"[{name}] aimJoystick not assigned.");
        if (muzzle == null) Debug.LogWarning($"[{name}] muzzle not assigned. Projectiles will spawn at player origin.");
        if (hud == null) Debug.LogWarning($"[{name}] hud not assigned. UI will not update.");
        if (inventory == null) Debug.LogWarning($"[{name}] inventory not assigned. Ammo and weapons will not persist.");


        if (health == null) health = GetComponent<Health>();
        if (health == null) Debug.LogWarning($"[{name}] Health component not found on Player.");


        if (shootButton != null)
        {
            shootButton.onClick.AddListener(OnShootTap);
        }

        if (reloadButton != null)
            reloadButton.onClick.AddListener(OnReloadButton);

        if (swapPrevButton != null)
            swapPrevButton.onClick.AddListener(SwapToPrevious);

        if (swapNextButton != null)
            swapNextButton.onClick.AddListener(SwapToNext);


        if (inventory != null && hud != null)
        {
            onAmmoChangedHandler = () => hud.UpdateAmmoFromInventory(inventory);
            inventory.OnAmmoChanged += onAmmoChangedHandler;

            hud.UpdateAmmoFromInventory(inventory);
        }


        if (health != null && hud != null)
        {
            health.OnHealthChanged += hud.UpdateHealth;
            hud.UpdateHealth(health.currentHealth, health.maxHealth);
        }


        if (inventory != null && inventory.currentWeapon != null)
        {
            Transform m = muzzle != null ? muzzle : transform;
            inventory.currentWeapon.Initialize(inventory, m);

            hud?.UpdateAmmoFromInventory(inventory);
        }
    }

    void Update()
    {

        moveInput = new Vector2(moveJoystick != null ? moveJoystick.Horizontal : 0f,
                                moveJoystick != null ? moveJoystick.Vertical : 0f);
        aimInput = new Vector2(aimJoystick != null ? aimJoystick.Horizontal : 0f,
                                aimJoystick != null ? aimJoystick.Vertical : 0f);


        if (aimInput.magnitude > aimDeadzone)
        {
            float angle = Mathf.Atan2(aimInput.y, aimInput.x) * Mathf.Rad2Deg;
            transform.rotation = Quaternion.Euler(0f, 0f, angle);
        }


        if (inventory != null && hud != null)
            hud.UpdateAmmoFromInventory(inventory);
    }

    void FixedUpdate()
    {
        if (rb == null) return;

        if (moveInput.sqrMagnitude > 0.01f)
        {
            Vector2 desired = rb.position + moveInput.normalized * moveSpeed * Time.fixedDeltaTime;


            if (worldBounds != null)
            {
                Rect r = worldBounds.GetRect();
                desired.x = Mathf.Clamp(desired.x, r.xMin, r.xMax);
                desired.y = Mathf.Clamp(desired.y, r.yMin, r.yMax);
            }

            rb.MovePosition(desired);
        }
    }


    public void OnShootTap()
    {
        if (inventory == null || inventory.currentWeapon == null) return;


        inventory.currentWeapon.FireSingle();
    }


    public void StartFiringHold()
    {
        if (!allowHoldToFire) return;
        isHoldingFire = true;
        if (inventory != null && inventory.currentWeapon != null)
        {
            inventory.currentWeapon.StartFiring();
        }
    }


    public void StopFiringHold()
    {
        if (!allowHoldToFire) return;
        isHoldingFire = false;
        if (inventory != null && inventory.currentWeapon != null)
        {
            inventory.currentWeapon.StopFiring();
        }
    }


    public void OnReloadButton()
    {
        if (inventory != null && inventory.currentWeapon != null)
        {
            StartCoroutine(inventory.currentWeapon.Reload());
        }
    }


    public void SwapToNext()
    {
        if (inventory != null)
            inventory.SwitchNext();
    }

    public void SwapToPrevious()
    {
        if (inventory != null)
            inventory.SwitchPrevious();
    }

    public void OnDeath()
    {

        enabled = false;
        if (rb != null) rb.linearVelocity = Vector2.zero;
        Debug.Log("[PlayerController] Player died.");
        if (GameManager.Instance != null) GameManager.Instance.OnPlayerDeath();
    }

    void OnDestroy()
    {

        if (inventory != null && onAmmoChangedHandler != null)
            inventory.OnAmmoChanged -= onAmmoChangedHandler;

        if (health != null && hud != null)
            health.OnHealthChanged -= hud.UpdateHealth;

        if (shootButton != null)
            shootButton.onClick.RemoveListener(OnShootTap);
        if (reloadButton != null)
            reloadButton.onClick.RemoveListener(OnReloadButton);
        if (swapPrevButton != null)
            swapPrevButton.onClick.RemoveListener(SwapToPrevious);
        if (swapNextButton != null)
            swapNextButton.onClick.RemoveListener(SwapToNext);
    }


    public Vector2 ClampToBounds(Vector2 position)
    {
        if (worldBounds == null) return position;
        Rect r = worldBounds.GetRect();
        position.x = Mathf.Clamp(position.x, r.xMin, r.xMax);
        position.y = Mathf.Clamp(position.y, r.yMin, r.yMax);
        return position;
    }
    public void OnPlayerWin()
    {
        Debug.Log("[PlayerController] Player wins!");
        enabled = false;
        if (rb != null) rb.linearVelocity = Vector2.zero;

        if (GameManager.Instance != null)
            GameManager.Instance.OnPlayerWin();
    }
}
