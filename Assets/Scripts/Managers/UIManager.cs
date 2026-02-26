using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

public class UIManager : MonoBehaviour
{
    public static UIManager Instance { get; private set; }

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    public void PauseMenuLayout()
    {
        Debug.Log("Showing pause menu");
    }

    public void StartLevel()
    {
        Debug.Log("Starting level");
    }

    // Koppla till "Save"-knapp i spelet
    public void OnClickSaveGame()
    {
        GameSaveSystem.SaveNow();
    }

    // Koppla till "Load"-knapp (fungerar både i meny och in-game)
    public void OnClickLoadGame()
    {
        StartCoroutine(LoadGameRoutine());
    }

    private IEnumerator LoadGameRoutine()
    {
        if (SceneManager.GetActiveScene().name != "MainScene")
        {
            var op = SceneManager.LoadSceneAsync("MainScene");
            while (!op.isDone) yield return null;

            // Vänta en frame så scene-objekt hinner initieras
            yield return null;
        }

        GameSaveSystem.LoadNow();
    }
}