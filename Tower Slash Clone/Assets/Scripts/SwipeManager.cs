using UnityEngine;
using System;

public class SwipeManager : MonoBehaviour
{
    public static event Action<Vector2> OnSwipe;

    [Header("Swipe Settings")]
    public float minSwipeDistance = 50f; // pixels

    private Vector2 startTouchPosition;
    private Vector2 endTouchPosition;

    private void Update()
    {
        // Handle both mouse (PC) and touch (mobile)
#if UNITY_EDITOR || UNITY_STANDALONE
        HandleMouseInput();
#else
        HandleTouchInput();
#endif
    }

    // ✅ --- Mouse Input for Editor / PC Testing ---
    private void HandleMouseInput()
    {
        if (Input.GetMouseButtonDown(0))
            startTouchPosition = Input.mousePosition;

        if (Input.GetMouseButtonUp(0))
        {
            endTouchPosition = Input.mousePosition;
            ProcessSwipe(endTouchPosition - startTouchPosition);
        }
    }

    // ✅ --- Touch Input for Mobile ---
    private void HandleTouchInput()
    {
        if (Input.touchCount > 0)
        {
            Touch touch = Input.GetTouch(0);

            if (touch.phase == TouchPhase.Began)
                startTouchPosition = touch.position;

            else if (touch.phase == TouchPhase.Ended)
            {
                endTouchPosition = touch.position;
                ProcessSwipe(endTouchPosition - startTouchPosition);
            }
        }
    }

    // ✅ --- Swipe & Tap Detection ---
    private void ProcessSwipe(Vector2 delta)
    {
        // Tap (short movement)
        if (delta.magnitude < minSwipeDistance)
        {
            Debug.Log("⚡ Tap detected! Attempting to trigger Dash...");
            FindFirstObjectByType<DashGauge>()?.UseDash(); // <-- TAP DASH
            return;
        }

        // Swipe
        Vector2 dir = delta.normalized;
        OnSwipe?.Invoke(dir);

        // Optional: Debug direction in console
        if (Mathf.Abs(dir.x) > Mathf.Abs(dir.y))
            Debug.Log(dir.x > 0 ? "👉 Swiped Right" : "👈 Swiped Left");
        else
            Debug.Log(dir.y > 0 ? "⬆️ Swiped Up" : "⬇️ Swiped Down");
    }
}
