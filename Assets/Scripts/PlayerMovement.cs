using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerMovement : MonoBehaviour
{
    public PlayerCombat playerCombat;
    private float horizontalInput;
    private float speed = 7f;
    private float jumpForce = 12f;
    private bool isFacingRight = true;
    private bool isRunning = false;

    [SerializeField] private Animator animator;
    [SerializeField] private Rigidbody2D rb;
    [SerializeField] private Transform groundCheck;
    [SerializeField] private LayerMask groundLayer;
    [SerializeField] private float groundCheckRadius = 0.2f;

    private float nextGroundCheckTime = 0f;
    private const float GROUND_CHECK_COOLDOWN = 0.05f;
    private bool cachedIsGrounded = false;


    public void OnAttack(InputAction.CallbackContext context)
    {
        if (context.started)
        {
            playerCombat.Attack();
        }
    }



    private void FixedUpdate()
    {
        rb.linearVelocity = new Vector2(horizontalInput * speed, rb.linearVelocity.y);
    }


    public void OnMove(InputAction.CallbackContext context)
    {
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
        if (context.started && IsGrounded())
        {
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpForce);
        }

        if (context.canceled && rb.linearVelocity.y > 0f)
        {
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, rb.linearVelocity.y * 0.5f);
        }
    }

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
