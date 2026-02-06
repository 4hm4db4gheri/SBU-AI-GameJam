using UnityEngine;

public class EnemySpawner : MonoBehaviour
{
    public float spawnRadius = 1f;
    public float timeBetweenWaves = 5f;
    public int enemiesPerWave = 5;

    private float timer;

    void Start()
    {
        SpawnWave();
    }

    void SpawnWave()
    {
        // Pick a random enemy type
        string type = (Random.value > 0.4f) ? "MeleeEnemy" : "RangedEnemy";
        
        // Get random position in a circle around the spawner
        Vector2 randomCircle = Random.insideUnitCircle.normalized * spawnRadius;
        Vector3 spawnPos = new Vector3(randomCircle.x, 0, randomCircle.y) + transform.position;

        ObjectPooler.Instance.SpawnFromPool(type, spawnPos, Quaternion.identity);
    }
}