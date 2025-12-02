using UnityEngine;

public class WorldBounds : MonoBehaviour
{
    [Header("World Bounds (X, Y are center)")]
    public float width = 100f;
    public float height = 100f;

    public Rect GetRect()
    {
        Vector2 center = (Vector2)transform.position;
        float halfW = width * 0.5f;
        float halfH = height * 0.5f;
        return new Rect(center.x - halfW, center.y - halfH, width, height);
    }

    // Optional draw gizmo in editor
    void OnDrawGizmosSelected()
    {
        Gizmos.color = new Color(0,1,0,0.15f);
        Rect r = GetRect();
        Gizmos.DrawCube(new Vector3(r.center.x, r.center.y, 0f), new Vector3(r.width, r.height, 0f));
        Gizmos.color = Color.green;
        Gizmos.DrawWireCube(new Vector3(r.center.x, r.center.y, 0f), new Vector3(r.width, r.height, 0f));
    }
}
