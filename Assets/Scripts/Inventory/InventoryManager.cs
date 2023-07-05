using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;

public class InventoryManager : SingletonMonobehaviour<InventoryManager>
{
    private Dictionary<int, ItemDetails> itemDetailsDictionary;

    public List<InventoryItem>[] inventoryLists;

    [HideInInspector] public int[] inventoryListCapacityIntArray; // The index of the array is the invetory list (from the InventoryLocation enum), and the value is the capacity of that inventory list

    [SerializeField] private SO_ItemList itemList = null;


    private void Start()
    {
        base.Awake();

        // Create inventory lists
        CreateInventoryLists();

        // Create item details dictionary
        CreateItemDetailsDictionary();
    }

    /// <summary>
    /// Populates the inventoryLists with the count of InventoryLocation and then creates a list of InventoryItem for each.
    /// <br>Also initialises both the inventory list capacity array and the player inventory list capacity.</br>
    /// </summary>

    private void CreateInventoryLists()
    {
        inventoryLists = new List<InventoryItem>[(int)InventoryLocation.count];

        for(int i = 0; i < (int)InventoryLocation.count; i++)
        {
            inventoryLists[i] = new List<InventoryItem>();
        }

        // Initialise inventory list capacity array
        inventoryListCapacityIntArray = new int[(int)InventoryLocation.count];

        // Initialise player inventory list capacity
        inventoryListCapacityIntArray[(int)InventoryLocation.player] = Settings.playerInitialInventoryCapacity;
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
        List<InventoryItem> inventoryList = inventoryLists[(int)inventoryLocation];

        // Check if inventory already contains item
        int itemPosition = FindItemInInventory(inventoryLocation, itemCode);

        if(itemPosition != -1)
        {
            AddItemAtPosition(inventoryList, itemCode, itemPosition);
        }
        else
        {
            AddItemAtPosition(inventoryList, itemCode);
        }

        // Send event that inventory has been updated
        EventHandler.CallInventoryUpdatedEvent(inventoryLocation, inventoryLists[(int)inventoryLocation]);
    }

    /// <summary>
    /// Add item to end of inventory
    /// </summary>
    private void AddItemAtPosition(List<InventoryItem> inventoryList, int itemCode)
    {
        InventoryItem inventoryItem = new InventoryItem();

        inventoryItem.itemCode = itemCode;
        inventoryItem.itemQuantity = 1;
        inventoryList.Add(inventoryItem);

      // DebugPrintInventoryList(inventoryList);
    }

    /// <summary>
    /// Add item to position in inventory
    /// </summary>
    private void AddItemAtPosition(List<InventoryItem> inventoryList, int itemCode, int position)
    {
        InventoryItem inventoryItem = new InventoryItem();

        int quantity = inventoryList[position].itemQuantity + 1;
        inventoryItem.itemCode = itemCode;
        inventoryItem.itemQuantity = quantity;
        inventoryList[position] = inventoryItem;

       // DebugPrintInventoryList(inventoryList);
    }

    /// <summary>
    /// Find if an itemCode is already in the inventory. 
    /// </summary>
    /// <returns>Either position of item or -1 if not found</returns>
    public int FindItemInInventory(InventoryLocation inventoryLocation, int itemCode)
    {
        List<InventoryItem> inventoryList = inventoryLists[(int)inventoryLocation];

        for(int i = 0; i < inventoryList.Count; i++)
        {
            if (inventoryList[i].itemCode == itemCode)
            {
                return i;
            }
        }

        return -1;
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
    /// Returns the itemDetails (From the SO_ItemList) for the itemCode, or null if the item code does not exist
    /// </summary>

    public ItemDetails GetItemDetails(int itemCode)
    {
        ItemDetails itemDetails;

        if(itemDetailsDictionary.TryGetValue(itemCode, out itemDetails))
        {
            return itemDetails;
        }
        else
        {
            return null;
        }
    }

    public void RemoveItem(InventoryLocation inventoryLocation, int itemCode)
    {
        List<InventoryItem> inventoryList = inventoryLists[(int)inventoryLocation];

        // Check if inventory already contains item
        int itemPosition = FindItemInInventory(inventoryLocation, itemCode);

        if(itemPosition != -1)
        {
            RemoveItemAtPosition(inventoryList, itemCode, itemPosition);
        }

        // Send event that inventory has been updated
        EventHandler.CallInventoryUpdatedEvent(inventoryLocation, inventoryLists[(int)inventoryLocation]);
    }

    private void RemoveItemAtPosition(List<InventoryItem> inventoryList, int itemCode, int position)
    {
        InventoryItem inventoryItem = new InventoryItem();

        int quantity = inventoryList[position].itemQuantity - 1;

        if(quantity > 0)
        {
            inventoryItem.itemQuantity = quantity;
            inventoryItem.itemCode = itemCode;
            inventoryList[position] = inventoryItem;
        }
        else
        {
            inventoryList.RemoveAt(position);
        }
    }

    //private void DebugPrintInventoryList(List<InventoryItem> inventoryList)
    //{
    //    foreach(InventoryItem inventoryItem in inventoryList)
    //    {
    //        Debug.Log("Item Description: " + InventoryManager.Instance.GetItemDetails(inventoryItem.itemCode).itemDescription + "      Item Quantity: " + inventoryItem.itemQuantity);
    //    }

    //    Debug.Log("******************************************************************");
    //}
}
