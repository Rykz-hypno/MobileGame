using UnityEngine;

public class PlayerCombat : MonoBehaviour
{
    [SerializeField] private Animator animator;

    public void Attack()
    {
        animator.SetTrigger("Attack");
    }

}