using UnityEngine;

[RequireComponent(typeof(Rigidbody2D), typeof(Animator), typeof(CircleCollider2D))]
public class PatrolEnemy : EnemyBase
{
    [Header("Patrol Settings")]
    [SerializeField] private float patrolDistance = 2f;
    [SerializeField] private float waitTimeAtPoint = 1f;
    [SerializeField] private bool moveHorizontally = true;

    private Vector2 pointA;
    private Vector2 pointB;
    private Vector2 targetPoint;
    private Vector2 startPos;
    private bool isWaiting;
    private float waitTimer;
    private bool isChasing;
    private bool isReturning;
    private Vector2 chaseDirection;

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
        UpdatePatrolDirection();
    }

    protected override void UpdateAnimator()
    {
        float speed = isWaiting ? 0f : (isChasing ? chaseSpeed : moveSpeed);
        animator.SetFloat("Speed", speed);
        animator.SetBool("IsAttacking", isAttacking);
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
            if (chaseDirection.sqrMagnitude > 0.0001f) // Kiểm tra vector không gần 0
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
        if (distance > 0.2f)
        {
            Vector2 newPos = Vector2.MoveTowards(currentPos, playerPos, chaseSpeed * Time.deltaTime);
            newPos = SanitizeVector(newPos, "newPos");
            transform.position = new Vector3(newPos.x, newPos.y, transform.position.z);
        }
        else
        {
            isWaiting = true; // Dừng khi gần người chơi
        }

        Vector2 directionToPlayer = ((Vector2)player.position - currentPos).normalized;
        directionToPlayer = SanitizeVector(directionToPlayer, "directionToPlayer");
        if (moveHorizontally)
            directionToPlayer.y = 0;
        else
            directionToPlayer.x = 0;
        if (directionToPlayer.sqrMagnitude > 0.0001f) // Kiểm tra vector không gần 0
        {
            UpdateDirection(directionToPlayer);
        }
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
    }

    private void UpdatePatrolDirection()
    {
        Vector2 direction = isReturning ? (startPos - (Vector2)transform.position).normalized : (targetPoint - (Vector2)transform.position).normalized;
        direction = SanitizeVector(direction, "direction");
        if (direction.sqrMagnitude > 0.0001f) // Kiểm tra vector không gần 0
        {
            UpdateDirection(direction);
        }
    }

    protected override void UpdateDirection(Vector2 direction)
    {
        direction = SanitizeVector(direction, "direction");
        if (moveHorizontally)
        {
            currentDirection = direction.x > 0 ? 1 : 3; // 1 = phải, 3 = trái
        }
        else
        {
            currentDirection = direction.y > 0 ? 0 : 2; // 0 = lên, 2 = xuống
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