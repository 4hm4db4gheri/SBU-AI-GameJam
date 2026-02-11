using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AmirEnemyController : MonoBehaviour
{
    [Header("Spawn Points (12)")]
    [SerializeField] private Transform[] spawnPoints; // drag your 12 points here

    [Header("Enemy Prefab")]
    [SerializeField] private GameObject enemyPrefab;

    [Header("Waves")]
    [Tooltip("Enemies per wave. Example: 2,4,5,6. If empty, uses auto growth.")]
    [SerializeField] private int[] enemiesPerWave = new int[] { 2, 4, 5, 6 };

    [Tooltip("If enemiesPerWave is empty or wave index exceeds it, use auto growth.")]
    [SerializeField] private int startingEnemies = 2;
    [SerializeField] private int enemiesIncreasePerWave = 2;

    [Header("Timing")]
    [SerializeField] private float timeBetweenWaves = 2f;

    private readonly List<GameObject> aliveEnemies = new List<GameObject>();
    private int currentWaveIndex = 0;
    private bool spawningWave = false;

    private void Start()
    {
        SpawnWave(currentWaveIndex);
    }

    private void Update()
    {
        // Clean up destroyed enemies (destroyed objects become "null" in Unity)
        for (int i = aliveEnemies.Count - 1; i >= 0; i--)
        {
            if (aliveEnemies[i] == null)
                aliveEnemies.RemoveAt(i);
        }

        // If wave finished, spawn next wave
        if (!spawningWave && aliveEnemies.Count == 0)
        {
            spawningWave = true;
            StartCoroutine(SpawnNextWaveAfterDelay());
        }
    }

    private IEnumerator SpawnNextWaveAfterDelay()
    {
        yield return new WaitForSeconds(timeBetweenWaves);

        currentWaveIndex++;
        SpawnWave(currentWaveIndex);

        spawningWave = false;
    }

    private void SpawnWave(int waveIndex)
    {
        if (enemyPrefab == null)
        {
            Debug.LogError("[EnemyController] Enemy prefab is not assigned.");
            return;
        }

        if (spawnPoints == null || spawnPoints.Length == 0)
        {
            Debug.LogError("[EnemyController] No spawn points assigned.");
            return;
        }

        int count = GetEnemyCountForWave(waveIndex);

        for (int i = 0; i < count; i++)
        {
            Transform sp = spawnPoints[Random.Range(0, spawnPoints.Length)];

            GameObject enemy = Instantiate(enemyPrefab, sp.position, sp.rotation);
            aliveEnemies.Add(enemy);
        }

        Debug.Log($"[EnemyController] Spawned wave {waveIndex + 1} with {count} enemies.");
    }

    private int GetEnemyCountForWave(int waveIndex)
    {
        // Use configured list if available
        if (enemiesPerWave != null && enemiesPerWave.Length > 0 && waveIndex < enemiesPerWave.Length)
            return Mathf.Max(0, enemiesPerWave[waveIndex]);

        // Otherwise use auto growth
        return Mathf.Max(0, startingEnemies + waveIndex * enemiesIncreasePerWave);
    }
}
