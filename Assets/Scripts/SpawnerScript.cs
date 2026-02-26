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
        // Stoppa all spawn när spelet inte är i "Playing" state
        if (GameManager.Instance != null &&
            GameManager.Instance.GetGameState() != GameManager.GameState.Playing)
        {
            return;
        }

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

    public int CurrentEnemyCount => currentEnemyCount;
    public int SpwanCount => spawnCount; // valfri bakåtkompatibel stavning om du vill
    public int SpwanMaxCount => maxSpawnCount;

    public int SpawnedCount => spawnCount;
    public int MaxSpawnCount => maxSpawnCount;
    public float CurrentSpawnTimer => SpawnTimer;
    public int CurrentWaveEnemyHealth => currentWaveEnemyHealth;

    public void RefreshEnemyCounts()
    {
        CleanupDeadEnemies();
    }

    public List<EnemyScript> GetAliveEnemies()
    {
        CleanupDeadEnemies();
        List<EnemyScript> result = new();
        foreach (var go in aliveEnemies)
        {
            if (go == null) continue;
            var enemy = go.GetComponent<EnemyScript>();
            if (enemy != null) result.Add(enemy);
        }
        return result;
    }

    public void RestoreFromSave(
        int savedMaxSpawnCount,
        int savedSpawnCount,
        float savedSpawnTimer,
        int savedWaveEnemyHealth,
        List<EnemySaveData> savedEnemies)
    {
        foreach (var go in aliveEnemies)
        {
            if (go != null) Destroy(go);
        }
        aliveEnemies.Clear();

        maxSpawnCount = Mathf.Max(0, savedMaxSpawnCount);
        spawnCount = Mathf.Clamp(savedSpawnCount, 0, maxSpawnCount);
        SpawnTimer = Mathf.Max(0f, savedSpawnTimer);
        currentWaveEnemyHealth = Mathf.Max(1, savedWaveEnemyHealth);

        if (savedEnemies != null)
        {
            foreach (var e in savedEnemies)
            {
                Vector3 pos = new Vector3(e.x, e.y, e.z);
                GameObject spawnedEnemy = Instantiate(objectToSpawn, pos, Quaternion.identity);

                EnemyScript enemy = spawnedEnemy.GetComponent<EnemyScript>();
                if (enemy != null)
                {
                    enemy.SetHealthState(e.maxHealth, e.currentHealth);
                }

                aliveEnemies.Add(spawnedEnemy);
            }
        }

        CleanupDeadEnemies();
    }
}
