using UnityEngine;
using TMPro;

public class WaveManager : MonoBehaviour
{
    public SpawnerScript spawner;
    public TMP_Text waveTimerText;
    public int enemiesPerWave = 5;
    public int enemiesIncreasePerWave = 2;
    public int baseEnemyHealth = 100;
    public int enemyHealthIncreasePerWave = 20;
    public float timeBetweenWaves = 30f;

    private int currentWave = 0;
    private float waveTimer;
    private bool isCountdownActive;

    void Start()
    {
        if (spawner == null)
        {
            spawner = FindFirstObjectByType<SpawnerScript>();
        }

        StartWave();
        waveTimer = timeBetweenWaves;
        isCountdownActive = false;
        UpdateWaveTimerText();
    }

    void Update()
    {
        if (GameManager.Instance != null &&
            GameManager.Instance.GetGameState() != GameManager.GameState.Playing)
        {
            isCountdownActive = false;
            UpdateWaveTimerText();
            return;
        }

        if (spawner == null)
        {
            UpdateWaveTimerText();
            return;
        }

        if (spawner.IsWaveComplete())
        {
            if (!isCountdownActive)
            {
                isCountdownActive = true;
                waveTimer = timeBetweenWaves;
            }

            waveTimer -= Time.deltaTime;
            if (waveTimer <= 0f)
            {
                StartWave();
                waveTimer = timeBetweenWaves;
                isCountdownActive = false;
            }
        }
        else
        {
            isCountdownActive = false;
            waveTimer = timeBetweenWaves;
        }

        UpdateWaveTimerText();
    }

    void UpdateWaveTimerText()
    {
        if (waveTimerText == null)
        {
            return;
        }

        if (!isCountdownActive)
        {
            waveTimerText.enabled = false;
            return;
        }

        float clampedTime = Mathf.Max(0f, waveTimer);
        bool isTimerRunning = clampedTime > 0f;
        waveTimerText.enabled = isTimerRunning;

        if (!isTimerRunning)
        {
            return;
        }

        int secondsLeft = Mathf.CeilToInt(clampedTime);
        waveTimerText.text = "Next Wave in: " + secondsLeft + "s";
    }

    void StartWave()
    {
        currentWave++;
        int enemiesThisWave = enemiesPerWave + ((currentWave - 1) * enemiesIncreasePerWave);
        int enemyHealthThisWave = baseEnemyHealth + ((currentWave - 1) * enemyHealthIncreasePerWave);

        if (spawner != null)
        {
            spawner.ApplyWaveSettings(enemiesThisWave, enemyHealthThisWave);
        }
        else
        {
            Debug.LogWarning("WaveManager saknar referens till SpawnerScript.");
        }

        Debug.Log("Starting Wave " + currentWave + " | Enemies: " + enemiesThisWave + " | Enemy HP: " + enemyHealthThisWave);
    }
}