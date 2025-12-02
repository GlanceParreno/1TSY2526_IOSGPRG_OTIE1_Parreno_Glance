using UnityEngine;

public class AmmoSpawnManager : MonoBehaviour
{
    public WorldBounds bounds;
    public GameObject[] ammoPrefabs; // assign 9mm, 12g, 5.56 prefabs
    public int spawnCount = 40;

    void Start()
    {
        if (bounds == null) return;
        Rect r = bounds.GetRect();
        for (int i = 0; i < spawnCount; i++)
        {
            Vector2 p = new Vector2(Random.Range(r.xMin, r.xMax), Random.Range(r.yMin, r.yMax));
            int idx = Random.Range(0, ammoPrefabs.Length);
            Instantiate(ammoPrefabs[idx], p, Quaternion.identity);
        }
    }
}
