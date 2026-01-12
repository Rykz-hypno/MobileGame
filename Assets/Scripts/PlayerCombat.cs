using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerCombat : MonoBehaviour
{
    [SerializeField] private Animator animator;

    public void Attack()
    {
        animator.SetBool("isAttacking", true);
    }
    public void StopAttack()
    {
        animator.SetBool("isAttacking", false);
    }
}