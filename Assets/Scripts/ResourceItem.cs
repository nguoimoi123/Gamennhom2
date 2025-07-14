using UnityEngine;

public abstract class ResourceItem : ScriptableObject
{
    [SerializeField] private string itemName;
    [SerializeField] private Sprite icon;
    [SerializeField] private string description;
    [SerializeField] private string source;

    public string ItemName => itemName;
    public Sprite Icon => icon;
    public string Description => description;
    public string Source => source;

    public abstract void DisplayDetails(ResourceDisplay display);
}

public interface ResourceDisplay
{
    void ShowName(string name);
    void ShowIcon(Sprite icon);
    void ShowDescription(string description);
    void ShowSource(string source);
    void ClearDynamicFields();
}