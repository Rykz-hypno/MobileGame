using UnityEngine;

public class PlayerCombat : MonoBehaviour
{
    public Transform attackPoint;
    public float weaponRange = 0.5f;
    public int attackDamage = 20;
    public LayerMask enemyLayers;

    public Animator animator;

    public void Attack()
    {
        animator.SetTrigger("Attack");
        Collider2D[] hitEnemies = Physics2D.OverlapCircleAll(attackPoint.position, weaponRange, enemyLayers);
    }

}