using UnityEngine;

public class EnemyScript : MonoBehaviour
{
    public int currentHealth;
    public int maxHealth = 100;

    private GameObject player;
    public float speed = 3f;

    private float distanceToPlayer;

    void Start()
    {
        currentHealth = maxHealth;
        player = GameObject.FindGameObjectWithTag("Player");
    }

    // Update is called once per frame
    void Update()
    {
        if (player == null) return;
        
        distanceToPlayer = Vector3.Distance(transform.position, player.transform.position);
        
        transform.position = Vector3.MoveTowards(transform.position, player.transform.position, speed * Time.deltaTime);
    }

    public void TakeDamage(int damage)
    {
        currentHealth -= damage;

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