using UnityEngine;
using TMPro;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System;
using Unity.Mathematics;
using System.Collections.Generic;
using UnityEngine.UIElements;
using static UnityEditor.Progress;
using UnityEngine.SceneManagement;

public class UIInventorySlot : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler, IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler
{
    private Camera mainCamera;
    private Transform parentItem;
    private GameObject draggedItem;
    private PlayerMovement playerMovement;
    private GameObject player;
    private Canvas parentCanvas;
    private GridCursor gridCursor;

    public UnityEngine.UI.Image inventorySlotHighlight;
    public UnityEngine.UI.Image inventorySlotImage;
    public TextMeshProUGUI textMeshProUGUI;
    public bool isTileOccupied = false;

    [SerializeField] private UIInventoryBar inventoryBar = null;
    [SerializeField] private GameObject itemPrefab;
    [SerializeField] private GameObject furniturePrefab1x1;
    [SerializeField] private int slotNumber = 0;
    [SerializeField] private GameObject inventoryTextBoxPrefab;

    [HideInInspector] public ItemDetails itemDetails;
    [HideInInspector] public int itemQuantity;
    [HideInInspector] public bool isSelected = false;

    public static Dictionary<Guid, Vector3Int> staticItemTileMap = new Dictionary<Guid, Vector3Int>();
    public static List<UIInventorySlot> allSlots = new List<UIInventorySlot>();

    private void Awake()
    {
        parentCanvas = GetComponentInParent<Canvas>();
        allSlots.Add(this);


    }



    private void Start()
    {
        mainCamera = Camera.main;
        player = GameObject.Find("Player");
        playerMovement = player.GetComponent<PlayerMovement>();

        gridCursor = FindObjectOfType<GridCursor>();

        // Initialize the GridCursor with this UIInventorySlot instance
        if (gridCursor != null)
        {
            gridCursor.Initialise(this);
        }
    }

    private bool IsItemOccupiedAtGridPosition(Vector3Int gridPosition)
    {
        return IsItemOccupiedAtGridPosition(gridPosition, -1); // Default item code to -1 when not specified
    }

    private bool IsItemOccupiedAtGridPosition(Vector3Int gridPosition, int itemCode)
    {
        Dictionary<string, (Vector3Int gridPosition, int itemCode)> sceneItemTileMap = GetSceneItemTileMap();

        if (sceneItemTileMap != null)
        {
            // Check if the grid position exists in the current scene's itemTileMap and has the specified item code
            foreach (var entry in sceneItemTileMap)
            {
                // Get the item code for the item at the grid position
                int itemCodeAtPosition = entry.Value.itemCode;
                if (itemCode == -1 || itemCodeAtPosition == itemCode) // If itemCode is -1, we ignore the item code check
                {
                    if (entry.Value.gridPosition == gridPosition)
                    {
                        return true;
                    }
                }
            }
        }

        return false;
    }

    private Dictionary<string, (Vector3Int gridPosition, int itemCode)> GetSceneItemTileMap()
    {
        ItemPickup itemPickup = FindObjectOfType<ItemPickup>();

        if (itemPickup == null)
        {
            Debug.LogError("ItemPickup not found in the scene.");
            return null;
        }

        // Get the current scene name
        string currentSceneName = UnityEngine.SceneManagement.SceneManager.GetActiveScene().name;

        // Check if the current scene exists in the sceneItemTileMaps dictionary
        if (itemPickup.sceneItemTileMaps.TryGetValue(currentSceneName, out Dictionary<string, (Vector3Int gridPosition, int itemCode)> sceneItemTileMap))
        {
            return sceneItemTileMap;
        }

        return null;
    }

    private void OnEnable()
    {
        EventHandler.AfterSceneLoadEvent += SceneLoaded;
        EventHandler.RemoveSelectedItemFromInventoryEvent += RemoveSelectedItemFromInventory;
        EventHandler.DropSelectedItemEvent += DropSelectedItemAtMousePosition;

    }

    private void OnDisable()
    {
        EventHandler.AfterSceneLoadEvent -= SceneLoaded;
        EventHandler.RemoveSelectedItemFromInventoryEvent -= RemoveSelectedItemFromInventory;
        EventHandler.DropSelectedItemEvent -= DropSelectedItemAtMousePosition;
    }

