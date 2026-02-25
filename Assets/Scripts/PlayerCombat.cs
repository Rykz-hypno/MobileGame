using UnityEngine;
using UnityEngine.InputSystem;
using Unity.Cinemachine;

public class PlayerCombat : MonoBehaviour
{
    public Transform attackPoint;
    public float weaponRange = 0.5f;
    public int attackDamage = 20;
    public LayerMask enemyLayers;

    public Animator PlayerAnim;

    public int playerHealth = 12;
    public int damagePerHit = 1;
    public float takeDamageCooldown = 0.65f;

    public PlayerMovement Movement;
    public GameManager GameManager;

    private float nextDamageTime = 0f;

    [Header("Cinemachine Death Camera")]
    [SerializeField] private CinemachineCamera normalCam;
    [SerializeField] private CinemachineCamera deathCam;
    [SerializeField] private float deathXOffset = 2f;
    [SerializeField] private float deathOrthoSize = 2.5f; // mindre = mer zoom (2D ortho)
    [SerializeField] private int normalPriority = 10;
    [SerializeField] private int deathPriority = 20;

    private void Start()
    {
        if (Movement == null)
        {
            Movement = FindFirstObjectByType<PlayerMovement>();
        }

        if (GameManager == null)
        {
            GameManager = GameManager.Instance;
        }
    }
    private bool CanAct()
    {
        return playerHealth > 0 && Movement != null && Movement.CanMove;
    }

    public void Attack()
    {
        if (!CanAct()) return;
        PlayerAnim.SetTrigger("Attack");
    }

    public void OnAttack(InputAction.CallbackContext context)
    {
        if (!CanAct()) return;

        if (context.started)
        {
            Attack();
        }
    }

    void OnCollisionEnter2D(Collision2D other)
    {
        if (other.gameObject.CompareTag("Enemy"))
        {
            Debug.Log("Player Hit by Enemy");

            if (TakeDamage())
            {
                PlayerAnim.SetTrigger("Hurt");
            }
        }
    }

    public bool TakeDamage()
    {
        if (playerHealth <= 0) return false;
        if (Time.time < nextDamageTime) return false; // cooldown aktiv

        nextDamageTime = Time.time + takeDamageCooldown;

        playerHealth -= damagePerHit;
        Debug.Log("Player took " + damagePerHit + " damage. Health: " + playerHealth);

        if (playerHealth <= 0)
        {
            Movement.DisableMovement();
            PlayerAnim.ResetTrigger("Attack");

            ActivateDeathCamera();

            if (GameManager != null)
                GameManager.ShowDeathScreen();
            else
                GameManager.Instance?.ShowDeathScreen();

            Debug.Log("Player Died");
        }

        return true;
    }

    private void ActivateDeathCamera()
    {
        if (deathCam == null) return;

        var follow = deathCam.GetComponent<CinemachineFollow>();
        if (follow != null)
        {
            var offset = follow.FollowOffset;
            offset.x = deathXOffset;
            offset.y = 1.6f; 
            follow.FollowOffset = offset;
        }

        var lens = deathCam.Lens;
        lens.OrthographicSize = deathOrthoSize;
        deathCam.Lens = lens;

        if (normalCam != null) normalCam.Priority = normalPriority;
        deathCam.Priority = deathPriority;
    }

    public void DealDamage() 
    {
        Collider2D[] hitEnemies = Physics2D.OverlapCircleAll(attackPoint.position, weaponRange, enemyLayers);

        foreach (Collider2D enemy in hitEnemies)
        {
            Debug.Log("Hit enemy: " + enemy.name);
            EnemyScript enemyScript = enemy.GetComponent<EnemyScript>();
            if (enemyScript != null)
            {
                enemyScript.TakeDamage(attackDamage);
            }
        }
    }

    void OnDrawGizmosSelected()
    {
        if (attackPoint == null) return;

        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(attackPoint.position, weaponRange);
    }
}