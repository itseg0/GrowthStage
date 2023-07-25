using UnityEngine;

public class ItemPickup : MonoBehaviour
{
    private void OnTriggerEnter2D(Collider2D collision)
    {
        Item item = collision.GetComponent<Item>();

        if (item != null)
        {
            // Get item details
            ItemDetails itemDetails = InventoryManager.Instance.GetItemDetails(item.ItemCode);

            if (itemDetails.canBePickedUp)
            {
                // Add the item to the player's inventory
                InventoryManager.Instance.AddItem(InventoryLocation.player, item, collision.gameObject);

                // Remove the item from the itemTileMap using its unique identifier
                UIInventorySlot.RemoveItemFromTileMap(item.UniqueIdentifier);

                // Debug all dictionary entries in UIInventorySlot
                UIInventorySlot.DebugItemTileMap();
            }
        }
    }
}
