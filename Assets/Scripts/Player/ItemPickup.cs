using UnityEngine;
using System.Collections.Generic;

public class ItemPickup : MonoBehaviour
{
    public static ItemPickup Instance { get; private set; }

    // This will store the itemTileMap for each scene
    public Dictionary<string, Dictionary<string, Vector3Int>> sceneItemTileMaps = new Dictionary<string, Dictionary<string, Vector3Int>>();

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    // Add items to the itemTileMap of a specific scene
    public void AddItemToSceneTileMap(string sceneName, string uniqueIdentifier, Vector3Int gridPosition)
    {
        if (!sceneItemTileMaps.ContainsKey(sceneName))
        {
            sceneItemTileMaps[sceneName] = new Dictionary<string, Vector3Int>();
        }
        sceneItemTileMaps[sceneName].Add(uniqueIdentifier, gridPosition);
    }

    // Remove items from the itemTileMap of a specific scene
    public void RemoveItemFromSceneTileMap(string sceneName, string uniqueIdentifier)
    {
        if (sceneItemTileMaps.ContainsKey(sceneName))
        {
            sceneItemTileMaps[sceneName].Remove(uniqueIdentifier);
        }
    }

    public void DebugSceneItemTileMap(string sceneName)
    {
        if (sceneItemTileMaps.TryGetValue(sceneName, out Dictionary<string, Vector3Int> sceneItemTileMap))
        {
            Debug.Log($"ItemTileMap for scene {sceneName}:");
            foreach (var entry in sceneItemTileMap)
            {
                Debug.Log("Key: " + entry.Key + ", Value: " + entry.Value);
            }
        }
        else
        {
            Debug.LogWarning($"Scene {sceneName} not found in sceneItemTileMaps.");
        }
    }

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

                // Get the current scene name
                string currentSceneName = UnityEngine.SceneManagement.SceneManager.GetActiveScene().name;

                // Remove the item from the itemTileMap using its unique identifier for the current scene
                RemoveItemFromSceneTileMap(currentSceneName, item.UniqueIdentifier.ToString());

                // Debug all dictionary entries in the current scene's itemTileMap
                DebugSceneItemTileMap(currentSceneName);
            }
        }
    }
}
