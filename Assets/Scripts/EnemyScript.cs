using Pathfinding;
using System.Collections;
using UnityEngine;

[RequireComponent(typeof(Seeker), typeof(Rigidbody2D))]
public class EnemyScript : MonoBehaviour
{
    [Header("Health")]
    public int currentHealth;
    public int maxHealth = 100;

    [Header("Target")]
    public Transform target;

    [Header("Pathfinding")]
    public float activateDistance = 50f;
    public float pathUpdateSeconds = 0.5f;
    public float nextWaypointDistance = 3f;

    [Header("Movement")]
    public float speed = 4f;
    public float jumpForce = 12f;
    public float jumpCooldown = 1f;
    public float jumpNodeHeightRequirement = 0.8f;
    public bool followEnabled = true;
    public bool jumpEnabled = true;
    public bool directionLookEnabled = true;
    public bool axisAlignedMovement = true;

    [Header("Combat")]
    public float knockbackForce = 5f;
    public float knockbackDuration = 0.2f;

    private Path path;
    private int currentWaypoint;
    private Seeker seeker;
    private Rigidbody2D rb;
    private float knockbackTimer;
    private bool isGrounded;
    private bool isJumping;
    private bool isInAir;
    private bool isOnCoolDown;
    private float lastMoveDir = 1f;
    private LayerMask groundLayer;

    void Start()
    {
        currentHealth = maxHealth;
        seeker = GetComponent<Seeker>();
        rb = GetComponent<Rigidbody2D>();
        groundLayer = LayerMask.GetMask("Ground");

        if (target == null)
        {
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            if (player != null)
            {
                target = player.transform;
            }
        }

        isJumping = false;
        isInAir = false;
        isOnCoolDown = false;

        InvokeRepeating("UpdatePath", 0f, pathUpdateSeconds);
    }

    void Update()
    {
        if (knockbackTimer > 0f)
        {
            knockbackTimer -= Time.deltaTime;
        }
    }

    void FixedUpdate()
    {
        if (target == null) return;
        if (knockbackTimer > 0f) return;

        if (TargetInDistance() && followEnabled)
        {
            PathFollow();
        }
    }

    void UpdatePath()
    {
        if (target == null) return;

        if (followEnabled && TargetInDistance() && seeker.IsDone())
        {
            seeker.StartPath(rb.position, target.position, OnPathComplete);
        }
    }

    void PathFollow()
    {
        if (path == null)
        {
            return;
        }

        // Check if we've reached the end of the path
        if (currentWaypoint >= path.vectorPath.Count)
        {
            // We're at the end - stop moving or wait for new path
            return;
        }

        // Get current and next waypoint
        Vector2 currentPos = rb.position;
        Vector2 targetPos = path.vectorPath[currentWaypoint];
        
        // Calculate direction to current waypoint
        Vector2 direction = (targetPos - currentPos).normalized;
        
        if (direction.x != 0)
        {
            lastMoveDir = Mathf.Sign(direction.x);
        }

        // Snap to axis-aligned movement
        if (axisAlignedMovement)
        {
            direction = SnapToAxis(direction);
        }
        
        Vector2 force = direction * speed;

        // Check if next waypoint requires a jump
        if (jumpEnabled && isGrounded && !isInAir && !isOnCoolDown)
        {
            // Look ahead to see if we need to jump
            if (currentWaypoint + 1 < path.vectorPath.Count)
            {
                Vector2 nextWaypoint = path.vectorPath[currentWaypoint + 1];
                float heightDifference = nextWaypoint.y - currentPos.y;
                
                // Jump if next waypoint is significantly higher
                if (heightDifference > jumpNodeHeightRequirement)
                {
                    isJumping = true;
                    rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpForce);
                    StartCoroutine(JumpCoolDown());
                }
            }
        }

        // Track air state
        if (isGrounded)
        {
            isJumping = false;
            isInAir = false;
        }
        else
        {
            isInAir = true;
        }

        // Apply horizontal movement
        rb.linearVelocity = new Vector2(force.x, rb.linearVelocity.y);

        // Move to next waypoint when close enough
        float distance = Vector2.Distance(currentPos, targetPos);
        if (distance < nextWaypointDistance)
        {
            currentWaypoint++;
        }

        // Flip sprite based on movement direction
        if (directionLookEnabled)
        {
            if (rb.linearVelocity.x > 0.05f)
            {
                transform.localScale = new Vector3(Mathf.Abs(transform.localScale.x), transform.localScale.y, transform.localScale.z);
            }
            else if (rb.linearVelocity.x < -0.05f)
            {
                transform.localScale = new Vector3(-1f * Mathf.Abs(transform.localScale.x), transform.localScale.y, transform.localScale.z);
            }
        }
    }


    bool TargetInDistance()
    {
        return Vector2.Distance(transform.position, target.position) < activateDistance;
    }

    Vector2 SnapToAxis(Vector2 input)
    {
        if (Mathf.Abs(input.x) >= Mathf.Abs(input.y))
        {
            return new Vector2(Mathf.Sign(input.x), 0f);
        }

        return new Vector2(0f, Mathf.Sign(input.y));
    }

    void OnPathComplete(Path p)
    {
        if (!p.error)
        {
            path = p;
            // Only reset waypoint if we're starting a completely new path
            // or if we're far from the current path
            if (currentWaypoint >= path.vectorPath.Count || path.vectorPath.Count < 2)
            {
                currentWaypoint = 0;
            }
            else
            {
                // Find closest waypoint to continue from
                float closestDist = float.MaxValue;
                int closestIndex = 0;
                for (int i = 0; i < path.vectorPath.Count; i++)
                {
                    float dist = Vector2.Distance(rb.position, path.vectorPath[i]);
                    if (dist < closestDist)
                    {
                        closestDist = dist;
                        closestIndex = i;
                    }
                }
                currentWaypoint = Mathf.Min(closestIndex, currentWaypoint);
            }
        }
    }

    IEnumerator JumpCoolDown()
    {
        isOnCoolDown = true;
        yield return new WaitForSeconds(jumpCooldown);
        isOnCoolDown = false;
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

        if (target != null && rb != null)
        {
            Vector3 knockbackDirection = (transform.position - target.position).normalized;
            rb.linearVelocity = knockbackDirection * knockbackForce;
            knockbackTimer = knockbackDuration;
        }

        if (currentHealth <= 0)
        {
            Die();
        }
    }

    void Die()
    {
        // TODO: Lägg till dödseffekt, poäng, etc.
        Destroy(gameObject);
    }
}