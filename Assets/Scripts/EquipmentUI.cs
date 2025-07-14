using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class EquipmentUI : MonoBehaviour
{
    [SerializeField] private PlayerStatus playerStatus;
    [SerializeField] private Button weapon1SlotButton;
    [SerializeField] private TextMeshProUGUI weapon1SlotText;
    [SerializeField] private Image weapon1SlotIcon;
    [SerializeField] private Button weapon2SlotButton;
    [SerializeField] private TextMeshProUGUI weapon2SlotText;
    [SerializeField] private Image weapon2SlotIcon;
    [SerializeField] private Button innerShirtSlotButton;
    [SerializeField] private TextMeshProUGUI innerShirtSlotText;
    [SerializeField] private Image innerShirtSlotIcon;
    [SerializeField] private Button outerShirtSlotButton;
    [SerializeField] private TextMeshProUGUI outerShirtSlotText;
    [SerializeField] private Image outerShirtSlotIcon;
    [SerializeField] private Button pantsSlotButton;
    [SerializeField] private TextMeshProUGUI pantsSlotText;
    [SerializeField] private Image pantsSlotIcon;
    [SerializeField] private Button shoesSlotButton;
    [SerializeField] private TextMeshProUGUI shoesSlotText;
    [SerializeField] private Image shoesSlotIcon;
    [SerializeField] private Button hatSlotButton;
    [SerializeField] private TextMeshProUGUI hatSlotText;
    [SerializeField] private Image hatSlotIcon;
    [SerializeField] private Button backpackSlotButton;
    [SerializeField] private TextMeshProUGUI backpackSlotText;
    [SerializeField] private Image backpackSlotIcon;
    [SerializeField] private Button unequipButton;

    private Item selectedItem;
    private string selectedSlot;

    void Start()
    {
        if (playerStatus == null || weapon1SlotButton == null || weapon1SlotText == null || weapon1SlotIcon == null ||
            weapon2SlotButton == null || weapon2SlotText == null || weapon2SlotIcon == null ||
            innerShirtSlotButton == null || innerShirtSlotText == null || innerShirtSlotIcon == null ||
            outerShirtSlotButton == null || outerShirtSlotText == null || outerShirtSlotIcon == null ||
            pantsSlotButton == null || pantsSlotText == null || pantsSlotIcon == null ||
            shoesSlotButton == null || shoesSlotText == null || shoesSlotIcon == null ||
            hatSlotButton == null || hatSlotText == null || hatSlotIcon == null ||
            backpackSlotButton == null || backpackSlotText == null || backpackSlotIcon == null ||
            unequipButton == null)
        {
            enabled = false;
            return;
        }

        unequipButton.gameObject.SetActive(false);

        weapon1SlotButton.onClick.AddListener(() => OnSlotClicked(playerStatus.GetWeapon1(), "Weapon1"));
        weapon2SlotButton.onClick.AddListener(() => OnSlotClicked(playerStatus.GetWeapon2(), "Weapon2"));
        innerShirtSlotButton.onClick.AddListener(() => OnSlotClicked(playerStatus.GetInnerShirt(), "InnerShirt"));
        outerShirtSlotButton.onClick.AddListener(() => OnSlotClicked(playerStatus.GetOuterShirt(), "OuterShirt"));
        pantsSlotButton.onClick.AddListener(() => OnSlotClicked(playerStatus.GetPants(), "Pants"));
        shoesSlotButton.onClick.AddListener(() => OnSlotClicked(playerStatus.GetShoes(), "Shoes"));
        hatSlotButton.onClick.AddListener(() => OnSlotClicked(playerStatus.GetHat(), "Hat"));
        backpackSlotButton.onClick.AddListener(() => OnSlotClicked(playerStatus.GetBackpack(), "Backpack"));
        unequipButton.onClick.AddListener(OnUnequipClicked);

        UpdateEquipmentUI();
    }

    void Update()
    {
        UpdateEquipmentUI();
    }

    void UpdateEquipmentUI()
    {
        // Weapon 1
        Item weapon1 = playerStatus.GetWeapon1();
        weapon1SlotText.text = weapon1 != null ? $"Weapon 1: {weapon1.itemName}" : "Weapon 1: None";
        weapon1SlotIcon.sprite = weapon1?.icon;
        weapon1SlotIcon.enabled = weapon1 != null;

        // Weapon 2
        Item weapon2 = playerStatus.GetWeapon2();
        weapon2SlotText.text = weapon2 != null ? $"Weapon 2: {weapon2.itemName}" : "Weapon 2: None";
        weapon2SlotIcon.sprite = weapon2?.icon;
        weapon2SlotIcon.enabled = weapon2 != null;

        // Inner Shirt
        Item innerShirt = playerStatus.GetInnerShirt();
        innerShirtSlotText.text = innerShirt != null ? $"Inner Shirt: {innerShirt.itemName}" : "Inner Shirt: None";
        innerShirtSlotIcon.sprite = innerShirt?.icon;
        innerShirtSlotIcon.enabled = innerShirt != null;

        // Outer Shirt
        Item outerShirt = playerStatus.GetOuterShirt();
        outerShirtSlotText.text = outerShirt != null ? $"Outer Shirt: {outerShirt.itemName}" : "Outer Shirt: None";
        outerShirtSlotIcon.sprite = outerShirt?.icon;
        outerShirtSlotIcon.enabled = outerShirt != null;

        // Pants
        Item pants = playerStatus.GetPants();
        pantsSlotText.text = pants != null ? $"Pants: {pants.itemName}" : "Pants: None";
        pantsSlotIcon.sprite = pants?.icon;
        pantsSlotIcon.enabled = pants != null;

        // Shoes
        Item shoes = playerStatus.GetShoes();
        shoesSlotText.text = shoes != null ? $"Shoes: {shoes.itemName}" : "Shoes: None";
        shoesSlotIcon.sprite = shoes?.icon;
        shoesSlotIcon.enabled = shoes != null;

        // Hat
        Item hat = playerStatus.GetHat();
        hatSlotText.text = hat != null ? $"Hat: {hat.itemName}" : "Hat: None";
        hatSlotIcon.sprite = hat?.icon;
        hatSlotIcon.enabled = hat != null;

        // Backpack
        Item backpack = playerStatus.GetBackpack();
        backpackSlotText.text = backpack != null ? $"Backpack: {backpack.itemName}" : "Backpack: None";
        backpackSlotIcon.sprite = backpack?.icon;
        backpackSlotIcon.enabled = backpack != null;
    }

    void OnSlotClicked(Item item, string slot)
    {
        if (item == null) return;

        selectedItem = item;
        selectedSlot = slot;
        unequipButton.gameObject.SetActive(true);

    }

    void OnUnequipClicked()
    {
        if (selectedItem == null || selectedSlot == null) return;

        switch (selectedSlot)
        {
            case "Weapon1":
                playerStatus.UnequipWeapon1();
                break;
            case "Weapon2":
                playerStatus.UnequipWeapon2();
                break;
            case "InnerShirt":
                playerStatus.UnequipInnerShirt();
                break;
            case "OuterShirt":
                playerStatus.UnequipOuterShirt();
                break;
            case "Pants":
                playerStatus.UnequipPants();
                break;
            case "Shoes":
                playerStatus.UnequipShoes();
                break;
            case "Hat":
                playerStatus.UnequipHat();
                break;
            case "Backpack":
                playerStatus.UnequipBackpack();
                break;
        }

        selectedItem = null;
        selectedSlot = null;
        unequipButton.gameObject.SetActive(false);
    }
}