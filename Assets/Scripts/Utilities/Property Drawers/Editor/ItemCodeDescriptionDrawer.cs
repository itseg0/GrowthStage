using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System;

[CustomPropertyDrawer(typeof(ItemCodeDescriptionAttribute))]

public class ItemCodeDescriptionDrawer : PropertyDrawer
{
    public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
    {
        // Overriding the base property height to be double to correctly show both the item code and item description
        // Otherwise it could only show one

        return EditorGUI.GetPropertyHeight(property) * 2;
    }

    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        // Using BeginPropert & EndProperty on the parent property means that the prefab override logic works on the entire property
    
        EditorGUI.BeginProperty(position, label, property);

        if(property.propertyType == SerializedPropertyType.Integer)
        {
            EditorGUI.BeginChangeCheck(); // Start of check for changed values in editor

            // Draw item code (Needs to be half of the original doubled height because it's only taking up 1/2 'slots'
            var newValue = EditorGUI.IntField(new Rect(position.x, position.y, position.width, position.height / 2), label, property.intValue);

            // Draw item description
            EditorGUI.LabelField(new Rect(position.x, position.y + position.height / 2, position.width, position.height / 2), "Item Description", GetItemDescription(property.intValue));

            // If item code value has changed, then set value to new value
            if(EditorGUI.EndChangeCheck() )
            {
                property.intValue = newValue;
            }
        }

        EditorGUI.EndProperty();
    }

    private string GetItemDescription(int itemCode)
    {
        SO_ItemList so_itemList;

        so_itemList = AssetDatabase.LoadAssetAtPath("Assets/Scriptable Objects/Items/so_ItemList.asset",typeof(SO_ItemList)) as SO_ItemList;

        List<ItemDetails> itemDetailsList = so_itemList.itemDetails;

        ItemDetails itemDetail = itemDetailsList.Find(x => x.itemCode == itemCode);

        if(itemDetail != null)
        {
            return itemDetail.itemDescription;
        }
        else
        {
            return "Item not found, check so_ItemList";
        }
    }
}
