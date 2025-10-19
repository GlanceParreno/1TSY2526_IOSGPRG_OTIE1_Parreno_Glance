using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    public Transform player;
    public float smoothSpeed = 5f;
    public float yOffset = 2f;

    private float highestY;

    void Start()
    {
        highestY = transform.position.y;
    }

    void LateUpdate()
    {
        if (player == null) return;

        float targetY = player.position.y + yOffset;

        if (targetY > highestY)
        {
            highestY = Mathf.Lerp(highestY, targetY, smoothSpeed * Time.deltaTime);
            transform.position = new Vector3(transform.position.x, highestY, transform.position.z);
        }
    }
}
