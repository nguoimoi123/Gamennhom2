using UnityEngine;
using UnityEngine.Tilemaps;

[RequireComponent(typeof(Rigidbody2D), typeof(Animator), typeof(CircleCollider2D))]
public class PlayerController : MonoBehaviour
{
    [SerializeField] private float speed = 5f;
    [SerializeField] private float defaultAttackRange = 2f;
    [SerializeField] private float pickupRange = 1.5f;
    [SerializeField] private float attackCooldown = 0.5f;
    [SerializeField] private ResourceTilemapManager tilemapManager;
    [SerializeField] private Camera mainCamera;
    [SerializeField] private PlayerStatus playerStatus;

    private Rigidbody2D rb;
    private Animator animator;
    private CircleCollider2D pickupCollider;
    private Vector2 lastDirection;
    private float lastAttackTime;
    private bool isReady;

    public PlayerStatus PlayerStatus
    {
        get => playerStatus;
        set => playerStatus = value;
    }

    public Camera MainCamera
    {
        get => mainCamera;
        set => mainCamera = value;
    }

    public ResourceTilemapManager TilemapManager
    {
        get => tilemapManager;
        set => tilemapManager = value;
    }

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
        pickupCollider = GetComponent<CircleCollider2D>();
        playerStatus ??= GetComponent<PlayerStatus>();
        mainCamera ??= Camera.main;

