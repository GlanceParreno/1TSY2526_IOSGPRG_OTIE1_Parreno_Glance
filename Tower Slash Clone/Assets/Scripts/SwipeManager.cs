using UnityEngine;
using System;

public class SwipeManager : MonoBehaviour
{
    public static event Action<Vector2> OnSwipe;
    [SerializeField] private float minSwipeDistance = 50f;

    private Vector2 startTouch;
    private Vector2 endTouch;

    void Update()
    {
        // Touch input
        if (Input.touchCount > 0)
        {
            Touch touch = Input.GetTouch(0);
            if (touch.phase == TouchPhase.Began)
                startTouch = touch.position;
            if (touch.phase == TouchPhase.Ended)
            {
                endTouch = touch.position;
                DetectSwipe();
            }
        }

#if UNITY_EDITOR
        // Mouse swipe for testing in editor
        if (Input.GetMouseButtonDown(0))
            startTouch = Input.mousePosition;
        if (Input.GetMouseButtonUp(0))
        {
            endTouch = Input.mousePosition;
            DetectSwipe();
        }
#endif
    }

    void DetectSwipe()
    {
        Vector2 delta = endTouch - startTouch;
        if (delta.magnitude < minSwipeDistance) return;

        Vector2 direction = delta.normalized;

        // ✅ Debug: print swipe direction to console
        string dirName = GetSwipeDirectionName(direction);
        Debug.Log($"Swipe detected: {dirName} ({direction})");

        OnSwipe?.Invoke(direction);
    }

    private string GetSwipeDirectionName(Vector2 dir)
    {
        // Determine the cardinal direction for easy reading
        if (Mathf.Abs(dir.x) > Mathf.Abs(dir.y))
        {
            return dir.x > 0 ? "Right" : "Left";
        }
        else
        {
            return dir.y > 0 ? "Up" : "Down";
        }
    }
}
