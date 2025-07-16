using UnityEngine;
using System.Collections.Generic;

public class ItemDropManager : MonoBehaviour
{
    public static ItemDropManager Instance { get; private set; }

    [System.Serializable]
    public class ItemDrop
    {
        public Item item;
        public float dropChance;
    }

    [SerializeField] private List<ItemDrop> defaultDrops;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void DropItems(Vector2 position, ItemDrop[] drops)
    {
        if (drops == null || drops.Length == 0)
        {
            return;
        }

        foreach (ItemDrop drop in drops)
        {
            if (Random.value <= drop.dropChance)
            {
                GameObject itemObj = new GameObject(drop.item.itemName);
                itemObj.transform.position = new Vector3(position.x, position.y, 0);
                itemObj.layer = LayerMask.NameToLayer("Default");
                DroppedItem droppedItem = itemObj.AddComponent<DroppedItem>();
                droppedItem.item = drop.item;
                droppedItem.amount = 1;
                SpriteRenderer sr = itemObj.AddComponent<SpriteRenderer>();
                sr.sortingOrder = 10;
            }
        }
    }
}