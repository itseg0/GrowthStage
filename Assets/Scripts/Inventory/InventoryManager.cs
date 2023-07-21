using System;
using System.Collections.Generic;
using UnityEngine;

public class InventoryManager : SingletonMonobehaviour<InventoryManager>

{
    private UIInventoryBar inventoryBar;
    private Dictionary<int, ItemDetails> itemDetailsDictionary;
    private int[] selectedInventoryItem; // Index of the array is the inventory list, and the value is the item code

    public List<InventoryItem>[] inventoryLists;
    public Dictionary<int, InventoryItem>[] inventoryDictionaries;

    [HideInInspector] public int[] inventoryListCapacityIntArray; // The index of the array is the invetory list (from the InventoryLocation enum), and the value is the capacity of that inventory list

    [SerializeField] private SO_ItemList itemList = null;


    protected override void Awake()

    {
        base.Awake();

        //Create item details dictionary
        CreateItemDetailsDictionary();

        //Create inventory lists
        CreateInventoryLists();

        //Initalise selected inv item array
        selectedInventoryItem = new int[(int)InventoryLocation.count];

        for (int i = 0; i < selectedInventoryItem.Length; i++)
        {

            selectedInventoryItem[i] = -1;

        }
    }

    private void Start()

    {

        inventoryBar = FindObjectOfType<UIInventoryBar>();

    }


    /// <summary>
    /// Populates the inventoryLists with the count of InventoryLocation and then creates a list of InventoryItem for each.
    /// <br>Also initialises both the inventory list capacity array and the player inventory list capacity.</br>
    /// </summary>
    
    private void CreateInventoryLists()
    {

        //initialize capacity array
        inventoryListCapacityIntArray = new int[(int)InventoryLocation.count];

        //player inv capacity
        inventoryListCapacityIntArray[(int)InventoryLocation.player] = Settings.playerInitialInventoryCapacity;

        //chest inv capacity
        inventoryListCapacityIntArray[(int)InventoryLocation.chest] = Settings.chestInventoryCapacity;

        inventoryDictionaries = new Dictionary<int, InventoryItem>[(int)InventoryLocation.count];

        //create dict for player inv
        Dictionary<int, InventoryItem> playerDict = new Dictionary<int, InventoryItem>();

        for (int i = 0; i < inventoryListCapacityIntArray[(int)InventoryLocation.player]; i++)
        {
            InventoryItem invItem;

            invItem.itemCode = 0;

            invItem.itemQuantity = 0;

            playerDict.Add(i, invItem);
        }

        inventoryDictionaries[(int)InventoryLocation.player] = playerDict;

        //create dict for chest inv
        Dictionary<int, InventoryItem> chestDict = new Dictionary<int, InventoryItem>();

        for (int i = 0; i < inventoryListCapacityIntArray[(int)InventoryLocation.chest]; i++)
        {

            InventoryItem invItem;

            invItem.itemCode = 0;

            invItem.itemQuantity = 0;

            chestDict.Add(i, invItem);
        }

        inventoryDictionaries[(int)InventoryLocation.chest] = chestDict;
    }

    /// <summary>
    /// Populates the itemDetailsDictionary from the scriptable objects items list
    /// </summary>
    
    private void CreateItemDetailsDictionary()
    {
        itemDetailsDictionary = new Dictionary<int, ItemDetails>();

        foreach (ItemDetails itemDetails in itemList.itemDetails)
        {
            itemDetailsDictionary.Add(itemDetails.itemCode, itemDetails);
        }
    }

    /// <summary>
    /// Add an item to the inventory list for the inventoryLocation and then destroy the gameObjectToDelete
    /// </summary>
    
    public void AddItem(InventoryLocation inventoryLocation, Item item, GameObject gameObjectToDelete)
    {
        AddItem(inventoryLocation, item);
        Destroy(gameObjectToDelete);
    }

