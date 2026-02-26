using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class GameSaveData
{
    public float playerX;
    public float playerY;
    public float playerZ;
    public int playerHealth;

    public int currentWave;
    public int enemiesRemaining;

    public int maxSpawnCount;
    public int spawnCount;
    public float spawnTimer;
    public int currentWaveEnemyHealth;

    public List<EnemySaveData> enemies = new();
}

public static class GameSaveSystem
{
    private const string SaveKey = "GAME_SAVE_V1";

    public static bool HasSave() => PlayerPrefs.HasKey(SaveKey);

    public static void SaveNow()
    {
        var player = Object.FindFirstObjectByType<PlayerCombat>();
        var wave = Object.FindFirstObjectByType<WaveManager>();
        var spawner = wave != null ? wave.spawner : Object.FindFirstObjectByType<SpawnerScript>();

        if (player == null || wave == null || spawner == null)
        {
            Debug.LogWarning("[Save] Saknar PlayerCombat/WaveManager/SpawnerScript.");
            return;
        }

        spawner.RefreshEnemyCounts();

        var data = new GameSaveData
        {
            playerX = player.transform.position.x,
            playerY = player.transform.position.y,
            playerZ = player.transform.position.z,
            playerHealth = player.playerHealth,

            currentWave = wave.CurrentWave,
            enemiesRemaining = spawner.CurrentEnemyCount,

            maxSpawnCount = spawner.MaxSpawnCount,
            spawnCount = spawner.SpawnedCount,
            spawnTimer = spawner.CurrentSpawnTimer,
            currentWaveEnemyHealth = spawner.CurrentWaveEnemyHealth
        };

        foreach (var enemy in spawner.GetAliveEnemies())
        {
            if (enemy == null) continue;

            data.enemies.Add(new EnemySaveData
            {
                x = enemy.transform.position.x,
                y = enemy.transform.position.y,
                z = enemy.transform.position.z,
                maxHealth = enemy.MaxHealth,
                currentHealth = enemy.CurrentHealth
            });
        }

        string json = JsonUtility.ToJson(data);
        PlayerPrefs.SetString(SaveKey, json);
        PlayerPrefs.Save();

        Debug.Log("[Save] Sparat.");
    }

    public static bool LoadNow()
    {
        if (!PlayerPrefs.HasKey(SaveKey))
        {
            Debug.Log("[Save] Ingen save hittad.");
            return false;
        }

        string json = PlayerPrefs.GetString(SaveKey);
        var data = JsonUtility.FromJson<GameSaveData>(json);
        if (data == null) return false;

        var player = Object.FindFirstObjectByType<PlayerCombat>();
        var wave = Object.FindFirstObjectByType<WaveManager>();
        var spawner = wave != null ? wave.spawner : Object.FindFirstObjectByType<SpawnerScript>();

        if (player == null || wave == null || spawner == null)
        {
            Debug.LogWarning("[Load] Saknar PlayerCombat/WaveManager/SpawnerScript.");
            return false;
        }

        player.transform.position = new Vector3(data.playerX, data.playerY, data.playerZ);
        player.playerHealth = Mathf.Max(1, data.playerHealth);

        wave.SetCurrentWaveFromSave(data.currentWave);

        spawner.RestoreFromSave(
            data.maxSpawnCount,
            data.spawnCount,
            data.spawnTimer,
            data.currentWaveEnemyHealth,
            data.enemies
        );

        Debug.Log("[Load] Laddat.");
        return true;
    }

    public static void ClearSave()
    {
        PlayerPrefs.DeleteKey(SaveKey);
        PlayerPrefs.Save();
    }
}