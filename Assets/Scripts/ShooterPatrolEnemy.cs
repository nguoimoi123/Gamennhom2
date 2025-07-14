using UnityEngine;

[RequireComponent(typeof(Rigidbody2D), typeof(Animator), typeof(CircleCollider2D))]
public class ShooterPatrolEnemy : EnemyBase
{
    [Header("Patrol Settings")]
    [SerializeField] private float patrolDistance = 2f;
    [SerializeField] private float waitTimeAtPoint = 1f;
    [SerializeField] private bool moveHorizontally = true;

    [Header("Shooting Settings")]
    [SerializeField] private float shootRange = 3f;
    [SerializeField] private float fireRate = 1f;
    [SerializeField] private GameObject projectilePrefab;
    [SerializeField] private Transform firePoint;

    private Vector2 pointA;
    private Vector2 pointB;
    private Vector2 targetPoint;
    private Vector2 startPos;
    private bool isWaiting;
    private float waitTimer;
    private bool isChasing;
    private bool isReturning;
    private Vector2 chaseDirection;
    private float nextFireTime;
    private bool isShooting;

    protected override void Awake()
    {
        base.Awake();
        startPos = transform.position;
        startPos = SanitizeVector(startPos, "startPos");
        transform.position = new Vector3(startPos.x, startPos.y, transform.position.z);

        if (moveHorizontally)
        {
            pointA = new Vector2(startPos.x - patrolDistance, startPos.y);
            pointB = new Vector2(startPos.x + patrolDistance, startPos.y);
        }
        else
        {
            pointA = new Vector2(startPos.x, startPos.y + patrolDistance);
            pointB = new Vector2(startPos.x, startPos.y - patrolDistance);
        }

        pointA = SanitizeVector(pointA, "pointA");
        pointB = SanitizeVector(pointB, "pointB");
        targetPoint = pointA;
        waitTimer = 0f;
        isWaiting = false;
        isChasing = false;
        isReturning = false;
        isShooting = false;
        nextFireTime = 0f;
        UpdatePatrolDirection();
    }

    protected override void Update()
    {
        if (isDead) return;

        CheckShooting();
        base.Update();
        UpdateAnimator();
    }

    private void CheckShooting()
    {
        if (player == null || float.IsNaN(player.position.x) || float.IsNaN(player.position.y))
        {
            isShooting = false;
            return;
        }

        Vector2 currentPos = transform.position;
        Vector2 playerPos = SanitizeVector(player.position, "playerPos");
        float distance = Vector2.Distance(currentPos, playerPos);

        if (distance <= shootRange)
        {
            isShooting = true;
            if (Time.time >= nextFireTime)
            {
                Shoot();
                nextFireTime = Time.time + fireRate;
            }
        }
        else
        {
            isShooting = false;
        }
    }

    private void Shoot()
    {
        GameObject projectile = Instantiate(projectilePrefab, firePoint.position, firePoint.rotation);
        Rigidbody2D rb = projectile.GetComponent<Rigidbody2D>();
        if (rb != null)
        {
            Vector2 direction = ((Vector2)player.position - (Vector2)firePoint.position).normalized;
            rb.linearVelocity = direction * 5f;
        }
    }

    protected override void UpdateAnimator()
    {
        float speed = isChasing ? chaseSpeed : moveSpeed; // Không đặt speed về 0 khi bắn
        animator.SetFloat("Speed", speed);
        animator.SetBool("IsAttacking", isShooting || isAttacking);
        animator.SetInteger("Direction", currentDirection);
    }

    protected override void Patrol()
    {
        if (isDead) return;

        if (isWaiting)
        {
            waitTimer += Time.deltaTime;
            if (waitTimer >= waitTimeAtPoint)
            {
                isWaiting = false;
                waitTimer = 0f;
                if (!isReturning)
                {
                    targetPoint = (targetPoint == pointA) ? pointB : pointA;
                    targetPoint = SanitizeVector(targetPoint, "targetPoint");
                }
                UpdatePatrolDirection();
            }
            return;
        }

        Vector2 currentPos = transform.position;
        targetPoint = SanitizeVector(targetPoint, "targetPoint");
        float distance = Vector2.Distance(currentPos, targetPoint);

        if (distance > 0.2f)
        {
            Vector2 newPos = Vector2.MoveTowards(currentPos, targetPoint, moveSpeed * Time.deltaTime);
            newPos = SanitizeVector(newPos, "newPos");
            transform.position = new Vector3(newPos.x, newPos.y, transform.position.z);
        }
        else
        {
            isWaiting = true;
            waitTimer = 0f;
            if (isReturning)
            {
                isReturning = false;
                targetPoint = pointA;
                targetPoint = SanitizeVector(targetPoint, "targetPoint");
                UpdatePatrolDirection();
            }
        }
    }

