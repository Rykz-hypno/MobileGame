using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerCombat : MonoBehaviour
{
    public Transform attackPoint;
    public float weaponRange = 0.5f;
    public int attackDamage = 20;
    public LayerMask enemyLayers;

    public Animator PlayerAnim;

    public int playerHealth = 12;
    public int damagePerHit = 1;

    public void Attack()
    {
        PlayerAnim.SetTrigger("Attack");
    }

    public void OnAttack(InputAction.CallbackContext context)
    {
        if (context.started)
        {
            Attack();
        }
    }

    void OnCollisionEnter2D(Collision2D other)
    {
        if (other.gameObject.CompareTag("Enemy"))
        {
            Debug.Log("Player Hit by Enemy");
            TakeDamage();
            PlayerAnim.SetTrigger("Hurt");
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

    public void DealDamage() // Kalla denna från Animation Event
    {
        Collider2D[] hitEnemies = Physics2D.OverlapCircleAll(attackPoint.position, weaponRange, enemyLayers);

        foreach (Collider2D enemy in hitEnemies)
        {
            Debug.Log("Hit enemy: " + enemy.name);
            EnemyScript enemyScript = enemy.GetComponent<EnemyScript>();
            if (enemyScript != null)
            {
                enemyScript.TakeDamage(attackDamage);
            }
        }
    }

    void OnDrawGizmosSelected()
    {
        if (attackPoint == null) return;

        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(attackPoint.position, weaponRange);
    }
}