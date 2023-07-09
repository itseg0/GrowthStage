using System;
using System.Collections.Generic;

public static class EventHandler
{
    // Inventory Update Event
    public static event Action<InventoryLocation, Dictionary<int, InventoryItem>> InventoryUpdatedEvent;

    public static void CallInventoryUpdatedEvent(InventoryLocation inventoryLocation, Dictionary<int, InventoryItem> inventoryDict)
    {
        if (InventoryUpdatedEvent != null)
        {
            InventoryUpdatedEvent(inventoryLocation, inventoryDict);
        }
    }
}