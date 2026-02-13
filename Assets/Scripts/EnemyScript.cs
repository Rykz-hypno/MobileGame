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
    public float speed = 200f;
    public float jumpForce = 100f;
    public float jumpCooldown = 1f;
    public float jumpNodeHeightRequirement = 0.8f;
    public bool followEnabled = true;
    public bool jumpEnabled = true;
    public bool directionLookEnabled = true;

    [Header("Platform Edge Detection")]
    public float verticalDetourThreshold = 1.5f;
    public float edgeDetectDistance = 2f;
    public float edgeDetectStepSize = 0.3f;
    public bool enableEdgeDetection = true;

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
    private Vector3 targetEdgePoint;
    private bool hasFoundEdge;
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
        hasFoundEdge = false;

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

        if (currentWaypoint >= path.vectorPath.Count)
        {
            return;
        }

        Vector2 direction = Vector2.zero;
        
        if (enableEdgeDetection && IsDirectlyAboveTarget())
        {
            if (!hasFoundEdge)
            {
                FindNearestPlatformEdge();
            }
            if (hasFoundEdge)
            {
                direction = ((Vector2)targetEdgePoint - rb.position).normalized;
            }
        }
        
        if (direction == Vector2.zero)
        {
            direction = ((Vector2)path.vectorPath[currentWaypoint] - rb.position).normalized;
            if (direction.x != 0)
            {
                lastMoveDir = Mathf.Sign(direction.x);
            }
        }
        
        Vector2 force = direction * speed;

        if (jumpEnabled && isGrounded && !isInAir && !isOnCoolDown)
        {
            if (direction.y > jumpNodeHeightRequirement)
            {
                if (isInAir) return;
                isJumping = true;
                rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpForce);
                StartCoroutine(JumpCoolDown());
            }
        }

        if (isGrounded)
        {
            isJumping = false;
            isInAir = false;
        }
        else
        {
            isInAir = true;
        }

        rb.linearVelocity = new Vector2(force.x, rb.linearVelocity.y);

        float distance = Vector2.Distance(rb.position, path.vectorPath[currentWaypoint]);
        if (distance < nextWaypointDistance)
        {
            currentWaypoint++;
        }

        if (directionLookEnabled)
        {
            if (rb.linearVelocity.x > 0.05f)
            {
                transform.localScale = new Vector3(-1f * Mathf.Abs(transform.localScale.x), transform.localScale.y, transform.localScale.z);
            }
            else if (rb.linearVelocity.x < -0.05f)
            {
                transform.localScale = new Vector3(Mathf.Abs(transform.localScale.x), transform.localScale.y, transform.localScale.z);
            }
        }
    }

    bool IsDirectlyAboveTarget()
    {
        float verticalDelta = target.position.y - transform.position.y;
        float horizontalDelta = Mathf.Abs(target.position.x - transform.position.x);
        return verticalDelta < -verticalDetourThreshold && horizontalDelta < 3f;
    }

    void FindNearestPlatformEdge()
    {
        float stepSize = edgeDetectStepSize;
        float searchDir = Mathf.Sign(target.position.x - transform.position.x);
        if (Mathf.Abs(searchDir) < 0.01f)
        {
            searchDir = lastMoveDir;
        }

        Vector3 checkPos = transform.position;
        float bestDistance = float.MaxValue;
        Vector3 bestEdgePos = transform.position;

        for (int i = 0; i < Mathf.Round(edgeDetectDistance / stepSize); i++)
        {
            checkPos.x += searchDir * stepSize;
            RaycastHit2D hit = Physics2D.Raycast(checkPos, Vector2.down, 0.5f, groundLayer);
            
            if (hit.collider == null)
            {
                float distToEdge = Vector2.Distance(rb.position, new Vector2(checkPos.x, rb.position.y));
                if (distToEdge < bestDistance)
                {
                    bestDistance = distToEdge;
                    bestEdgePos = checkPos;
                }
                break;
            }
        }

        if (bestDistance < float.MaxValue)
        {
            targetEdgePoint = bestEdgePos;
            hasFoundEdge = true;
        }
    }

    bool TargetInDistance()
    {
        return Vector2.Distance(transform.position, target.position) < activateDistance;
    }

    void OnPathComplete(Path p)
    {
        if (!p.error)
        {
            path = p;
            currentWaypoint = 0;
            hasFoundEdge = false;
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
        Destroy(gameObject);
    }
}