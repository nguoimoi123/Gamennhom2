using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.EventSystems;

public class InventorySlotUI : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [SerializeField] private Image itemImage;
    [SerializeField] private TextMeshProUGUI amountText;
    [SerializeField] private Outline outline;

    private int index;
    private InventoryUI ui;
    private Item item;

    public Sprite ItemIcon => itemImage.sprite;
    public string ItemName => item?.itemName ?? "Unknown";
    public string ItemDescription => item?.description ?? "No description";
    public Item.ItemType ItemType => item?.itemType ?? Item.ItemType.Resource;
    public int ItemMaxStackSize => item?.maxStackSize ?? 99;
    public int ItemAmount => int.TryParse(amountText.text, out int amount) ? amount : 0;
    public Item Item => item;

    public void Setup(Sprite icon, int amount, int index, InventoryUI ui)
    {
        this.index = index;
        this.ui = ui;
        itemImage.sprite = icon;
        amountText.text = amount.ToString();
        outline.enabled = false;

        if (Inventory.instance != null && index < Inventory.instance.GetSlots().Count)
        {
            item = Inventory.instance.GetSlots()[index].item;
        }
        else
        {
            Debug.LogWarning($"Inventory.instance is null or index {index} out of range!");
            item = null;
        }
    }

    public void SetOutline(bool enabled)
    {
        outline.enabled = enabled;
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (ui.SelectedSlotIndex != index)
            SetOutline(true);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (ui.SelectedSlotIndex != index)
            SetOutline(false);
    }
}