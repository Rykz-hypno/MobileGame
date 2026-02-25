using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using System.Linq;
using System.Collections.Generic; // <-- ny

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }
    
    private GameState _currentState = GameState.MainMenu;
    private InputAction _escapeAction;

    private GameObject gameplayCanvas;
    private GameObject deathCanvas;

    private readonly Dictionary<string, GameObject> _canvasMap = new();
    private readonly Dictionary<string, System.Action> _buttonActions = new();

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

        // actions
        _buttonActions["DeathCanvas/RetryButton"] = () => LoadScene("MainScene");
        _buttonActions["DeathCanvas/MenuButton"] = ReturnToMainMenu;
        _buttonActions["DeathCanvas/ExitButton"] = QuitGame;

        _buttonActions["PauseCanvas/ResumeButton"] = () => SetGameState(GameState.Playing);
        _buttonActions["PauseCanvas/MenuButton"] = ReturnToMainMenu;

        _buttonActions["GameCanvas/PauseButton"] = () => SetGameState(GameState.Paused);

        _buttonActions["MenuCanvas/PlayButton"] = StartGame;
        _buttonActions["MenuCanvas/SoundButton"] = () => Debug.Log("Sound settings - not implemented");
        _buttonActions["MenuCanvas/ExitButton"] = QuitGame;

        SceneManager.sceneLoaded += OnSceneLoaded;

        OnSceneLoaded(SceneManager.GetActiveScene(), LoadSceneMode.Single);
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        CacheCanvases(scene);

        gameplayCanvas = GetCanvas("GameCanvas"); 
        deathCanvas = GetCanvas("DeathCanvas");

        foreach (var canvasName in _canvasMap.Keys)
            SetupButtons(canvasName);

        if (scene.name == "MainScene")
        {
            SetGameState(GameState.Playing);
            SetOnlyCanvasActive("GameCanvas");
        }
    }

    private void CacheCanvases(Scene scene)
    {
        _canvasMap.Clear();
        var canvases = Resources.FindObjectsOfTypeAll<Canvas>()
            .Where(c => c.gameObject.scene == scene);

        foreach (var c in canvases)
            _canvasMap[c.name] = c.gameObject;
    }

    private GameObject GetCanvas(string canvasName)
        => _canvasMap.TryGetValue(canvasName, out var go) ? go : null;

    private void SetupButtons(string canvasName)
    {
        var canvas = GetCanvas(canvasName);
        if (canvas == null) return;

        var buttons = canvas.GetComponentsInChildren<Button>(true);
        foreach (var button in buttons)
        {

            button.onClick.RemoveAllListeners();

            var key = $"{canvasName}/{button.name}";
            if (_buttonActions.TryGetValue(key, out var action))
            {
                button.onClick.AddListener(() => action());
                Debug.Log($"[Bind] {key}");
            }
            else
            {
                Debug.LogWarning($"[Saknar action] {key}");
            }
        }
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
            Time.timeScale = 0f;
        }
        else if (newState == GameState.Playing)
        {
            Time.timeScale = 1f;
        }
    }

    public void LoadScene(string SceneName)
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneName);
    }
    
    public GameState GetGameState()
    {
        return _currentState;
    }

    public void StartGame()
    {
        LoadScene("MainScene");
    }

    public void QuitGame()
    {
        Application.Quit();
    }

    public void ReturnToMainMenu()
    {
        LoadScene("Menu");
        SetGameState(GameState.MainMenu);
    }

    public void ShowDeathScreen()
    {
        EnemyScript.KillAllEnemies();

        SetOnlyCanvasActive("DeathCanvas");
        SetupButtons("DeathCanvas");

        SetGameState(GameState.GameOver);
    }

    private void OnDestroy()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void SetOnlyCanvasActive(string canvasToShow)
    {
        foreach (var kvp in _canvasMap)
        {
            if (kvp.Value != null)
                kvp.Value.SetActive(kvp.Key == canvasToShow);
        }
    }
}
