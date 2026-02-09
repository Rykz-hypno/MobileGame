using UnityEngine;
using UnityEngine.UI;

public class HealthBar : MonoBehaviour
{
    public Image healthBarImage;
    public PlayerCombat playerCombat;
    public Sprite[] healthSprites; // Dra in sprites här i inspector
    public int maxHealth = 12;

    void Start()
    {
        if (playerCombat == null)
            playerCombat = FindFirstObjectByType<PlayerCombat>();
        
        maxHealth = playerCombat.playerHealth;
        UpdateHealthBar();
    }

    void Update()
    {
        UpdateHealthBar();
    }

    public void UpdateHealthBar()
    {
        // Minska HP och visa rätt sprite
        int healthIndex = playerCombat.playerHealth - 1; // 0-index
        
        if (healthIndex >= 0 && healthIndex < healthSprites.Length)
        {
            healthBarImage.sprite = healthSprites[healthIndex];
        }
    }
}