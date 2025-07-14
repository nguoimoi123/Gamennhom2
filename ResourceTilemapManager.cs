using UnityEngine;
using UnityEngine.Tilemaps;
using System.Collections.Generic;
using UnityEngine.SceneManagement;
using System.IO;
public class ResourceTilemapManager : MonoBehaviour
{
    public Tilemap resourceTilemap;
    public GameObject droppedItemPrefab;
    private Dictionary<Vector3Int, ResourceTileData> tileData = new Dictionary<Vector3Int, ResourceTileData>();
    [SerializeField] private ResourceTile grassTile;
    [SerializeField] private ResourceTile berryTile;
    [SerializeField] private ResourceTile woodTile;
    [SerializeField] private ResourceTile stoneTile;
    [SerializeField] private ResourceTile herbTile;
    [SerializeField] private ResourceTile treeiceTile;
    private bool isInitialized;
    private string currentSceneName;

    public ResourceTile GrassTile => grassTile;
    public ResourceTile BerryTile => berryTile;
    public ResourceTile TreeIceTile => treeiceTile;
    public bool IsInitialized => isInitialized;

    void Start()
    {
        if (grassTile == null) Debug.LogError("GrassTile chưa được gán trong ResourceTilemapManager! Gán trong Inspector.");
        if (berryTile == null) Debug.LogError("BerryTile chưa được gán trong ResourceTilemapManager! Gán trong Inspector.");
        if (woodTile == null) Debug.LogError("WoodTile chưa được gán trong ResourceTilemapManager! Gán trong Inspector.");
        if (stoneTile == null) Debug.LogError("StoneTile chưa được gán trong ResourceTilemapManager! Gán trong Inspector.");
        if (herbTile == null) Debug.LogError("HerbTile chưa được gán trong ResourceTilemapManager! Gán trong Inspector.");
        if (droppedItemPrefab == null) Debug.LogError("droppedItemPrefab chưa được gán trong ResourceTilemapManager! Gán prefab DroppedItem trong Inspector.");

        currentSceneName = SceneManager.GetActiveScene().name;
        string saveFilePath = Path.Combine(Application.persistentDataPath, "savegame.json");
        if (File.Exists(saveFilePath))
        {
            // Load game, để GameSaveManager xử lý
        }
        else
        {
            InitializeTilemap();
        }
    }

    public void InitializeTilemap()
    {
        isInitialized = false;
        tileData.Clear();
        int tileCount = 0;
        BoundsInt bounds = resourceTilemap.cellBounds;

        foreach (Vector3Int pos in bounds.allPositionsWithin)
        {
            ResourceTile tile = resourceTilemap.GetTile<ResourceTile>(pos);
            if (tile != null && tile.isDestructible)
            {
                tileData[pos] = new ResourceTileData(tile);
                tileCount++;
            }
        }

        isInitialized = true;
    }

    public void LoadTilemapForMapBasic()
    {
        isInitialized = false;

        if (resourceTilemap == null)
        {
            return;
        }
        if (droppedItemPrefab == null)
        {
            return;
        }

        if (resourceTilemap.layoutGrid == null || resourceTilemap.layoutGrid.cellLayout != GridLayout.CellLayout.Isometric)
        {
            return;
        }

        tileData.Clear();
        int tileCount = 0;
        BoundsInt bounds = resourceTilemap.cellBounds;

        foreach (Vector3Int pos in bounds.allPositionsWithin)
        {
            ResourceTile tile = resourceTilemap.GetTile<ResourceTile>(pos);
            if (tile != null && tile.isDestructible)
            {
                string key = $"Tile_{pos.x}_{pos.y}_{pos.z}";
                if (PlayerPrefs.GetInt(key, 1) == 0)
                {
                    resourceTilemap.SetTile(pos, null);
                }
                else
                {
                    tileData[pos] = new ResourceTileData(tile);
                    tileCount++;
                }
            }
        }



        isInitialized = true;
    }

    public void TakeDamage(Vector3Int tilePos, float damage)
    {
        if (!isInitialized) return;

        if (tileData.ContainsKey(tilePos))
        {
            var data = tileData[tilePos];
            data.currentHealth -= damage;
            if (data.currentHealth <= 0)
            {
                DropItems(tilePos);
                resourceTilemap.SetTile(tilePos, null);
                tileData.Remove(tilePos);
                string key = $"Tile_{tilePos.x}_{tilePos.y}_{tilePos.z}";
            }
        }
    }

    public void DestroyTile(Vector3Int tilePos)
    {
        if (!isInitialized || !tileData.ContainsKey(tilePos)) return;

        var tile = resourceTilemap.GetTile<ResourceTile>(tilePos);
        if (tile != null && tile.isDestructible)
        {
            DropItems(tilePos);
            resourceTilemap.SetTile(tilePos, null);
            tileData.Remove(tilePos);
            string key = $"Tile_{tilePos.x}_{tilePos.y}_{tilePos.z}";
        }
    }

    void DropItems(Vector3Int tilePos)
    {
        if (!tileData.ContainsKey(tilePos))
        {
            return;
        }

        var data = tileData[tilePos];
        if (data.tile.dropItem == null)
        {
            return;
        }

        int amount = Random.Range(data.tile.minDropAmount, data.tile.maxDropAmount + 1);
        Vector3 baseWorldPos = resourceTilemap.GetCellCenterWorld(tilePos);

        if (droppedItemPrefab == null)
        {
            Inventory.instance.AddItem(data.tile.dropItem, amount);
            return;
        }

        for (int i = 0; i < amount; i++)
        {
            Vector3 offset = new Vector3(Random.Range(-0.2f, 0.2f), Random.Range(-0.2f, 0.2f), 0);
            Vector3 worldPos = baseWorldPos + offset;

            GameObject droppedItemObj = Instantiate(droppedItemPrefab, worldPos, Quaternion.identity);
            DroppedItem droppedItem = droppedItemObj.GetComponent<DroppedItem>();
            if (droppedItem == null)
            {
                Destroy(droppedItemObj);
                Inventory.instance.AddItem(data.tile.dropItem, 1);
                continue;
            }

            droppedItem.item = data.tile.dropItem;
            droppedItem.amount = 1;
        }
    }

    public bool IsPositionEmpty(Vector3 worldPosition)
    {
        if (!isInitialized) return false;

        Vector3Int tilePos = resourceTilemap.WorldToCell(worldPosition);
        return resourceTilemap.GetTile(tilePos) == null;
    }

    public Item.ResourceType GetResourceType(Vector3Int tilePos)
    {
        if (!isInitialized) return Item.ResourceType.Wood;

        ResourceTile tile = resourceTilemap.GetTile<ResourceTile>(tilePos);
        return tile != null ? tile.resourceType : Item.ResourceType.Wood;
    }
}

public class ResourceTileData
{
    public float currentHealth;
    public ResourceTile tile;

    public ResourceTileData(ResourceTile tile)
    {
        this.tile = tile;
        currentHealth = tile.maxHealth;
    }
}