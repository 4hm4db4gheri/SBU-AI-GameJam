using UnityEngine;

public class Spawner : MonoBehaviour
{
    public GameObject meleePrefab;
    public GameObject rangedPrefab;

    public float spawnInterval = 5f; // هر 5 ثانیه
    private float timer;

    void Update()
    {
        timer += Time.deltaTime;
        if (timer >= spawnInterval)
        {
            SpawnEnemy();
            timer = 0;
        }
    }

    void SpawnEnemy()
    {
        // انتخاب تصادفی نوع دشمن
        GameObject prefabToSpawn = (Random.value > 0.5f) ? meleePrefab : rangedPrefab;

        // مکان تصادفی (روی زمین)
        float x = Random.Range(-10f, 10f);
        float z = Random.Range(-10f, 10f);
        Vector3 pos = new Vector3(x, 0.5f, z);

        Instantiate(prefabToSpawn, pos, Quaternion.identity);
    }
}