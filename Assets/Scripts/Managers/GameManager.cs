using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using System.Linq;
using System.Collections;
using System.Collections.Generic;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }
    
    private GameState _currentState = GameState.MainMenu;
    private InputAction _escapeAction;

    private GameObject gameplayCanvas;
    private GameObject deathCanvas;
    private GameObject menuCreditsPanel;
    private GameObject menuTitle;

    [SerializeField] private string creditsPanelName = "CreditPanel";
    [SerializeField] private string titleName = "Title";

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

        _buttonActions["PauseCanvas/ResumeButton"] = ResumeGameFromPause;
        _buttonActions["PauseCanvas/MenuButton"] = SaveAndReturnToMainMenu;
        _buttonActions["PauseCanvas/SaveButton"] = SaveGame;
        _buttonActions["PauseCanvas/LoadButton"] = LoadGame;

        _buttonActions["GameCanvas/PauseButton"] = PauseGameFromGameplay;
        _buttonActions["GameCanvas/SaveButton"] = SaveGame; // om du har save i HUD

        _buttonActions["MenuCanvas/PlayButton"] = StartGame;
        _buttonActions["MenuCanvas/LoadButton"] = LoadGame; // om du har load i meny
        _buttonActions["MenuCanvas/CreditsButton"] = ToggleCreditsPanel;
        _buttonActions["MenuCanvas/ExitButton"] = QuitGame;

        SceneManager.sceneLoaded += OnSceneLoaded;

        OnSceneLoaded(SceneManager.GetActiveScene(), LoadSceneMode.Single);
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        CacheCanvases(scene);

        gameplayCanvas = GetCanvas("GameCanvas"); 
        deathCanvas = GetCanvas("DeathCanvas");
        ResolveMenuObjects();

        foreach (var canvasName in _canvasMap.Keys)
            SetupButtons(canvasName);

        if (scene.name == "MainScene")
        {
            SetGameState(GameState.Playing);
            SetOnlyCanvasActive("GameCanvas");
        }
        else if (scene.name == "Menu")
        {
            SetCreditsVisible(false);
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

    private void ResumeGameFromPause()
    {
        SetGameState(GameState.Playing);
        SetOnlyCanvasActive("GameCanvas");
    }

    private void PauseGameFromGameplay()
    {
        SetGameState(GameState.Paused);
        SetOnlyCanvasActive("PauseCanvas");
    }

    private void SaveAndReturnToMainMenu()
    {
        SaveGame();
        ReturnToMainMenu();
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

    private void ToggleCreditsPanel()
    {
        if (menuCreditsPanel == null || menuTitle == null)
        {
            ResolveMenuObjects();
        }

        if (menuCreditsPanel == null || menuTitle == null)
        {
            Debug.LogWarning("CreditsPanel eller Title hittades inte i MenuCanvas.");
            return;
        }

        bool shouldShowCredits = !menuCreditsPanel.activeSelf;
        SetCreditsVisible(shouldShowCredits);
    }

    private void SetCreditsVisible(bool showCredits)
    {
        if (menuCreditsPanel != null)
            menuCreditsPanel.SetActive(showCredits);

        if (menuTitle != null)
            menuTitle.SetActive(!showCredits);
    }

    private void ResolveMenuObjects()
    {
        var menuCanvas = GetCanvas("MenuCanvas");
        if (menuCanvas == null)
        {
            menuCreditsPanel = null;
            menuTitle = null;
            return;
        }

        var creditsTransform = FindChildTransformByName(
            menuCanvas.transform,
            creditsPanelName,
            "CreditsPanel",
            "CreditPanel");

        var titleTransform = FindChildTransformByName(
            menuCanvas.transform,
            titleName,
            "Title");

        menuCreditsPanel = creditsTransform != null ? creditsTransform.gameObject : null;
        menuTitle = titleTransform != null ? titleTransform.gameObject : null;
    }

    private Transform FindChildTransformByName(Transform root, params string[] candidateNames)
    {
        if (root == null || candidateNames == null || candidateNames.Length == 0)
            return null;

        var allChildren = root.GetComponentsInChildren<Transform>(true);

        foreach (var candidate in candidateNames)
        {
            var exactMatch = allChildren.FirstOrDefault(t =>
                t != null &&
                !string.IsNullOrWhiteSpace(t.name) &&
                string.Equals(t.name, candidate, System.StringComparison.OrdinalIgnoreCase));

            if (exactMatch != null)
                return exactMatch;
        }

        foreach (var candidate in candidateNames)
        {
            var prefixMatch = allChildren.FirstOrDefault(t =>
                t != null &&
                !string.IsNullOrWhiteSpace(t.name) &&
                t.name.StartsWith(candidate, System.StringComparison.OrdinalIgnoreCase));

            if (prefixMatch != null)
                return prefixMatch;
        }

        return null;
    }

    private void SaveGame()
    {
        GameSaveSystem.SaveNow();
    }

    private void LoadGame()
    {
        StartCoroutine(LoadGameRoutine());
    }

    private IEnumerator LoadGameRoutine()
    {
        if (!GameSaveSystem.HasSave())
            yield break;

        if (SceneManager.GetActiveScene().name != "MainScene")
        {
            Time.timeScale = 1f;
            var op = SceneManager.LoadSceneAsync("MainScene");
            while (!op.isDone) yield return null;
            yield return null; // vänta 1 frame så objekt hinner initieras
        }

        GameSaveSystem.LoadNow();
        SetGameState(GameState.Playing);
    }
}
