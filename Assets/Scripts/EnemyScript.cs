using UnityEngine;

public class EnemyScript : MonoBehaviour
{
    public int currentHealth;
    public int maxHealth = 100;

    private GameObject player;
    public float speed = 3f;
    public float knockbackForce = 5f;
    public float knockbackDuration = 0.2f; // Hur länge knockback pågår

    private float distanceToPlayer;
    private Rigidbody2D rb;
    private float knockbackTimer = 0f;

    void Start()
    {
        currentHealth = maxHealth;
        player = GameObject.FindGameObjectWithTag("Player");
        rb = GetComponent<Rigidbody2D>();
    }

    void Update()
    {
        if (player == null) return;
        

        if (knockbackTimer > 0)
        {
            knockbackTimer -= Time.deltaTime;
            return; 
        }
        
        distanceToPlayer = Vector3.Distance(transform.position, player.transform.position);
        

        Vector3 direction = (player.transform.position - transform.position).normalized;
        rb.linearVelocity = direction * speed;
    }

    public void TakeDamage(int damage)
    {
        currentHealth -= damage;

        // Applicera knockback från spelaren
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