using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Video;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }
    
    private GameState _currentState = GameState.MainMenu;
    private InputAction _escapeAction;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        
        Instance = this;

        QualitySettings.vSyncCount = 0;
        Application.targetFrameRate = 240;

        DontDestroyOnLoad(gameObject);
    }

    public enum GameState
    {
        MainMenu,
        Playing,
        Paused,
        GameOver
    }
    
    public void SetGameState(GameState newState)
    {
        _currentState = newState;
        Debug.Log("Spelläge ändrat till: " + newState);
        
        if (newState == GameState.Paused)
        {
            Time.timeScale = 0f; // Pausa spelet
        }
        else if (newState == GameState.Playing)
        {
            Time.timeScale = 1f; // Återuppta spelet
        }
    }
    public void LoadScene(string SceneName)
    {
        Time.timeScale = 1f;
        UnityEngine.SceneManagement.SceneManager.LoadScene(SceneName);
    }
    
    public GameState GetGameState()
    {
        return _currentState;
    }
    public void StartGame()
    {
        LoadScene("MainScene");
        SetGameState(GameState.Playing);
    }
    public void QuitGame()
    {
        Application.Quit();
    }
}