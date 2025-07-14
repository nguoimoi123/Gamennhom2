using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class PlayerStatus : MonoBehaviour
{
    public static PlayerStatus instance;

    [Header("Thanh Trạng Thái")]
    [SerializeField] private Slider healthSlider;
    [SerializeField] private Slider fatigueSlider;
    [SerializeField] private Slider hungerSlider;
    [SerializeField] private Slider thirstSlider;

    [Header("Giá Trị Trạng Thái")]
    [SerializeField] private float maxValue = 100f;
    private float currentHealth;
    private float currentFatigue;
    private float currentHunger;
    private float currentThirst;

    [Header("Tốc Độ Giảm")]
    [SerializeField] private float fatigueDecreaseRate = 0.1f;
    [SerializeField] private float hungerDecreaseRate = 0.2f;
    [SerializeField] private float thirstDecreaseRate = 0.3f;

    [Header("Hồi Phục Mặc Định")]
    [SerializeField] private float defaultPotionHealthRecovery = 30f;
    [SerializeField] private float defaultPotionThirstRecovery = 20f;
    [SerializeField] private float defaultFoodHungerRecovery = 25f;

    [Header("Tiêu Hao Khi Sử Dụng Vũ Khí")]
    [SerializeField] private float fatigueCostPerAttack = 5f;
    [SerializeField] private float hungerCostPerAttack = 2f;
    [SerializeField] private float thirstCostPerAttack = 2f;

    [Header("Trang Bị")]
    [SerializeField] private Item weapon1;
    [SerializeField] private Item weapon2;
    [SerializeField] private Item innerShirt;
    [SerializeField] private Item outerShirt;
    [SerializeField] private Item pants;
    [SerializeField] private Item shoes;
    [SerializeField] private Item hat;
    [SerializeField] private Item backpack;
    private float currentDurabilityWeapon1;
    private float currentDurabilityWeapon2;
    private Item activeWeapon;

    private bool isResting = false;
    private bool isDead = false;

    public delegate void WeaponSwitchedHandler(Item weapon);
    public event WeaponSwitchedHandler OnWeaponSwitched;

    void Awake()
    {
        if (instance != null)
        {
            Destroy(gameObject);
            return;
        }
        instance = this;
        DontDestroyOnLoad(gameObject);
    }

    void Start()
    {
        currentHealth = maxValue;
        currentFatigue = maxValue;
        currentHunger = maxValue;
        currentThirst = maxValue;
        activeWeapon = weapon1;
        UpdateUI();
    }

    void Update()
    {
        if (isDead) return;

        if (!isResting)
        {
            currentFatigue = Mathf.Max(0, currentFatigue - fatigueDecreaseRate * Time.deltaTime);
            currentHunger = Mathf.Max(0, currentHunger - hungerDecreaseRate * Time.deltaTime);
            currentThirst = Mathf.Max(0, currentThirst - thirstDecreaseRate * Time.deltaTime);
        }
        else
        {
            currentFatigue = Mathf.Min(maxValue, currentFatigue + defaultFoodHungerRecovery * Time.deltaTime);
        }

        if (Input.GetKeyDown(KeyCode.R))
        {
            ToggleRest();
        }

        CheckStatus();
        UpdateUI();
    }

    public void TakeDamage(float damage)
    {
        if (isDead) return;
        currentHealth = Mathf.Max(0, currentHealth - damage);
        UpdateUI();
        CheckStatus();
    }

    public void SwitchWeapon(int slot)
    {
        if (slot == 1 && weapon1 != null)
        {
            activeWeapon = weapon1;
        }
        else if (slot == 2 && weapon2 != null)
        {
            activeWeapon = weapon2;
        }
        OnWeaponSwitched?.Invoke(activeWeapon);
    }

    public Item GetActiveWeapon()
    {
        return activeWeapon;
    }

    public void UpdateUI()
    {
        if (healthSlider != null) healthSlider.value = currentHealth / maxValue;
        if (fatigueSlider != null) fatigueSlider.value = currentFatigue / maxValue;
        if (hungerSlider != null) hungerSlider.value = currentHunger / maxValue;
        if (thirstSlider != null) thirstSlider.value = currentThirst / maxValue;
    }

    void CheckStatus()
    {
        if (currentHealth <= 0 || currentHunger <= 0 || currentThirst <= 0)
        {
            Die();
        }
        else if (currentFatigue <= 0)
        {
            Debug.LogWarning("Player is too fatigued! Movement speed reduced.");
        }
    }

    void Die()
    {
        if (isDead) return;
        isDead = true;
        StartCoroutine(Respawn());
    }

    IEnumerator Respawn()
    {
        // Tải BaseMap nếu không phải BaseMap
        string currentScene = UnityEngine.SceneManagement.SceneManager.GetActiveScene().name;
        if (currentScene != "BaseMap")
        {
            GameManager.Instance.LoadNewMap("BaseMap");
            // Chờ cho đến khi BaseMap được tải
            yield return new WaitUntil(() => UnityEngine.SceneManagement.SceneManager.GetActiveScene().name == "BaseMap");
        }

        // Tìm trụ hồi sinh gần nhất
        RespawnManager manager = FindFirstObjectByType<RespawnManager>();
        Vector3 respawnPosition = Vector3.zero;
        if (manager != null)
        {
            // Sử dụng vị trí mặc định (0,0,0) để tìm trụ gần nhất trong BaseMap
            respawnPosition = manager.GetNearestRespawnPosition(Vector3.zero);
        }

        // Đặt vị trí người chơi
        transform.position = respawnPosition;

        // Reset trạng thái
        currentHealth = maxValue;
        currentFatigue = maxValue;
        currentHunger = maxValue;
        currentThirst = maxValue;
        isDead = false;
        isResting = false;

        // Xóa inventory
        if (Inventory.instance != null)
        {
            Inventory.instance.ClearInventory();
        }

        // Xóa trang bị
        weapon1 = null;
        weapon2 = null;
        activeWeapon = null;
        innerShirt = null;
        outerShirt = null;
        pants = null;
        shoes = null;
        hat = null;
        backpack = null;
        currentDurabilityWeapon1 = 0;
        currentDurabilityWeapon2 = 0;

        OnWeaponSwitched?.Invoke(activeWeapon);
        UpdateUI();
    }

    void ToggleRest()
    {
        if (isDead) return;
        isResting = !isResting;
    }

    public void UseItem(Item item)
    {
        if (isDead || item == null)
        {
            return;
        }

        float healthToAdd = item.GetHealthRecovery() > 0 ? item.GetHealthRecovery() : defaultPotionHealthRecovery;
        float hungerToAdd = item.GetHungerRecovery() > 0 ? item.GetHungerRecovery() : defaultFoodHungerRecovery;
        float thirstToAdd = item.GetThirstRecovery() > 0 ? item.GetThirstRecovery() : defaultPotionThirstRecovery;

        switch (item.itemType)
        {
            case Item.ItemType.Potion:
                currentHealth = Mathf.Min(maxValue, currentHealth + healthToAdd);
                currentThirst = Mathf.Min(maxValue, currentThirst + thirstToAdd);
                break;
            case Item.ItemType.Food:
                currentHunger = Mathf.Min(maxValue, currentHunger + hungerToAdd);
                break;
            case Item.ItemType.Tool:
            case Item.ItemType.Weapon:
                EquipWeapon(item);
                break;
            case Item.ItemType.Armor:
            case Item.ItemType.Utility:
                EquipArmor(item);
                break;
            default:
                break;
        }

        UpdateUI();
    }

    void EquipWeapon(Item item)
    {
        if (weapon1 == null)
        {
            weapon1 = item;
            currentDurabilityWeapon1 = item.GetMaxDurability();
            activeWeapon = weapon1;
        }
        else if (weapon2 == null)
        {
            weapon2 = item;
            currentDurabilityWeapon2 = item.GetMaxDurability();
            if (activeWeapon == null) activeWeapon = weapon2;
        }
        else
        {
            weapon1 = item;
            currentDurabilityWeapon1 = item.GetMaxDurability();
            activeWeapon = weapon1;
        }
        OnWeaponSwitched?.Invoke(activeWeapon);
    }

    void EquipArmor(Item item)
    {
        switch (item.itemName.ToLower())
        {
            case string s when s.Contains("inner shirt"):
                innerShirt = item;
                break;
            case string s when s.Contains("outer shirt"):
                outerShirt = item;
                break;
            case string s when s.Contains("pants"):
                pants = item;
                break;
            case string s when s.Contains("shoes"):
                shoes = item;
                break;
            case string s when s.Contains("hat"):
                hat = item;
                break;
            case string s when s.Contains("backpack"):
                backpack = item;
                if (Inventory.instance != null)
                {
                    Inventory.instance.AdjustSlots(item.GetInventorySlotIncrease());
                }
                break;
        }
    }

    public void ReduceDurability(Item weapon)
    {
        if (weapon == null) return;

        if (weapon == weapon1)
        {
            currentDurabilityWeapon1 -= weapon.GetDurabilityPerUse();
            if (currentDurabilityWeapon1 <= 0)
            {
                if (Inventory.instance != null)
                {
                    Inventory.instance.RemoveItem(weapon, 1);
                }
                weapon1 = null;
                if (activeWeapon == weapon) activeWeapon = weapon2;
                OnWeaponSwitched?.Invoke(activeWeapon);
            }
        }
        else if (weapon == weapon2)
        {
            currentDurabilityWeapon2 -= weapon.GetDurabilityPerUse();
            if (currentDurabilityWeapon2 <= 0)
            {
                if (Inventory.instance != null)
                {
                    Inventory.instance.RemoveItem(weapon, 1);
                }
                weapon2 = null;
                if (activeWeapon == weapon) activeWeapon = weapon1;
                OnWeaponSwitched?.Invoke(activeWeapon);
            }
        }
    }

    public void ConsumeStatsOnAttack()
    {
        currentFatigue = Mathf.Max(0, currentFatigue - fatigueCostPerAttack);
        currentHunger = Mathf.Max(0, currentHunger - hungerCostPerAttack);
        currentThirst = Mathf.Max(0, currentThirst - thirstCostPerAttack);
        UpdateUI();
    }

    public Item GetWeapon1() => weapon1;
    public Item GetWeapon2() => weapon2;
    public Item GetInnerShirt() => innerShirt;
    public Item GetOuterShirt() => outerShirt;
    public Item GetPants() => pants;
    public Item GetShoes() => shoes;
    public Item GetHat() => hat;
    public Item GetBackpack() => backpack;

    public float GetHealth() => currentHealth;
    public float GetFatigue() => currentFatigue;
    public float GetHunger() => currentHunger;
    public float GetThirst() => currentThirst;

    public void UnequipWeapon1()
    {
        if (weapon1 != null)
        {
            if (Inventory.instance != null)
            {
                Inventory.instance.AddItem(weapon1, 1);
            }
            weapon1 = null;
            currentDurabilityWeapon1 = 0;
            if (activeWeapon == weapon1) activeWeapon = weapon2;
            OnWeaponSwitched?.Invoke(activeWeapon);
        }
    }

    public void UnequipWeapon2()
    {
        if (weapon2 != null)
        {
            if (Inventory.instance != null)
            {
                Inventory.instance.AddItem(weapon2, 1);
            }
            weapon2 = null;
            currentDurabilityWeapon2 = 0;
            if (activeWeapon == weapon2) activeWeapon = weapon1;
            OnWeaponSwitched?.Invoke(activeWeapon);
        }
    }

    public void UnequipInnerShirt()
    {
        if (innerShirt != null)
        {
            if (Inventory.instance != null)
            {
                Inventory.instance.AddItem(innerShirt, 1);
            }
            innerShirt = null;
        }
    }

    public void UnequipOuterShirt()
    {
        if (outerShirt != null)
        {
            if (Inventory.instance != null)
            {
                Inventory.instance.AddItem(outerShirt, 1);
            }
            outerShirt = null;
        }
    }

    public void UnequipPants()
    {
        if (pants != null)
        {
            if (Inventory.instance != null)
            {
                Inventory.instance.AddItem(pants, 1);
            }
            pants = null;
        }
    }

    public void UnequipShoes()
    {
        if (shoes != null)
        {
            if (Inventory.instance != null)
            {
                Inventory.instance.AddItem(shoes, 1);
            }
            shoes = null;
        }
    }

    public void UnequipHat()
    {
        if (hat != null)
        {
            if (Inventory.instance != null)
            {
                Inventory.instance.AddItem(hat, 1);
            }
            hat = null;
        }
    }

    public void UnequipBackpack()
    {
        if (backpack != null)
        {
            if (Inventory.instance != null)
            {
                Inventory.instance.AddItem(backpack, 1);
                Inventory.instance.AdjustSlots(-backpack.GetInventorySlotIncrease());
            }
            backpack = null;
        }
    }
}