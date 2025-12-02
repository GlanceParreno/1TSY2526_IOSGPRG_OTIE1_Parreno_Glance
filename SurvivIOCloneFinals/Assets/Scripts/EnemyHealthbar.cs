using UnityEngine;
using UnityEngine.UI;

public class EnemyHealthbar : MonoBehaviour
{
    public Image fillImage;
    public Vector3 offset = new Vector3(0, 1.2f, 0);

    Transform target;

    public void Initialize(Transform followTarget, int maxHealth)
    {
        target = followTarget;
        fillImage.fillAmount = 1f;
    }

    public void UpdateHealth(int current, int max)
    {
        fillImage.fillAmount = (float)current / max;
    }

    void LateUpdate()
    {
        if (target == null)
        {
            Destroy(gameObject);
            return;
        }


        transform.position = target.position + offset;


        transform.rotation = Quaternion.identity;
    }
}