    private void ClearCursors()
    {
        gridCursor.DisableCursor();

        // gridCursor.SelectedItemType = ItemType.none;
    }

    /// <summary>
    /// Sets this inventory slot item to be selected 
    /// </summary>
    private void SetSelectedItem()
    {
        // Clear currently highlighted items
        inventoryBar.ClearHighlightOnInventorySlots();

        // Highlight item on inventory bar
        isSelected = true;

        // Set highlighted inventory slots
        inventoryBar.SetHighlightedInventorySlots();

        // Set use radius for cursors
        gridCursor.ItemUseGridRadius = itemDetails.itemUseGridRadius;

        if(itemDetails.itemUseGridRadius > 0)
        {
            gridCursor.EnableCursor();
        }
        else
        {
            gridCursor.DisableCursor();
        }

        // Set item type
        gridCursor.SelectedItemType = itemDetails.itemType;

        // Set item selected in inventory
        InventoryManager.Instance.SetSelectedInventoryItem(InventoryLocation.player, itemDetails.itemCode);
    }

    private void ClearSelectedItem()
    {
        ClearCursors();

        // Clear currently highlighted items
        inventoryBar.ClearHighlightOnInventorySlots();

        isSelected = false;

        // Set no item selected
        InventoryManager.Instance.ClearSelectedInventoryItem(InventoryLocation.player);
    }



    /// <summary>
    /// Drops the item at the current mouse position.
    /// </summary>

    private void DropSelectedItemAtMousePosition()
    {
        if (itemDetails != null && isSelected)
        {
            Vector3 worldPosition = mainCamera.ScreenToWorldPoint(new Vector3(Input.mousePosition.x, Input.mousePosition.y, -mainCamera.transform.position.z));

            Vector3Int gridPosition = GridPropertiesManager.Instance.grid.WorldToCell(worldPosition);
            Vector3 tileCenterWorldPosition = GridPropertiesManager.Instance.grid.GetCellCenterWorld(gridPosition);

            GridPropertyDetails gridPropertyDetails = GridPropertiesManager.Instance.GetGridPropertyDetails(gridPosition.x, gridPosition.y);

            if (gridCursor.CursorPositionIsValid)
            {
                // Center the item on the tile
                float itemHalfWidth = itemPrefab.GetComponentInChildren<SpriteRenderer>().bounds.extents.x;
                float itemHalfHeight = itemPrefab.GetComponentInChildren<SpriteRenderer>().bounds.extents.y;
                Vector3 centeredPosition = new Vector3(tileCenterWorldPosition.x, tileCenterWorldPosition.y, 0f);
                centeredPosition.x += itemHalfWidth;
                centeredPosition.y += itemHalfHeight;


                // Create item from prefab at centered position
                GameObject itemGameObject = Instantiate(itemPrefab, centeredPosition, Quaternion.identity, parentItem);
                Item item = itemGameObject.GetComponent<Item>();
                item.ItemCode = itemDetails.itemCode;
                SpriteRenderer sr = itemGameObject.GetComponentInChildren<SpriteRenderer>();
                sr.sprite = itemDetails.itemSprite;

                Debug.Log(item.ItemCode);

                item.UniqueIdentifier = Guid.NewGuid();

                // Remove item from player's inventory
                InventoryManager.Instance.RemoveItem(InventoryLocation.player, item.ItemCode);

                // If there's no more quantity, clear selected
                if (InventoryManager.Instance.FindItemInInventory(InventoryLocation.player, item.ItemCode) == -1)
                {
                    ClearSelectedItem();
                }

                // Use ItemPickup script to add item to the tile map
                ItemPickup.Instance.AddItemToSceneTileMap(SceneManager.GetActiveScene().name, item.UniqueIdentifier.ToString(), gridPosition, item.ItemCode);

                // Debug the tile map from ItemPickup script
                ItemPickup.Instance.DebugSceneItemTileMap(SceneManager.GetActiveScene().name);

                // Remove item from player's inventory
                InventoryManager.Instance.RemoveItem(InventoryLocation.player, item.ItemCode);
            }
        }
        
    }

