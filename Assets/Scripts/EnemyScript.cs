using UnityEngine;

public class EnemyScript : MonoBehaviour
{
    public int currentHealth;
    public int maxHealth = 100;

    private GameObject player;
    public float speed = 3f;
    public float knockbackForce = 5f;
    public float knockbackDuration = 0.2f;
    public float jumpForce = 1.5f;
    public float jumpCooldown = 1.5f;
    public float jumpDistance = 3f;
    public float groundDrag = 0.5f;

    private float distanceToPlayer;
    private Rigidbody2D rb;
    private float knockbackTimer = 0f;
    private float jumpTimer = 0f;
    private bool isGrounded = false;

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

        Vector3 direction = (player.transform.position - transform.position).normalized;
        
        // Fortsätt röra sig mot spelaren även under hopp
        rb.linearVelocity = new Vector2(direction.x * speed, rb.linearVelocity.y);

        // Hoppa när spelaren är högre upp OCH inom hoppavståndet
        float verticalDistance = player.transform.position.y - transform.position.y;
        float horizontalDistance = Mathf.Abs(player.transform.position.x - transform.position.x);
        
        if (verticalDistance > 0.5f && horizontalDistance < jumpDistance && jumpTimer <= 0 && isGrounded)
        {
            Jump();
            jumpTimer = jumpCooldown;
        }
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