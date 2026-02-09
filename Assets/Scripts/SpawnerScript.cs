using UnityEngine;

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

    void Start()
    {
        SpawnTimer = 0f;
        spawnCount = 0;
        currentEnemyCount = 0;
    }

    void Update()
    {
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
        Instantiate(objectToSpawn, spawnPoint.position, spawnPoint.rotation);
        spawnCount++;
        currentEnemyCount++;
    }
}