    private void DropSelectedFurnitureAtMousePosition()
    {
        if (itemDetails != null && isSelected && gridCursor.CursorPositionIsValid)
        {
            Vector3 worldPosition = mainCamera.ScreenToWorldPoint(new Vector3(Input.mousePosition.x, Input.mousePosition.y, -mainCamera.transform.position.z));
            Vector3Int gridPosition = GridPropertiesManager.Instance.grid.WorldToCell(worldPosition);
            Vector3 tileCenterWorldPosition = GridPropertiesManager.Instance.grid.GetCellCenterWorld(gridPosition);

            // Center the furniture on the tile
            float itemHalfWidth = furniturePrefab1x1.GetComponentInChildren<SpriteRenderer>().bounds.extents.x;
            float itemHalfHeight = furniturePrefab1x1.GetComponentInChildren<SpriteRenderer>().bounds.extents.y;
            Vector3 centeredPosition = new Vector3(tileCenterWorldPosition.x + itemHalfWidth, tileCenterWorldPosition.y + itemHalfHeight, 0f);

            // Instantiate the furniture prefab at the centered position and add it to the parentItem transform.
            GameObject furnitureGameObject = Instantiate(furniturePrefab1x1, centeredPosition, Quaternion.identity, parentItem);
            Item furnitureItem = furnitureGameObject.GetComponent<Item>();
            furnitureItem.ItemCode = itemDetails.itemCode;
            SpriteRenderer furnitureSr = furnitureGameObject.GetComponentInChildren<SpriteRenderer>();
            furnitureSr.sprite = itemDetails.itemSprite;

            Debug.Log(furnitureItem.ItemCode);

            FurnitureColliderResizer colliderResizer = furnitureGameObject.GetComponent<FurnitureColliderResizer>();

            if(colliderResizer != null)
            {
                colliderResizer.ResizeCollider();
                Debug.Log("Resized Collider");
            }

            furnitureItem.UniqueIdentifier = Guid.NewGuid();

            // Remove the furniture item from the player's inventory
            InventoryManager.Instance.RemoveItem(InventoryLocation.player, furnitureItem.ItemCode);

            // Use ItemPickup script to add item to the tile map
            ItemPickup.Instance.AddItemToSceneTileMap(SceneManager.GetActiveScene().name, furnitureItem.UniqueIdentifier.ToString(), gridPosition, furnitureItem.ItemCode);

            // Debug the tile map from ItemPickup script
            ItemPickup.Instance.DebugSceneItemTileMap(SceneManager.GetActiveScene().name);

            if (InventoryManager.Instance.FindItemInInventory(InventoryLocation.player, furnitureItem.ItemCode) == -1)
            {
                ClearSelectedItem();
            }
        }
    }

    private void RemoveSelectedItemFromInventory()
    {
        if(itemDetails != null && isSelected)
        {
            int itemCode = itemDetails.itemCode;

            InventoryManager.Instance.RemoveItem(InventoryLocation.player, itemCode);

            if(InventoryManager.Instance.FindItemInInventory(InventoryLocation.player, itemCode) == -1)
            {
                ClearSelectedItem();
            }
        }
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        if (itemDetails != null)
        {
            // Destroy existing dragged item if present
            if (draggedItem != null)
            {
                Destroy(draggedItem);
            }

            // Disable player input
            playerMovement.DisablePlayerInput();

            // Instantiate gameObject as dragged item
            draggedItem = Instantiate(inventoryBar.inventoryBarDraggedItem, inventoryBar.transform);

            // Get image for dragged item
            UnityEngine.UI.Image draggedItemImage = draggedItem.GetComponentInChildren<UnityEngine.UI.Image>();
            draggedItemImage.sprite = inventorySlotImage.sprite;

            // Select item on drag
            SetSelectedItem();
        }
    }

