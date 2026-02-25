using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerMovement : MonoBehaviour
{
    public PlayerCombat playerCombat;
    public float jumpForce = 12f;
    private float horizontalInput;
    private float speed = 7f;
    private bool isFacingRight = true;
    private bool isRunning = false;
    private bool canMove = true;

    [SerializeField] private Animator animator;
    [SerializeField] private Rigidbody2D rb;
    [SerializeField] private Transform groundCheck;
    [SerializeField] private LayerMask groundLayer;
    [SerializeField] private float groundCheckRadius = 0.2f;

    private float nextGroundCheckTime = 0f;
    private const float GROUND_CHECK_COOLDOWN = 0.05f;
    private bool cachedIsGrounded = false;

    public void DealDamage()
    {
        Collider2D [] hitEnemies = Physics2D.OverlapCircleAll(playerCombat.attackPoint.position, playerCombat.weaponRange, playerCombat.enemyLayers);
        if (hitEnemies.Length > 0)
        {
            foreach (Collider2D enemy in hitEnemies)
            {
                enemy.GetComponent<EnemyScript>().TakeDamage(playerCombat.attackDamage);
            }
        }

    }
    private void FixedUpdate()
    {
        if (!canMove) return;
        rb.linearVelocity = new Vector2(horizontalInput * speed, rb.linearVelocity.y);
    }

    public void OnMove(InputAction.CallbackContext context)
    {
        if (!canMove) return;

        horizontalInput = context.ReadValue<Vector2>().x;
        Flip();

        bool shouldRun = horizontalInput != 0f;

        if (shouldRun != isRunning)
        {
            isRunning = shouldRun;
            animator.SetBool("isRunning", isRunning);
        }
    }

    public void OnJump(InputAction.CallbackContext context)
    {
        if (!canMove) return;

        if (context.started && IsGrounded())
        {
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpForce);
        }

        if (context.canceled && rb.linearVelocity.y > 0f)
        {
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, rb.linearVelocity.y * 0.5f);
        }
    }

    public void DisableMovement()
    {
        canMove = false;
        horizontalInput = 0f;
        rb.linearVelocity = Vector2.zero;
        animator.SetBool("isRunning", false);
    }

    public void EnableMovement()
    {
        canMove = true;
    }

    public bool CanMove => canMove;

    private bool IsGrounded()
    {

        if (Time.time >= nextGroundCheckTime)
        {
            cachedIsGrounded = Physics2D.OverlapCircle(groundCheck.position, groundCheckRadius, groundLayer);
            nextGroundCheckTime = Time.time + GROUND_CHECK_COOLDOWN;
        }
        return cachedIsGrounded;
    }

    private void Flip()
    {
        if (isFacingRight && horizontalInput < 0f || !isFacingRight && horizontalInput > 0f)
        {
            isFacingRight = !isFacingRight;

            transform.localScale = new Vector3(-transform.localScale.x, transform.localScale.y, transform.localScale.z);
        }
    }
}
