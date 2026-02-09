using UnityEngine;

public class EnemyScript : MonoBehaviour
{
    public int currentHealth;
    public int maxHealth = 100;

    private GameObject player;
    public float speed = 3f;
    public float knockbackForce = 5f;
    public float knockbackDuration = 0.2f;
    public float jumpForce = 0.3f;
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

        // Röra sig horisontellt endast på marken
        if (isGrounded)
        {
            rb.linearVelocity = new Vector2(moveDirection.x * speed, rb.linearVelocity.y);
        }

        // Detektera vägg framåt
        bool wallAhead = IsWallAhead(moveDirection.x);

        // Hoppa ENDAST om det finns en vägg framåt eller spelaren är högre upp
        float verticalDistance = player.transform.position.y - transform.position.y;
        float horizontalDistance = Mathf.Abs(player.transform.position.x - transform.position.x);

        if (isGrounded && jumpTimer <= 0 && horizontalDistance < jumpDistance)
        {
            // Hoppa över vägg
            if (wallAhead)
            {
                Jump();
                jumpTimer = jumpCooldown;
            }
            // Hoppa när spelaren är högre upp (men inte för mycket)
            else if (verticalDistance > 0.5f && verticalDistance < 2f)
            {
                Jump();
                jumpTimer = jumpCooldown;
            }
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