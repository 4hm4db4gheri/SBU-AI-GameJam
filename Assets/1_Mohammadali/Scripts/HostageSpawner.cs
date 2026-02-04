using UnityEngine;

public class HostageSpawner : MonoBehaviour
{
    public GameObject hostagePrefab;
    public float spawnInterval = 30f; // Hostages are rare
    public float spawnRange = 1f;

    void Start()
    {
        SpawnHostage();
    }

    void SpawnHostage()
    {
        Vector2 randPos = Random.insideUnitCircle * spawnRange;
        Vector3 spawnPos = new Vector3(randPos.x, 0, randPos.y) + transform.position;
        
        Instantiate(hostagePrefab, spawnPos, Quaternion.identity);
    }
}