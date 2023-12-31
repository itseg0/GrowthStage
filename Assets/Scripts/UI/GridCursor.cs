using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GridCursor : MonoBehaviour
{
    private Canvas canvas;
    private Grid grid;
    private Camera mainCamera;
    private UIInventorySlot uiInventorySlot;
    [SerializeField] private Image cursorImage = null;
    [SerializeField] private RectTransform cursorRectTransform = null;
    [SerializeField] private Sprite greenCursorSprite = null;
    [SerializeField] private Sprite redCursorSprite = null;

    private bool _cursorPositionIsValid = false;
    public bool CursorPositionIsValid { get => _cursorPositionIsValid; set => _cursorPositionIsValid = value; }

    private int _itemUseGridRadius = 0;
    public int ItemUseGridRadius { get => _itemUseGridRadius; set => _itemUseGridRadius = value; }

    private ItemType _selectedItemType;
    public ItemType SelectedItemType { get => _selectedItemType; set => _selectedItemType = value; }

    private bool _cursorIsEnabled = false;
    public bool CursorIsEnabled { get => _cursorIsEnabled; set => _cursorIsEnabled = value; }

    private void OnDisable()
    {
        EventHandler.AfterSceneLoadEvent -= SceneLoaded;
    }

    private void OnEnable()
    {
        EventHandler.AfterSceneLoadEvent += SceneLoaded;
    }

    // Start is called before the first frame update
    private void Start()
    {
        mainCamera = Camera.main;
        canvas = GetComponentInParent<Canvas>();

    }

    public void Initialise(UIInventorySlot inventorySlot)
    {
        uiInventorySlot = inventorySlot;
    }

    // Update is called once per frame
    private void Update()
    {
        if (CursorIsEnabled)
        {
            DisplayCursor();
        }
    }

    private Vector3Int DisplayCursor()
    {
        if (grid != null)
        {
            // Get grid position for cursor
            Vector3Int gridPosition = GetGridPositionForCursor();

            // Get grid position for player
            Vector3Int playerGridPosition = GetGridPositionForPlayer();

            // Debug.Log("Is Tile Occupied: " + IsItemOccupiedAtGridPosition(gridPosition, 10003));

            // Set cursor validity based on item occupancy
            SetCursorValidity(gridPosition, playerGridPosition);

            // Get rect transform position for cursor
            cursorRectTransform.position = GetRectTransformPositionForCursor(gridPosition);

            return gridPosition;
        }
        else
        {
            return Vector3Int.zero;
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


    private void SceneLoaded()
    {
        grid = GameObject.FindObjectOfType<Grid>();
    }

    private void SetCursorValidity(Vector3Int cursorGridPosition, Vector3Int playerGridPosition)
    {

        SetCursorToValid();

        // Check item use radius is valid
        if (Mathf.Abs(cursorGridPosition.x - playerGridPosition.x) > ItemUseGridRadius
            || Mathf.Abs(cursorGridPosition.y - playerGridPosition.y) > ItemUseGridRadius)
        {
            SetCursorToInvalid();
            return;
        }

        // Get selected item details
        ItemDetails itemDetails = InventoryManager.Instance.GetSelectedInventoryItemDetails(InventoryLocation.player);

        if (itemDetails == null)
        {
            SetCursorToInvalid();
            return;
        }

        // Get grid property details at cursor position
        GridPropertyDetails gridPropertyDetails = GridPropertiesManager.Instance.GetGridPropertyDetails(cursorGridPosition.x, cursorGridPosition.y);

        if (gridPropertyDetails != null)
        {
            // Determine cursor validity based on inventory item selected and grid property details
            switch (itemDetails.itemType)
            {
                case ItemType.Seed:
                    if (!IsCursorValidForSeed(gridPropertyDetails) && !IsItemOccupiedAtGridPosition(cursorGridPosition, 10003))
                    {
                        SetCursorToInvalid();
                        return;
                    }
                    break;

                case ItemType.Commodity:
                    if (!IsCursorValidForCommodity(gridPropertyDetails))
                    {
                        SetCursorToInvalid();
                        return;
                    }
                    break;

                case ItemType.Furniture:
                    if (!IsCursorValidForFurniture(gridPropertyDetails))
                    {
                        SetCursorToInvalid();
                        return;
                    }
                    break;

                case ItemType.count:
                    break;

                default:
                    break;
            }
        }
        else
        {
            SetCursorToInvalid();
            return;
        }

        // Check item occupancy
        if (SelectedItemType == ItemType.Seed)
        {
            if (IsItemOccupiedAtGridPosition(cursorGridPosition, 10003))
            {
                SetCursorToValid(); // Cursor is valid if it is a Seed and placed above an item with code 10003.
            }
            else
            {
                SetCursorToInvalid(); // Cursor is invalid if it is a Seed but not placed above an item with code 10003.
            }
        }
        else if (IsItemOccupiedAtGridPosition(cursorGridPosition))
        {
            SetCursorToInvalid(); // Cursor is invalid if it's not a Seed but placed above any item.
        }
        else
        {
            SetCursorToValid(); // Cursor is valid if it's not a Seed and not placed above any item.
        }

    }



    /// <summary>
    /// Set the cursor to be invalid
    /// </summary>
    private void SetCursorToInvalid()
    {
        cursorImage.sprite = redCursorSprite;
        CursorPositionIsValid = false;
    }

    /// <summary>
    /// Set the cursor to be valid
    /// </summary>
    private void SetCursorToValid()
    {
        cursorImage.sprite = greenCursorSprite;
        CursorPositionIsValid = true;
    }

    /// <summary>
    /// Test cursor validity for a commodity for the target gridPropertyDetails. Returns true if valid, false if invalid
    /// </summary>
    private bool IsCursorValidForCommodity(GridPropertyDetails gridPropertyDetails)
    {
        return gridPropertyDetails.canDropItem;

    }

    /// <summary>
    /// Set cursor validity for a seed for the target gridPropertyDetails. Returns true if valid, false if invalid
    /// </summary>
    private bool IsCursorValidForSeed(GridPropertyDetails gridPropertyDetails)
    {
        return gridPropertyDetails.canDropItem;

    }

    private bool IsCursorValidForFurniture(GridPropertyDetails gridPropertyDetails)
    {
        return gridPropertyDetails.canPlaceFurniture;
    }

    public void DisableCursor()
    {
        cursorImage.color = Color.clear;

        CursorIsEnabled = false;
    }

    public void EnableCursor()
    {
        cursorImage.color = new Color(1f, 1f, 1f, 1f);
        CursorIsEnabled = true;
    }

    public Vector3Int GetGridPositionForCursor()
    {
        Vector3 worldPosition = mainCamera.ScreenToWorldPoint(new Vector3(Input.mousePosition.x, Input.mousePosition.y, -mainCamera.transform.position.z));  // z is how far the objects are in front of the camera - camera is at -10 so objects are (-)-10 in front = 10
        return grid.WorldToCell(worldPosition);
    }

    public Vector3Int GetGridPositionForPlayer()
    {
        return grid.WorldToCell(Player.Instance.transform.position);
    }

    public Vector2 GetRectTransformPositionForCursor(Vector3Int gridPosition)
    {
        Vector3 gridWorldPosition = grid.CellToWorld(gridPosition);
        Vector2 gridScreenPosition = mainCamera.WorldToScreenPoint(gridWorldPosition);
        return RectTransformUtility.PixelAdjustPoint(gridScreenPosition, cursorRectTransform, canvas);
    }

}
