using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]

public class ItemDetails
{
    public string itemDescription;
    public int itemCode;
    public ItemType itemType;
    public Sprite itemSprite;
    public string itemLongDescription;
    public short itemUseGridRadius;
    public float itemUseRadius;
    public bool isStartingItem;
    public bool canBePickedUp;
    public bool canBeDropped;
    public bool canBeUsed;
    public bool canBeCarried;
}
