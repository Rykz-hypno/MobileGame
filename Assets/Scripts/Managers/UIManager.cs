using UnityEngine;

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
        // Visa pausmeny
        Debug.Log("Showing pause menu");
    }

    public void StartLevel()
    {
        // Dölj pausmeny och börja nivå
        Debug.Log("Starting level");
    }
}