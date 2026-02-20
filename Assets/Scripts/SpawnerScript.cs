using UnityEngine;
using System.Collections.Generic;

public class SpawnerScript : MonoBehaviour
{
    [SerializeField] private GameObject objectToSpawn;
    [SerializeField] private Transform LeftSpawnPoint;
    [SerializeField] private Transform RightSpawnPoint;
    [SerializeField] private float spawnInterval = 2f;
    [SerializeField] private int maxSpawnCount = 10;

    private float SpawnTimer;
    private int spawnCount;
    private int currentEnemyCount;
    private int currentWaveEnemyHealth = 100; // NY
    private List<GameObject> aliveEnemies = new List<GameObject>();

    void Start()
    {
        SpawnTimer = 0f;
        spawnCount = 0;
        currentEnemyCount = 0;
    }

    void Update()
    {
        CleanupDeadEnemies();
        SpawnTimer += Time.deltaTime;

        if (SpawnTimer >= spawnInterval && spawnCount < maxSpawnCount)
        {
            SpawnEnemy();
            SpawnTimer = 0f;
        }
    }

    private void SpawnEnemy()
    {
        Transform spawnPoint = (spawnCount % 2 == 0) ? LeftSpawnPoint : RightSpawnPoint;
        GameObject spawnedEnemy = Instantiate(objectToSpawn, spawnPoint.position, spawnPoint.rotation);

        // Sätt HP på den spawnade instansen
        EnemyScript enemy = spawnedEnemy.GetComponent<EnemyScript>();
        if (enemy != null)
        {
            enemy.SetHealth(currentWaveEnemyHealth);
        }

        aliveEnemies.Add(spawnedEnemy);
        spawnCount++;
        currentEnemyCount++;
    }

    public void ApplyWaveSettings(int newMaxSpawnCount, int newEnemyHealth)
    {
        maxSpawnCount = Mathf.Max(0, newMaxSpawnCount);
        currentWaveEnemyHealth = Mathf.Max(1, newEnemyHealth); // NY

        spawnCount = 0;
        currentEnemyCount = 0;
        aliveEnemies.Clear();
        SpawnTimer = 0f;
    }

    public bool IsWaveComplete()
    {
        CleanupDeadEnemies();
        return spawnCount >= maxSpawnCount && currentEnemyCount <= 0;
    }

    private void CleanupDeadEnemies()
    {
        aliveEnemies.RemoveAll(enemy => enemy == null);
        currentEnemyCount = aliveEnemies.Count;
    }
}