    public void OnDrag(PointerEventData eventData)
    {
        // Move gameObject as dragged item
        if(draggedItem != null)
        {
            draggedItem.transform.position = Input.mousePosition;
        }
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        // Destroy gameObject as dragged item
        if(draggedItem != null)
        {
            Destroy(draggedItem);

            // If drag ends over inventory bar, get item drag is over and swap item
            if(eventData.pointerCurrentRaycast.gameObject != null && eventData.pointerCurrentRaycast.gameObject.GetComponent<UIInventorySlot>() != null)
            {
                // Get slot number where drag ended
                int toSlotNumber = eventData.pointerCurrentRaycast.gameObject.GetComponent<UIInventorySlot>().slotNumber;

                // Swap inventory items in the inventory list
                InventoryManager.Instance.SwapInventoryItems(InventoryLocation.player, slotNumber, toSlotNumber);

                // Destroy text box
                DestroyInventoryTextBox();

                // Clear selected item
                ClearSelectedItem();

                
            }
            // Else attempt to drop the item if it can be dropped
            else
            {
                if(itemDetails.canBeDropped || itemDetails.canBePlanted)
                {
                    Vector3Int cursorGridPosition = gridCursor.GetGridPositionForCursor();

                    if (itemDetails.isFurniture)
                    {
                        DropSelectedFurnitureAtMousePosition();
                    }

                    else if (itemDetails.canBePlanted && IsItemOccupiedAtGridPosition(cursorGridPosition, 10003))
                    {
                        Vector3Int gridPosition = gridCursor.GetGridPositionForCursor();

                        GridPropertyDetails gridPropertyDetails = GridPropertiesManager.Instance.GetGridPropertyDetails(gridPosition.x, gridPosition.y);
                        PlayerMovement.Instance.ProcessPlayerClickInputSeed(gridPropertyDetails, itemDetails);
                    }

                    else
                    {
                        DropSelectedItemAtMousePosition();
                    }

                    if(itemQuantity <= 1)
                    ClearSelectedItem();
                }
            }
            // Re-Enable player input
            playerMovement.EnablePlayerInput();
        }
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        // Populate text box with item details
        if(itemQuantity != 0)
        {
            // Instantiate inventory text box
            inventoryBar.inventoryTextBoxGameObject = Instantiate(inventoryTextBoxPrefab, transform.position, Quaternion.identity);
            inventoryBar.inventoryTextBoxGameObject.transform.SetParent(parentCanvas.transform, false);

            UIInventoryTextBox inventoryTextBox = inventoryBar.inventoryTextBoxGameObject.GetComponent<UIInventoryTextBox>();

            // Set item type description
            string itemTypeDescription = InventoryManager.Instance.GetItemDescription(itemDetails.itemType);

            // Populate text box
            inventoryTextBox.SetTextboxText(itemDetails.itemDescription, itemTypeDescription, "", itemDetails.itemLongDescription, "", "");

            // Set text bow position according to inventory bar position
            if(inventoryBar.isInventoryBarPositionBottom)
            {
                inventoryBar.inventoryTextBoxGameObject.GetComponent<RectTransform>().pivot = new Vector2(0.5f, 0f);
                inventoryBar.inventoryTextBoxGameObject.transform.position = new Vector3(transform.position.x, transform.position.y + 50f, transform.position.z);
            }
            else
            {
                inventoryBar.inventoryTextBoxGameObject.GetComponent<RectTransform>().pivot = new Vector2(0.5f, 1f);
                inventoryBar.inventoryTextBoxGameObject.transform.position = new Vector3(transform.position.x, transform.position.y - 50f, transform.position.z);
            }
        }
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        DestroyInventoryTextBox();
    }

    public void DestroyInventoryTextBox()
    {
        if (inventoryBar != null && inventoryBar.inventoryTextBoxGameObject != null)
        {
            Destroy(inventoryBar.inventoryTextBoxGameObject);
        }
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        // If left click
        if(eventData.button == PointerEventData.InputButton.Left)
        {
            // If inventory slot currently selected, then deselect
            if(isSelected == true)
            {
                ClearSelectedItem();
            }
            else
            {
                if(itemQuantity >= 1)
                {
                    SetSelectedItem();
                }
            }
        }
    }

    public void SceneLoaded()
    {
        parentItem = GameObject.FindGameObjectWithTag(Tags.ItemsParentTransform).transform;
    }

    public static void RemoveItemFromTileMap(Guid uniqueIdentifier)
    {
        if (staticItemTileMap.ContainsKey(uniqueIdentifier))
        {
            staticItemTileMap.Remove(uniqueIdentifier);
            Debug.Log("Item removed from tile map with identifier: " + uniqueIdentifier);
        }
    }

    
}
