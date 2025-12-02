using UnityEngine;

[RequireComponent(typeof(Camera))]
public class CameraFollow : MonoBehaviour
{
    [Header("Follow")]
    public Transform target;
    public float smoothSpeed = 0.12f;
    public Vector3 offset = new Vector3(0f, 0f, -10f);

    [Header("Bounds (optional)")]
    public WorldBounds worldBounds;

    Camera cam;

    void Awake()
    {
        cam = GetComponent<Camera>();
        if (cam == null) cam = Camera.main;
    }

    void LateUpdate()
    {
        if (target == null) return;

        Vector3 desiredPosition = target.position + offset;
        desiredPosition.z = offset.z; // ensure z is consistent

        // Clamp camera to world bounds if provided (orthographic)
        if (worldBounds != null && cam != null && cam.orthographic)
        {
            Rect r = worldBounds.GetRect();
            float vertExtent = cam.orthographicSize;
            float horzExtent = vertExtent * cam.aspect;

            float minX = r.xMin + horzExtent;
            float maxX = r.xMax - horzExtent;
            float minY = r.yMin + vertExtent;
            float maxY = r.yMax - vertExtent;

            // In case world is smaller than camera view, center it
            if (minX > maxX) { minX = maxX = r.center.x; }
            if (minY > maxY) { minY = maxY = r.center.y; }

            desiredPosition.x = Mathf.Clamp(desiredPosition.x, minX, maxX);
            desiredPosition.y = Mathf.Clamp(desiredPosition.y, minY, maxY);
        }

        transform.position = Vector3.Lerp(transform.position, desiredPosition, smoothSpeed);
    }
}
