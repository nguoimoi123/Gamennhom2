using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(Item))]
public class ItemEditor : Editor
{
    public override void OnInspectorGUI()
    {
        Item item = (Item)target;
        SerializedObject serializedObject = new SerializedObject(item);

        // Hiển thị các thuộc tính chung
        EditorGUILayout.PropertyField(serializedObject.FindProperty("itemName"));
        EditorGUILayout.PropertyField(serializedObject.FindProperty("description"));
        EditorGUILayout.PropertyField(serializedObject.FindProperty("icon"));
        EditorGUILayout.PropertyField(serializedObject.FindProperty("maxStackSize"));
        EditorGUILayout.PropertyField(serializedObject.FindProperty("itemType"));

        // Hiển thị struct dựa trên itemType
        switch (item.itemType)
        {
            case Item.ItemType.Potion:
            case Item.ItemType.Food:
                EditorGUILayout.PropertyField(serializedObject.FindProperty("recoveryStats"));
                break;
            case Item.ItemType.Tool:
                EditorGUILayout.PropertyField(serializedObject.FindProperty("equipmentStats"));
                EditorGUILayout.PropertyField(serializedObject.FindProperty("harvestStats"));
                break;
            case Item.ItemType.Weapon:
                EditorGUILayout.PropertyField(serializedObject.FindProperty("equipmentStats"));
                break;
            case Item.ItemType.Utility:
                EditorGUILayout.PropertyField(serializedObject.FindProperty("utilityStats"));
                break;
                // Các loại khác (Resource, Armor, CraftingStation) không hiển thị struct
        }

        serializedObject.ApplyModifiedProperties();
    }
}