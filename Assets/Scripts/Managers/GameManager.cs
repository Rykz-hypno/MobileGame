using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        
        Instance = this;

        QualitySettings.vSyncCount = 0;
        Application.targetFrameRate = 120;

        DontDestroyOnLoad(gameObject);
    }

}