    /// <summary>
    /// Add an item to the inventory list for the inventoryLocation
    /// </summary>
    
    public void AddItem(InventoryLocation inventoryLocation, Item item)
    {
        int itemCode = item.ItemCode;
        AddItem(inventoryLocation, itemCode);
    }

    /// <summary>
    /// Add item with itemcode
    /// </summary>
    
    public void AddItem(InventoryLocation inventoryLocation, int itemCode)
    {
        Dictionary<int, InventoryItem> inventoryDict = inventoryDictionaries[(int)inventoryLocation];

        // Check if inventory already contains the item
        int itemPosition = FindItemInInventory(inventoryLocation, itemCode);

        if (itemPosition != -1)
        {
            AddItemAtPosition(inventoryDict, itemCode, itemPosition);
        }
        else
        {
            AddItemAtPosition(inventoryDict, itemCode);
        }

        EventHandler.CallInventoryUpdatedEvent(inventoryLocation, inventoryDictionaries[(int)inventoryLocation]);
    }



    private int GetFirstEmptyItemSlot(Dictionary<int, InventoryItem> inventoryDict)
    { 
        foreach (KeyValuePair<int, InventoryItem> item in inventoryDict)
        {
            if (item.Value.itemCode == 0) return item.Key;
        }
        return -1;
    }

    /// <summary>
    /// Add item at the end of the inventory
    /// </summary>

    private void AddItemAtPosition(Dictionary<int, InventoryItem> inventoryDict, int itemCode)
    {
        InventoryItem inventoryItem = new InventoryItem();

        int itemSlot = GetFirstEmptyItemSlot(inventoryDict);

        if (itemSlot != -1)
        {
            inventoryItem.itemCode = itemCode;
            inventoryItem.itemQuantity = 1;
            inventoryDict[itemSlot] = inventoryItem;
        }
        //DebugPrintInventoryList(inventoryList);
    }

    /// <summary>
    /// Add Quantity to item
    /// </summary>

    private void AddItemAtPosition(Dictionary<int, InventoryItem> inventoryDict, int itemCode, int position)
    {
        InventoryItem inventoryItem = new InventoryItem();

        int quantity = inventoryDict[position].itemQuantity + 1;
        inventoryItem.itemCode = itemCode;
        inventoryItem.itemQuantity = quantity;
        inventoryDict[position] = inventoryItem;

        // DebugPrintInventoryList(inventoryList);
    }

    /// <summary>
    /// Swap item indexes in inventory bar
    /// </summary>

    public void SwapInventoryItems(InventoryLocation inventoryLocation, int fromItem, int toItem)
    {
        // If fromItem index and toItem index are wwithin the bounds of the list, not the same and also greather than or equal to zero

        if (fromItem != toItem && fromItem >= 0 && toItem >= 0)
        {
            if (inventoryDictionaries[(int)inventoryLocation].ContainsKey(toItem))
            {
                InventoryItem fromInvItem = inventoryDictionaries[(int)inventoryLocation][fromItem];
                InventoryItem toInvItem = inventoryDictionaries[(int)inventoryLocation][toItem];

                inventoryDictionaries[(int)inventoryLocation][toItem] = fromInvItem;
                inventoryDictionaries[(int)inventoryLocation][fromItem] = toInvItem;

                // Send event that inventory has been update
                EventHandler.CallInventoryUpdatedEvent(inventoryLocation, inventoryDictionaries[(int)inventoryLocation]);

            }
        }
    }

    public void ClearSelectedInventoryItem(InventoryLocation inventoryLocation)
    {
        selectedInventoryItem[(int)inventoryLocation] = -1;
    }

    /// <summary>
    /// Find if an itemCode is already in the inventory. 
    /// </summary>
    /// <returns>Either position of item or -1 if not found</returns>