    protected override void ChasePlayer()
    {
        if (isDead) return;

        isAttacking = false;
        animator.SetBool("IsAttacking", false);
        if (!isChasing)
        {
            if (player == null || float.IsNaN(player.position.x) || float.IsNaN(player.position.y))
            {
                isChasing = false;
                return;
            }
            chaseDirection = ((Vector2)player.position - (Vector2)transform.position).normalized;
            chaseDirection = SanitizeVector(chaseDirection, "chaseDirection");
            if (moveHorizontally)
                chaseDirection.y = 0;
            else
                chaseDirection.x = 0;
            if (chaseDirection.sqrMagnitude > 0.0001f)
            {
                UpdateDirection(chaseDirection);
            }
            isChasing = true;
        }

        if (player == null || float.IsNaN(player.position.x) || float.IsNaN(player.position.y))
        {
            isChasing = false;
            return;
        }

        Vector2 currentPos = transform.position;
        Vector2 playerPos = SanitizeVector(player.position, "playerPos");
        float distance = Vector2.Distance(currentPos, playerPos);
        if (distance > attackRange)
        {
            Vector2 newPos = Vector2.MoveTowards(currentPos, playerPos, chaseSpeed * Time.deltaTime);
            newPos = SanitizeVector(newPos, "newPos");
            transform.position = new Vector3(newPos.x, newPos.y, transform.position.z);
        }
        else
        {
            isWaiting = true;
        }

        Vector2 directionToPlayer = ((Vector2)player.position - currentPos).normalized;
        directionToPlayer = SanitizeVector(directionToPlayer, "directionToPlayer");
        if (moveHorizontally)
            directionToPlayer.y = 0;
        else
            directionToPlayer.x = 0;
        if (directionToPlayer.sqrMagnitude > 0.0001f)
        {
            UpdateDirection(directionToPlayer);
        }
    }

    protected override void TryAttack()
    {
        if (isShooting) return;
        base.TryAttack();
    }

    protected override void OnTriggerEnter2D(Collider2D other)
    {
        base.OnTriggerEnter2D(other);
        if (other.CompareTag("Player"))
        {
            isChasing = true;
            isReturning = false;
        }
    }

    protected override void OnTriggerExit2D(Collider2D other)
    {
        base.OnTriggerExit2D(other);
        if (other.CompareTag("Player"))
        {
            isChasing = false;
            isShooting = false;
            isReturning = true;
            targetPoint = SanitizeVector(startPos, "startPos");
            UpdatePatrolDirection();
        }
    }

    protected override void OnValidate()
    {
        base.OnValidate();
        if (patrolDistance <= 0f)
        {
            patrolDistance = 2f;
        }
        if (shootRange <= 0f || shootRange > detectionRange)
        {
            shootRange = Mathf.Min(3f, detectionRange - 0.1f);
        }
        if (fireRate <= 0f)
        {
            fireRate = 1f;
        }
    }

    private void UpdatePatrolDirection()
    {
        Vector2 direction = isReturning ? (startPos - (Vector2)transform.position).normalized : (targetPoint - (Vector2)transform.position).normalized;
        direction = SanitizeVector(direction, "direction");
        if (direction.sqrMagnitude > 0.0001f)
        {
            UpdateDirection(direction);
        }
    }

    protected override void UpdateDirection(Vector2 direction)
    {
        direction = SanitizeVector(direction, "direction");
        if (moveHorizontally)
        {
            currentDirection = direction.x > 0 ? 1 : 3;
        }
        else
        {
            currentDirection = direction.y > 0 ? 0 : 2;
        }
        animator.SetInteger("Direction", currentDirection);
    }

    private Vector2 SanitizeVector(Vector2 vec, string context)
    {
        if (float.IsNaN(vec.x) || float.IsNaN(vec.y) || float.IsInfinity(vec.x) || float.IsInfinity(vec.y))
        {
            return Vector2.zero;
        }
        const float maxCoordinate = 1000f;
        return new Vector2(
            Mathf.Clamp(vec.x, -maxCoordinate, maxCoordinate),
            Mathf.Clamp(vec.y, -maxCoordinate, maxCoordinate)
        );
    }
}