        if (mainCamera == null)
        {
            Debug.LogError("Main Camera not found!");
        }
    }

    void Start()
    {
        if (rb == null || animator == null || pickupCollider == null)
        {
            enabled = false;
            return;
        }

        InitializeRigidbody();
        RefreshReferences();
        isReady = CheckReferences();
    }

    void Update()
    {
        if (!enabled || !isReady) return;

        RefreshReferences();
        isReady = CheckReferences();

        HandleInput();
        HandleMovement();
        HandleAttack();
    }

    private void InitializeRigidbody()
    {
        rb.gravityScale = 0;
        rb.freezeRotation = true;
        pickupCollider.isTrigger = true;
        pickupCollider.radius = pickupRange;
        lastAttackTime = -attackCooldown;
    }

    private void HandleInput()
    {
        if (Input.GetKeyDown(KeyCode.Alpha1))
        {
            playerStatus.SwitchWeapon(1);
        }
        else if (Input.GetKeyDown(KeyCode.Alpha2))
        {
            playerStatus.SwitchWeapon(2);
        }
    }

    private void HandleMovement()
    {
        Vector2 input = new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical")).normalized;
        Vector2 movement = new Vector2(input.x, input.y);

        // Biến đổi đầu vào sang tọa độ đẳng cự
        Vector2 isoVelocity = new Vector2(movement.x + movement.y, (movement.y - movement.x) * 0.5f) * speed;
        rb.linearVelocity = isoVelocity;

        UpdateAnimator(movement);
    }

    private void UpdateAnimator(Vector2 movement)
    {
        animator.SetFloat("MoveX", movement.x);
        animator.SetFloat("MoveY", movement.y);
        animator.SetBool("IsMoving", movement != Vector2.zero);

        if (movement != Vector2.zero)
        {
            lastDirection = movement;
            int direction = GetDirection(movement);
            animator.SetInteger("LastDirection", direction);
        }
    }

    private int GetDirection(Vector2 movement)
    {
        return movement.y > 0 ? 0 : movement.y < 0 ? 2 : movement.x > 0 ? 1 : 3;
    }

    private void HandleAttack()
    {
        if (Input.GetMouseButtonDown(0) && Time.time >= lastAttackTime + attackCooldown)
        {
            if (!AttackEnemy())
            {
                InteractWithTile();
            }
            lastAttackTime = Time.time;
        }
    }

    private bool CheckReferences()
    {
        return mainCamera != null && tilemapManager != null && playerStatus != null && tilemapManager.IsInitialized;
    }

    public void RefreshReferences()
    {
        if (mainCamera == null)
        {
            mainCamera = Camera.main;
        }

        if (tilemapManager == null || !tilemapManager.IsInitialized)
        {
            GameObject resourceManager = GameObject.Find("ResourceManager");
            if (resourceManager != null)
            {
                tilemapManager = resourceManager.GetComponent<ResourceTilemapManager>();
            }
        }

        if (playerStatus == null)
        {
            playerStatus = GetComponent<PlayerStatus>();
        }
    }

    void InteractWithTile()
    {
        if (!isReady || !tilemapManager.IsInitialized) return;

        Vector3 mousePos = mainCamera.ScreenToWorldPoint(Input.mousePosition);
        mousePos.z = 0;
        Vector3Int tilePos = tilemapManager.resourceTilemap.WorldToCell(mousePos);
        ResourceTile tile = tilemapManager.resourceTilemap.GetTile<ResourceTile>(tilePos);

        if (tile == null || !tile.isDestructible) return;

        float distanceToTile = Vector2.Distance(transform.position, tilemapManager.resourceTilemap.GetCellCenterWorld(tilePos));
        float distanceToMouse = Vector2.Distance(transform.position, mousePos);
        Item equippedWeapon = playerStatus.GetActiveWeapon();
        float attackRange = equippedWeapon?.GetAttackRange() ?? defaultAttackRange;

        if (distanceToTile <= attackRange && distanceToMouse <= attackRange)
        {
            HandleTileDestruction(tile, tilePos, equippedWeapon);
        }
    }

    private void HandleTileDestruction(ResourceTile tile, Vector3Int tilePos, Item equippedWeapon)
    {
        if (tile == tilemapManager.GrassTile || tile == tilemapManager.BerryTile)
        {
            tilemapManager.DestroyTile(tilePos);
            playerStatus.ConsumeStatsOnAttack();
            return;
        }

        if (equippedWeapon == null || (equippedWeapon.itemType != Item.ItemType.Tool && equippedWeapon.itemType != Item.ItemType.Weapon))
        {
            return;
        }

        float damage = equippedWeapon.GetAttackPower();
        float efficiency = equippedWeapon.GetHarvestEfficiency(tile.resourceType);
        float totalDamage = damage * efficiency;

        if (efficiency <= 0) return;

        if (equippedWeapon.itemName == "Axe" && tile.resourceType == Item.ResourceType.Wood)
        {
            totalDamage = Mathf.Max(totalDamage, tile.maxHealth);
        }

        playerStatus.ConsumeStatsOnAttack();
        playerStatus.ReduceDurability(equippedWeapon);
        tilemapManager.TakeDamage(tilePos, totalDamage);
    }

    bool AttackEnemy()
    {
        if (playerStatus == null || mainCamera == null) return false;

        Item equippedWeapon = playerStatus.GetActiveWeapon();
        float attackRange = equippedWeapon?.GetAttackRange() ?? defaultAttackRange;
        float damage = equippedWeapon?.GetAttackPower() ?? 10f;

        Vector3 mousePos = mainCamera.ScreenToWorldPoint(Input.mousePosition);
        float distanceToMouse = Vector2.Distance(transform.position, new Vector2(mousePos.x, mousePos.y));

        if (distanceToMouse > attackRange) return false;

        RaycastHit2D hit = Physics2D.Raycast(mousePos, Vector2.zero);
        if (hit.collider != null)
        {
            EnemyBase enemy = hit.collider.GetComponent<EnemyBase>();
            if (enemy != null)
            {
                enemy.TakeDamage(damage);
                playerStatus.ConsumeStatsOnAttack();
                playerStatus.ReduceDurability(equippedWeapon);
                return true;
            }
        }
        return false;
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        DroppedItem droppedItem = other.GetComponent<DroppedItem>();
        if (droppedItem != null && droppedItem.item != null && Inventory.instance != null)
        {
            Inventory.instance.AddItem(droppedItem.item, droppedItem.amount);
            Destroy(droppedItem.gameObject);
        }
    }
}