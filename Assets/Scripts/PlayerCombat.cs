using UnityEngine;
using UnityEngine.InputSystem;
public class PlayerCombat : MonoBehaviour
{
    public Transform attackPoint;
    public float weaponRange = 0.5f;
    public int attackDamage = 20;
    public LayerMask enemyLayers;

    public Animator animator;

    public int playerHealth = 10;
    public int damagePerHit = 1;

    void OnCollisionEnter2D(Collision2D other)
    {
        if (other.gameObject.CompareTag("Enemy"))
        {
            Debug.Log("Player Hit by Enemy");
            TakeDamage();
        }
    }

    public void TakeDamage()
    {
        playerHealth -= damagePerHit;
        Debug.Log("Player took " + damagePerHit + " damage. Health: " + playerHealth);
        if (playerHealth <= 0)
        {
            Debug.Log("Player Died");
        }
    }



}