    public int FindItemInInventory(InventoryLocation inventoryLocation, int itemCode)
    {
        Dictionary<int, InventoryItem> inventoryDict = inventoryDictionaries[(int)inventoryLocation];

        foreach (KeyValuePair<int, InventoryItem> item in inventoryDict)
        {
            if (item.Value.itemCode == itemCode) return item.Key;
        }

        return -1;
    }

    /// <summary>
    /// Returns the itemDetails (From the SO_ItemList) for the itemCode, or null if the item code does not exist
    /// </summary>

    public ItemDetails GetItemDetails(int itemCode)
    {
        ItemDetails itemDetails;

        if (itemDetailsDictionary.TryGetValue(itemCode, out itemDetails))
        {
            return itemDetails;
        }
        else
        {
            return null;
        }
    }

    public ItemDetails GetSelectedInventoryItemDetails(InventoryLocation inventoryLocation)
    {
        int itemCode = GetSelectedInventoryItem(inventoryLocation);

        if(itemCode == -1) 
            return null;

        else
            return GetItemDetails(itemCode);
    }

    /// <summary>
    /// Get the selected item for the inventoryLocation - returns ItemCode or -1 if nothing is selected
    /// </summary>
    private int GetSelectedInventoryItem(InventoryLocation inventoryLocation)
    {
        return selectedInventoryItem[(int)inventoryLocation];
    }

    /// <summary>
    /// Returns itemDetails for selected item in inv location or null if nothing is selected
    /// </summary>

    public ItemDetails GetSelectedInvetoryItemDetails(InventoryLocation inventoryLocation)
    {
        int itemCode = GetSelectedInvetoryItem(inventoryLocation);

        if (itemCode == -1)
        {
            return null;
        }
        else
        {
            return GetItemDetails(itemCode);
        }
    }

    /// <summary>
    /// Get the selected item for the inv location, returns item code ... -1 if nothing
    /// </summary>

    private int GetSelectedInvetoryItem(InventoryLocation inventoryLocation)
    {
        return selectedInventoryItem[(int)inventoryLocation];
    }

    public string GetItemDescription(ItemType itemType)
    {
        string itemTypeDescription;
        switch (itemType)
        {
            case ItemType.Hoeing_Tool:
                itemTypeDescription = Settings.HoeingTool;
                break;

            default:
                itemTypeDescription = itemType.ToString();
                break;
        }
        return itemTypeDescription;
    }

    /// <summary>
    /// Remove item from the inventory
    /// </summary>

    public void RemoveItem(InventoryLocation inventoryLocation, int itemCode)
    {
        Dictionary<int, InventoryItem> inventoryDict = inventoryDictionaries[(int)inventoryLocation];

        // Check if inventory already contains item
        int itemPosition = FindItemInInventory(inventoryLocation, itemCode);

        if (itemPosition != -1)
        {
            RemoveItemAtPosition(inventoryDict, itemCode, itemPosition);
        }

        // Send event that inventory has been updated
        EventHandler.CallInventoryUpdatedEvent(inventoryLocation, inventoryDictionaries[(int)inventoryLocation]);
    }

    /// <summary>
    /// Remove item at a specific position from the inventory
    /// </summary>

    private void RemoveItemAtPosition(Dictionary<int, InventoryItem> inventoryDict, int itemCode, int itemPosition)
    {
        InventoryItem inventoryItem = new InventoryItem();

        int quantity = inventoryDict[itemPosition].itemQuantity - 1;

        if (quantity > 0)
        {
            inventoryItem.itemQuantity = quantity;
            inventoryItem.itemCode = itemCode;
        }
        else
        {
            inventoryItem.itemQuantity = 0;
            inventoryItem.itemCode = 0;
        }

        inventoryDict[itemPosition] = inventoryItem;
    }

    /// <summary>
    /// Set the selected inventory item for inventoryLocation to itemCode
    /// </summary>

    public void SetSelectedInventoryItem(InventoryLocation inventoryLocation, int itemCode)
    {
        selectedInventoryItem[(int)inventoryLocation] = itemCode;
    }
}