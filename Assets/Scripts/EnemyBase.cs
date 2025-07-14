using UnityEngine;

[RequireComponent(typeof(Rigidbody2D), typeof(Animator), typeof(CircleCollider2D))]
public abstract class EnemyBase : MonoBehaviour
{
    [Header("Enemy Stats")]
    [SerializeField] protected float moveSpeed = 3f;
    [SerializeField] protected float chaseSpeed = 4f;
    [SerializeField] protected float attackRange = 1f;
    [SerializeField] protected float detectionRange = 5f;
    [SerializeField] protected float attackCooldown = 1f;
    [SerializeField, Tooltip("Damage dealt per attack")] protected float attackDamage = 10f;
    [SerializeField, Tooltip("Maximum health of the enemy")] protected float maxHealth = 100f;

    [Header("Components")]
    [SerializeField] protected CircleCollider2D detectionCollider;

    [Header("Item Drops")]
    [SerializeField] protected ItemDropManager.ItemDrop[] customDrops;

    protected Rigidbody2D rb;
    protected Animator animator;
    protected Transform player;
    protected bool isAttacking;
    protected float lastAttackTime;
    protected int currentDirection; // 0: lên, 1: phải, 2: xuống, 3: trái
    protected float currentHealth;
    protected Vector2 spawnPoint;
    protected bool isDead;
    protected EnemySpawner spawner;

    protected virtual void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
        if (detectionCollider == null)
            detectionCollider = gameObject.AddComponent<CircleCollider2D>();

        rb.gravityScale = 0;
        rb.freezeRotation = true;
        detectionCollider.isTrigger = true;
        detectionCollider.radius = detectionRange;

        isAttacking = false;
        lastAttackTime = -attackCooldown;
        currentDirection = 2; // Mặc định hướng xuống
        currentHealth = maxHealth;
        isDead = false;
        spawnPoint = transform.position;
    }

    protected virtual void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player")?.transform;
    }

    protected virtual void Update()
    {
        if (isDead) return;

        UpdateAnimator();
        if (player != null && IsPlayerInDetectionRange())
        {
            if (Vector2.Distance(transform.position, player.position) <= attackRange)
            {
                TryAttack();
            }
            else
            {
                ChasePlayer();
            }
        }
        else
        {
            Patrol();
        }
    }

    protected virtual void UpdateAnimator()
    {
        float speed = rb.linearVelocity.magnitude / Mathf.Max(moveSpeed, chaseSpeed);
        animator.SetFloat("Speed", speed);
        animator.SetBool("IsAttacking", isAttacking);
        animator.SetInteger("Direction", currentDirection);
    }

    protected virtual bool IsPlayerInDetectionRange()
    {
        if (player == null) return false;
        return Vector2.Distance(transform.position, player.position) <= detectionRange;
    }

    protected virtual void ChasePlayer()
    {
        isAttacking = false;
    }

    protected virtual void TryAttack()
    {
        if (Time.time >= lastAttackTime + attackCooldown)
        {
            isAttacking = true;
            rb.linearVelocity = Vector2.zero;
            lastAttackTime = Time.time;
            if (player != null)
            {
                PlayerStatus playerStatus = player.GetComponent<PlayerStatus>();
                if (playerStatus != null)
                {
                    playerStatus.TakeDamage(attackDamage);
                }
            }
        }
    }

    protected virtual void Patrol()
    {
        rb.linearVelocity = Vector2.zero;
    }

    protected virtual Vector2 ConvertToIsometricVelocity(Vector2 direction)
    {
        return new Vector2(direction.x + direction.y, (direction.y - direction.x) * 0.5f);
    }

    protected virtual void UpdateDirection(Vector2 direction)
    {
        float absX = Mathf.Abs(direction.x);
        float absY = Mathf.Abs(direction.y);
        if (absX > absY)
        {
            currentDirection = direction.x > 0 ? 1 : 3;
        }
        else
        {
            currentDirection = direction.y > 0 ? 0 : 2;
        }
    }

    protected virtual void OnTriggerEnter2D(Collider2D other)
    {

    }

    protected virtual void OnTriggerExit2D(Collider2D other)
    {
    }

    protected virtual void OnValidate()
    {
        if (detectionCollider != null)
        {
            detectionCollider.radius = detectionRange;
        }
        if (maxHealth <= 0f)
        {
            maxHealth = 100f;
        }
        if (attackDamage < 0f)
        {
            attackDamage = 10f;
        }
    }

    public virtual void TakeDamage(float damage)
    {
        if (isDead) return;

        currentHealth -= damage;

        if (currentHealth <= 0)
        {
            Die();
        }
    }

    protected virtual void Die()
    {
        isDead = true;
        rb.linearVelocity = Vector2.zero;
        ItemDropManager.Instance?.DropItems(transform.position, customDrops);

        if (spawner != null)
        {
            spawner.NotifyEnemyDeath(this);
        }
        else
        {
            gameObject.SetActive(false);
        }
    }

    public void SetSpawner(EnemySpawner newSpawner)
    {
        spawner = newSpawner;
    }

    public void Respawn()
    {
        isDead = false;
        currentHealth = maxHealth;
        transform.position = spawnPoint;
        gameObject.SetActive(true);
    }

    public ItemDropManager.ItemDrop[] GetCustomDrops()
    {
        return customDrops;
    }
}