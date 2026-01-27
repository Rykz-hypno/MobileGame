using UnityEngine;
using UnityEngine.InputSystem;

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
        Application.targetFrameRate = 120;

        DontDestroyOnLoad(gameObject);
    }

    private void OnEnable()
    {
        _escapeAction = new InputAction("Escape", InputActionType.Button, "<Keyboard>/escape");
        _escapeAction.Enable();
        _escapeAction.performed += OnEscapePressed;
    }

    private void OnDisable()
    {
        _escapeAction.performed -= OnEscapePressed;
        _escapeAction.Dispose();
    }

    private void OnEscapePressed(InputAction.CallbackContext context)
    {
        Debug.Log("ESC tangenten trycktes!");
        
        if (_currentState == GameState.Playing)
        {
            SetGameState(GameState.Paused);
        }
        else if (_currentState == GameState.Paused)
        {
            SetGameState(GameState.Playing);
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
            Time.timeScale = 0f; // Pausa spelet
        }
        else if (newState == GameState.Playing)
        {
            Time.timeScale = 1f; // Återuppta spelet
        }
    }
    
    public GameState GetGameState()
    {
        return _currentState;
    }
}