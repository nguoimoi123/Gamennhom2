using UnityEngine;
using UnityEngine.Tilemaps;

[CreateAssetMenu(fileName = "New Resource Tile", menuName = "Tiles/Resource Tile")]
public class ResourceTile : Tile
{
    public Item dropItem; // Item rớt khi phá (Wood, Stone, v.v.)
    public int minDropAmount = 1; // Số lượng tối thiểu
    public int maxDropAmount = 3; // Số lượng tối đa
    public float maxHealth = 100f; // Máu của tile
    public bool isDestructible = true; // Có thể phá hủy không
    public Item.ResourceType resourceType = Item.ResourceType.Wood; // Loại tài nguyên

    public override void GetTileData(Vector3Int position, ITilemap tilemap, ref UnityEngine.Tilemaps.TileData tileData)
    {
        base.GetTileData(position, tilemap, ref tileData);
        tileData.colliderType = base.colliderType;
    }
}