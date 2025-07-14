using System.Collections.Generic;
using UnityEngine;
using System;
using System.Linq;

public class Inventory : MonoBehaviour
{
    public static Inventory instance { get; private set; }

    public List<InventorySlot> slots = new List<InventorySlot>();
    [SerializeField] private int baseSlotCount = 20;
    private int additionalSlots = 0;
    private int maxSlots => baseSlotCount + additionalSlots;

    public event Action OnInventoryChanged;

    void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
            return;
        }
        instance = this;

        for (int i = 0; i < maxSlots; i++)
        {
            slots.Add(new InventorySlot());
        }
    }

    void OnDestroy()
    {
        if (instance == this)
        {
            instance = null;
        }
    }

    void Start()
    {
        Item rareGem = Resources.Load<Item>("Items/RareGem");
        Item axe = Resources.Load<Item>("Items/Axe 1");
        AddItem(axe, 1);
        AddItem(rareGem, 20);
    }

    public bool AddItem(Item item, int amount)
    {
        if (item == null || amount <= 0) return false;

        foreach (InventorySlot slot in slots)
        {
            if (slot.item == item && slot.amount < item.maxStackSize)
            {
                int canAdd = item.maxStackSize - slot.amount;
                int addedAmount = Mathf.Min(amount, canAdd);
                slot.amount += addedAmount;
                amount -= addedAmount;

                if (amount <= 0)
                {
                    TriggerInventoryChanged();
                    return true;
                }
            }
        }

        foreach (InventorySlot slot in slots)
        {
            if (slot.item == null)
            {
                slot.item = item;
                slot.amount = Mathf.Min(amount, item.maxStackSize);
                amount -= slot.amount;

                if (amount <= 0)
                {
                    TriggerInventoryChanged();
                    return true;
                }
            }
        }

        TriggerInventoryChanged();
        return amount <= 0;
    }

    public bool RemoveItem(Item item, int amount)
    {
        if (item == null || amount <= 0) return false;

        foreach (InventorySlot slot in slots)
        {
            if (slot.item == item && slot.amount >= amount)
            {
                slot.amount -= amount;
                if (slot.amount <= 0)
                {
                    slot.item = null;
                    slot.amount = 0;
                }
                TriggerInventoryChanged();
                return true;
            }
            else if (slot.item == item && slot.amount > 0)
            {
                amount -= slot.amount;
                slot.item = null;
                slot.amount = 0;
                if (amount <= 0)
                {
                    TriggerInventoryChanged();
                    return true;
                }
            }
        }
        TriggerInventoryChanged();
        return false;
    }
    public bool HasItem(string itemName, int amount)
    {
        Item item = ItemDatabase.instance.GetItemByName(itemName);
        if (item == null) return false;

        int totalAmount = 0;
        foreach (var slot in slots)
        {
            if (slot.item == item)
            {
                totalAmount += slot.amount;
            }
        }
        return totalAmount >= amount;
    }
    public void ClearInventory()
    {
        foreach (var slot in slots)
        {
            slot.item = null;
            slot.amount = 0;
        }
        TriggerInventoryChanged();
    }

    public void UseItem(int slotIndex)
    {
        if (slotIndex < 0 || slotIndex >= slots.Count || slots[slotIndex].item == null || slots[slotIndex].amount <= 0)
        {
            return;
        }

        InventorySlot slot = slots[slotIndex];
        Item item = slot.item;

        if (PlayerStatus.instance != null)
        {
            PlayerStatus.instance.UseItem(item);

            if (item.itemType == Item.ItemType.Utility && item.GetInventorySlotIncrease() > 0)
            {
                AdjustSlots(item.GetInventorySlotIncrease());
            }
            else if (item.itemType != Item.ItemType.Tool && item.itemType != Item.ItemType.Weapon && item.itemType != Item.ItemType.Utility && item.itemType != Item.ItemType.Armor)
            {
                RemoveItem(item, 1);
            }
        }

    }

    public void AdjustSlots(int slotChange)
    {
        int previousMaxSlots = maxSlots;
        additionalSlots += slotChange;
        if (additionalSlots < 0) additionalSlots = 0;

        if (maxSlots > previousMaxSlots)
        {
            for (int i = previousMaxSlots; i < maxSlots; i++)
            {
                slots.Add(new InventorySlot());
            }
        }
        else if (maxSlots < previousMaxSlots)
        {
            int slotsToRemove = previousMaxSlots - maxSlots;
            for (int i = slots.Count - 1; i >= 0 && slotsToRemove > 0; i--)
            {
                if (slots[i].item == null)
                {
                    slots.RemoveAt(i);
                    slotsToRemove--;
                }
            }
        }
        TriggerInventoryChanged();
    }

    public int GetMaxSlots() => maxSlots;

    public List<InventorySlot> GetSlots()
    {
        return new List<InventorySlot>(slots);
    }

    private void TriggerInventoryChanged()
    {
        OnInventoryChanged?.Invoke();
    }

    private string GetGameObjectPath(GameObject obj)
    {
        string path = obj.name;
        Transform parent = obj.transform.parent;
        while (parent != null)
        {
            path = parent.name + "/" + path;
            parent = parent.parent;
        }
        return path;
    }
    public void UpdateInventory()
    {
        InventoryUI.instance?.UpdateInventory();
    }
}

[System.Serializable]
public class InventorySlot
{
    public Item item;
    public int amount;

    public InventorySlot()
    {
        item = null;
        amount = 0;
    }
}