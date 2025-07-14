using UnityEngine;
using UnityEngine.UI;

public class WeaponSwitchUI : MonoBehaviour
{
    [SerializeField] private PlayerStatus playerStatus;
    [SerializeField] private Button weapon1Button;
    [SerializeField] private Button weapon2Button;
    [SerializeField] private Image weapon1Icon;
    [SerializeField] private Image weapon2Icon;
    [SerializeField] private Image weapon1Highlight;
    [SerializeField] private Image weapon2Highlight;

    void Start()
    {
        weapon1Button.onClick.AddListener(() => playerStatus.SwitchWeapon(1));
        weapon2Button.onClick.AddListener(() => playerStatus.SwitchWeapon(2));
        playerStatus.OnWeaponSwitched += UpdateUI;
        UpdateUI(playerStatus.GetActiveWeapon());
    }

    public void UpdateUI(Item activeWeapon)
    {
        weapon1Icon.sprite = playerStatus.GetWeapon1()?.icon ?? null;
        weapon2Icon.sprite = playerStatus.GetWeapon2()?.icon ?? null;
        weapon1Icon.enabled = weapon1Icon.sprite != null;
        weapon2Icon.enabled = weapon2Icon.sprite != null;

        weapon1Highlight.color = (activeWeapon == playerStatus.GetWeapon1() && activeWeapon != null) ? Color.yellow : new Color(1, 1, 1, 0);
        weapon2Highlight.color = (activeWeapon == playerStatus.GetWeapon2() && activeWeapon != null) ? Color.yellow : new Color(1, 1, 1, 0);
    }

    void OnDestroy()
    {
        playerStatus.OnWeaponSwitched -= UpdateUI;
    }
}