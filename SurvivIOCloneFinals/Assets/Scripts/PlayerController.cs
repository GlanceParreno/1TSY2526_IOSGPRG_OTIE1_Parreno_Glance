using UnityEngine;
using UnityEngine.UI;
using System;
using System.Collections;

[RequireComponent(typeof(Rigidbody2D))]
public class PlayerController : MonoBehaviour
{
    [Header("Movement & Input")]
    public float moveSpeed = 5f;
    public FixedJoystick moveJoystick;   // left joystick (Joystick Pack)
    public FixedJoystick aimJoystick;    // right joystick (Joystick Pack)

    [Header("References")]
    public Rigidbody2D rb;
    public Transform muzzle;             // projectile spawn point (child transform)
    public Button shootButton;           // OnClick -> OnShootTap
    public Button reloadButton;          // OnClick -> OnReloadButton
    public Button swapPrevButton;        // OnClick -> SwapToPrevious
    public Button swapNextButton;        // OnClick -> SwapToNext
    public Inventory inventory;          // player's inventory (for ammo and weapons)
    public HUDController hud;            // HUD controller to update UI
    public WorldBounds worldBounds;      // optional world bounds to clamp movement
    public Health health;                // player's Health component (optional, auto-find)

    [Header("Gameplay")]
    public float aimDeadzone = 0.18f;    // joystick deadzone for aiming
    public bool allowHoldToFire = true;  // if true you can set up EventTrigger PointerDown/Up to call StartFiringHold/StopFiringHold

    // internal
    Vector2 moveInput;
    Vector2 aimInput;
    bool isHoldingFire = false;

    // local reference to inventory event so we can unsubscribe cleanly
    private Action onAmmoChangedHandler;

    void Awake()
    {
        if (rb == null) rb = GetComponent<Rigidbody2D>();
    }

    void Start()
    {
        // Defensive warnings
        if (moveJoystick == null) Debug.LogWarning($"[{name}] moveJoystick not assigned.");
        if (aimJoystick == null) Debug.LogWarning($"[{name}] aimJoystick not assigned.");
        if (muzzle == null) Debug.LogWarning($"[{name}] muzzle not assigned. Projectiles will spawn at player origin.");
        if (hud == null) Debug.LogWarning($"[{name}] hud not assigned. UI will not update.");
        if (inventory == null) Debug.LogWarning($"[{name}] inventory not assigned. Ammo and weapons will not persist.");

        // Auto-find health/hud if not assigned
        if (health == null) health = GetComponent<Health>();
        if (health == null) Debug.LogWarning($"[{name}] Health component not found on Player.");

        // Button wiring (OnClick)
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

        // Subscribe to inventory updates to update HUD
        if (inventory != null && hud != null)
        {
            onAmmoChangedHandler = () => hud.UpdateAmmoFromInventory(inventory);
            inventory.OnAmmoChanged += onAmmoChangedHandler;
            // initial HUD refresh
            hud.UpdateAmmoFromInventory(inventory);
        }

        // Subscribe to health updates for HUD
        if (health != null && hud != null)
        {
            health.OnHealthChanged += hud.UpdateHealth;
            hud.UpdateHealth(health.currentHealth, health.maxHealth);
        }

        // If there's already an equipped weapon in inventory when scene starts, ensure it is initialized
        if (inventory != null && inventory.currentWeapon != null)
        {
            Transform m = muzzle != null ? muzzle : transform;
            inventory.currentWeapon.Initialize(inventory, m);
            // ensure HUD shows correct weapon name/ammo
            hud?.UpdateAmmoFromInventory(inventory);
        }
    }

    void Update()
    {
        // Read inputs
        moveInput = new Vector2(moveJoystick != null ? moveJoystick.Horizontal : 0f,
                                moveJoystick != null ? moveJoystick.Vertical : 0f);
        aimInput  = new Vector2(aimJoystick != null ? aimJoystick.Horizontal : 0f,
                                aimJoystick != null ? aimJoystick.Vertical : 0f);

        // Aiming: rotate to right joystick if outside deadzone
        if (aimInput.magnitude > aimDeadzone)
        {
            float angle = Mathf.Atan2(aimInput.y, aimInput.x) * Mathf.Rad2Deg;
            transform.rotation = Quaternion.Euler(0f, 0f, angle);
        }

        // Optional: keep HUD ammo in sync (cheap, safe)
        if (inventory != null && hud != null)
            hud.UpdateAmmoFromInventory(inventory);
    }

    void FixedUpdate()
    {
        if (rb == null) return;

        if (moveInput.sqrMagnitude > 0.01f)
        {
            Vector2 desired = rb.position + moveInput.normalized * moveSpeed * Time.fixedDeltaTime;

            // Clamp to world bounds if provided
            if (worldBounds != null)
            {
                Rect r = worldBounds.GetRect();
                desired.x = Mathf.Clamp(desired.x, r.xMin, r.xMax);
                desired.y = Mathf.Clamp(desired.y, r.yMin, r.yMax);
            }

            rb.MovePosition(desired);
        }
    }

    // Called by Shoot button OnClick (single tap)
    public void OnShootTap()
    {
        if (inventory == null || inventory.currentWeapon == null) return;

        // single-shot wrapper on the weapon ensures exactly one shot per tap
        inventory.currentWeapon.FireSingle();
    }

    // For auto-weapon hold: call on PointerDown event (EventTrigger -> PointerDown)
    public void StartFiringHold()
    {
        if (!allowHoldToFire) return;
        isHoldingFire = true;
        if (inventory != null && inventory.currentWeapon != null)
        {
            inventory.currentWeapon.StartFiring();
        }
    }

    // For auto-weapon hold: call on PointerUp event (EventTrigger -> PointerUp)
    public void StopFiringHold()
    {
        if (!allowHoldToFire) return;
        isHoldingFire = false;
        if (inventory != null && inventory.currentWeapon != null)
        {
            inventory.currentWeapon.StopFiring();
        }
    }

    // Reload button pressed
    public void OnReloadButton()
    {
        if (inventory != null && inventory.currentWeapon != null)
        {
            StartCoroutine(inventory.currentWeapon.Reload());
        }
    }

    // Swap buttons
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
        // Disable movement/controls
        enabled = false;
        if (rb != null) rb.linearVelocity = Vector2.zero;
        Debug.Log("[PlayerController] Player died.");
        if (GameManager.Instance != null) GameManager.Instance.OnPlayerDeath();
    }

    void OnDestroy()
    {
        // Unsubscribe to prevent leaks
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

    // Optional: editor-friendly helper to clamp a position externally
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
             enabled = false;                 // stop player input
        if (rb != null) rb.linearVelocity = Vector2.zero;

         if (GameManager.Instance != null)
             GameManager.Instance.OnPlayerWin(); 
    }   
}
