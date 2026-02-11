using UnityEngine;

public class EnemyScript : MonoBehaviour
{
    public int currentHealth;
    public int maxHealth = 100;

    private GameObject player;
    public float speed = 3f;
    public float knockbackForce = 5f;
    public float knockbackDuration = 0.2f;
    public float jumpForce = 10f;
    public float jumpCooldown = 1f;
    public float jumpDistance = 2f;
    public float wallDetectDistance = 0.5f;
    public float raycastDistance = 1f;

    private float distanceToPlayer;
    private Rigidbody2D rb;
    private float knockbackTimer = 0f;
    private float jumpTimer = 0f;
    private bool isGrounded = false;
    private Vector2 moveDirection = Vector2.zero;

    void Start()
    {
        currentHealth = maxHealth;
        player = GameObject.FindGameObjectWithTag("Player");
        rb = GetComponent<Rigidbody2D>();
    }

    void Update()
    {
        if (player == null) return;

        jumpTimer -= Time.deltaTime;

        if (knockbackTimer > 0)
        {
            knockbackTimer -= Time.deltaTime;
            return;
        }

        distanceToPlayer = Vector3.Distance(transform.position, player.transform.position);
        moveDirection = (player.transform.position - transform.position).normalized;

        // Detektera vägg/plattform framåt
        bool wallAhead = IsWallAhead(moveDirection.x);
        
        // Detektera vägg/plattform uppåt
        bool platformAbove = IsPlatformAbove();

        Vector2 actualMoveDirection = moveDirection;

        // Om plattform finns uppåt, leta närmaste väg till fritt område
        if (platformAbove)
        {
            // Prova att flytta åt höger först
            if (!IsWallAhead(1f))
            {
                actualMoveDirection = Vector2.right;
            }
            // Annars prova åt vänster
            else if (!IsWallAhead(-1f))
            {
                actualMoveDirection = Vector2.left;
            }
        }
        // Om vägg framåt (men inte plattform ovanför), flytta åt sidan
        else if (wallAhead && moveDirection.x != 0)
        {
            bool rightFree = !IsWallAhead(1f);
            bool leftFree = !IsWallAhead(-1f);
            
            if (rightFree && moveDirection.x > 0)
            {
                actualMoveDirection = Vector2.right;
            }
            else if (leftFree && moveDirection.x < 0)
            {
                actualMoveDirection = Vector2.left;
            }
        }

        // Röra sig horisontellt endast på marken
        if (isGrounded)
        {
            rb.linearVelocity = new Vector2(actualMoveDirection.x * speed, rb.linearVelocity.y);
        }

        // Hoppa bara när vägg blockerar vägen mot spelaren
        if (isGrounded && jumpTimer <= 0 && wallAhead && !platformAbove)
        {
            Jump();
            jumpTimer = jumpCooldown;
        }
    }

    bool IsWallAhead(float direction)
    {
        // Raycast framåt för att detektera vägg
        Vector2 rayDirection = new Vector2(direction, 0).normalized;
        RaycastHit2D hit = Physics2D.Raycast(
            transform.position,
            rayDirection,
            wallDetectDistance,
            LayerMask.GetMask("Ground")
        );

        return hit.collider != null;
    }

    bool IsPlatformAbove()
    {
        // Raycast uppåt för att detektera plattform ovanför
        RaycastHit2D hit = Physics2D.Raycast(
            transform.position,
            Vector2.up,
            wallDetectDistance,
            LayerMask.GetMask("Ground")
        );

        return hit.collider != null;
    }

    void Jump()
    {
        rb.AddForce(new Vector2(0f, jumpForce), ForceMode2D.Impulse);
    }

    void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Ground"))
        {
            isGrounded = true;
        }
    }

    void OnCollisionExit2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Ground"))
        {
            isGrounded = false;
        }
    }

    public void TakeDamage(int damage)
    {
        currentHealth -= damage;

        if (player != null && rb != null)
        {
            Vector3 knockbackDirection = (transform.position - player.transform.position).normalized;
            rb.linearVelocity = knockbackDirection * knockbackForce;
            knockbackTimer = knockbackDuration;
        }

        if (currentHealth <= 0)
        {
            Die();
        }
    }

    private void Die()
    {
        Destroy(gameObject);
    }
}