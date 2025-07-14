using UnityEngine;

[CreateAssetMenu(fileName = "New Item", menuName = "Inventory/Item")]
public class Item : ScriptableObject
{
    public string itemName;
    public string description;
    public Sprite icon;
    public int maxStackSize = 99;
    public ItemType itemType;

    [System.Serializable]
    public struct RecoveryStats
    {
        public float healthRecovery;
        public float hungerRecovery;
        public float thirstRecovery;
    }

    [System.Serializable]
    public struct EquipmentStats
    {
        public float maxDurability;
        public float attackPower;
        public float durabilityPerUse;
        public float attackRange;
    }

    [System.Serializable]
    public struct HarvestStats
    {
        public float woodEfficiency;
        public float stoneEfficiency;
        public float oreEfficiency;
    }

    [System.Serializable]
    public struct UtilityStats
    {
        public int inventorySlotIncrease;
    }

    [SerializeField] private RecoveryStats recoveryStats;
    [SerializeField] private EquipmentStats equipmentStats;
    [SerializeField] private HarvestStats harvestStats;
    [SerializeField] private UtilityStats utilityStats;

    public enum ItemType
    {
        Resource,
        Tool,
        Weapon,
        Armor,
        Utility,
        CraftingStation,
        Potion,
        Food
    }

    public enum ResourceType
    {
        Wood,
        Stone,
        Ore,
        Special
    }

    public float GetHealthRecovery() => itemType == ItemType.Potion || itemType == ItemType.Food ? recoveryStats.healthRecovery : 0f;
    public float GetHungerRecovery() => itemType == ItemType.Food ? recoveryStats.hungerRecovery : 0f;
    public float GetThirstRecovery() => itemType == ItemType.Potion ? recoveryStats.thirstRecovery : 0f;
    public float GetMaxDurability() => itemType == ItemType.Tool || itemType == ItemType.Weapon ? equipmentStats.maxDurability : 0f;
    public float GetAttackPower() => itemType == ItemType.Tool || itemType == ItemType.Weapon ? equipmentStats.attackPower : 0f;
    public float GetDurabilityPerUse() => itemType == ItemType.Tool || itemType == ItemType.Weapon ? equipmentStats.durabilityPerUse : 0f;
    public float GetAttackRange() => itemType == ItemType.Tool || itemType == ItemType.Weapon ? equipmentStats.attackRange : 2f;
    public float GetHarvestEfficiency(ResourceType resourceType)
    {
        if (itemType != ItemType.Tool) return 0f;
        switch (resourceType)
        {
            case ResourceType.Wood:
                return harvestStats.woodEfficiency;
            case ResourceType.Stone:
                return harvestStats.stoneEfficiency;
            case ResourceType.Ore:
                return harvestStats.oreEfficiency;
            default:
                return 0f;
        }
    }
    public int GetInventorySlotIncrease() => itemType == ItemType.Utility ? utilityStats.inventorySlotIncrease : 0;

}