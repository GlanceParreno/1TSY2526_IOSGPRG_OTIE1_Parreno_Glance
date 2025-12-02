using UnityEngine;

[ExecuteAlways]
public class PatrolPoint : MonoBehaviour
{
    public Color gizmoColor = Color.cyan;
    public float gizmoSize = 0.25f;

    void OnDrawGizmos()
    {
        Gizmos.color = gizmoColor;
        Gizmos.DrawSphere(transform.position, gizmoSize);
        Gizmos.DrawLine(transform.position + Vector3.up * (gizmoSize * 2f),
                        transform.position - Vector3.up * (gizmoSize * 2f));
    }
}
