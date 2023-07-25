using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(GenerateGUID))]
public class GridPropertiesManager : SingletonMonobehaviour<GridPropertiesManager>, ISaveable
{
    [SerializeField] private SO_GridProperties[] so_gridPropertiesArray = null;

    [HideInInspector] public Grid grid;
    public string ISaveableUniqueID { get { return _iSaveableUniqueID; } set { _iSaveableUniqueID = value; } }
    public GameObjectSave GameObjectSave { get { return _gameObjectSave; } set { _gameObjectSave = value; } }

    private string _iSaveableUniqueID;
    private GameObjectSave _gameObjectSave;
    private Dictionary<string, GridPropertyDetails> gridPropertyDictionary;

    protected override void Awake()
    {
        base.Awake();

        ISaveableUniqueID = GetComponent<GenerateGUID>().GUID;
        GameObjectSave = new GameObjectSave();
    }

    private void OnEnable()
    {
        ISaveableRegister();
        EventHandler.AfterSceneLoadEvent += AfterSceneLoaded;
    }

    private void OnDisable()
    {
        ISaveableDeregister();
        EventHandler.AfterSceneLoadEvent -= AfterSceneLoaded;
    }

    private void Start()
    {
        InitialiseGridProperties();
    }

    /// <summary>
    /// Initialises the grid property dictionary with the values from the ScriptableObject assets and stores the values for each scene to the GameObjectSave sceneData
    /// </summary>
    private void InitialiseGridProperties()
    {
        // Loop through all grid properties in the array
        foreach (SO_GridProperties so_GridProperties in so_gridPropertiesArray)
        {
            // Create dictionary of grid property details
            Dictionary<string, GridPropertyDetails> gridPropertyDictionary = new Dictionary<string, GridPropertyDetails>();

            // Populate grid property dictionary - Iterate through all the grid properties in the so grid properties list
            foreach (GridProperty gridProperty in so_GridProperties.gridPropertyList)
            {
                GridPropertyDetails gridPropertyDetails;

                gridPropertyDetails = GetGridPropertyDetails(gridProperty.gridCoordinate.x, gridProperty.gridCoordinate.y, gridPropertyDictionary);

                if (gridPropertyDetails == null)
                {
                    gridPropertyDetails = new GridPropertyDetails();
                }

                switch (gridProperty.gridBoolProperty)
                {
                    case GridBoolProperty.canDropItem:
                        gridPropertyDetails.canDropItem = gridProperty.gridBoolValue;
                        break;

                    case GridBoolProperty.canPlaceFurniture:
                        gridPropertyDetails.canPlaceFurniture = gridProperty.gridBoolValue;
                        break;

                    case GridBoolProperty.isPath:
                        gridPropertyDetails.isPath = gridProperty.gridBoolValue;
                        break;

                    case GridBoolProperty.isNPCObstacle:
                        gridPropertyDetails.isNPCObstacle = gridProperty.gridBoolValue;
                        break;

                    default:
                        break;
                }

                // Set the initial values for hasItem and hasFurniture to false
                gridPropertyDetails.hasItem = false;
                gridPropertyDetails.hasFurniture = false;

                SetGridPropertyDetails(gridProperty.gridCoordinate.x, gridProperty.gridCoordinate.y, gridPropertyDetails, gridPropertyDictionary);
            }

            // Create scene save for this gameobject
            SceneSave sceneSave = new SceneSave();

            // Add grid property dictionary to scene save data
            sceneSave.gridPropertyDetailsDictionary = gridPropertyDictionary;

            // If starting scene set the gridPropertyDictionary member variable to the current iteration
            if (so_GridProperties.sceneName.ToString() == SceneControllerManager.Instance.startingSceneName.ToString())
            {
                this.gridPropertyDictionary = gridPropertyDictionary;
            }

            // Add scene save to game object scene data
            GameObjectSave.sceneData.Add(so_GridProperties.sceneName.ToString(), sceneSave);
        }
    }


    /// <summary>
    /// Set the grid property details in gridPropertyDetails for the tile at gridX, gridY for the gridpropertyDetails
    /// </summary>
    public void SetGridPropertyDetails(int gridX, int gridY, GridPropertyDetails gridPropertyDetails, Dictionary<string, GridPropertyDetails> gridPropertyDictionary)
    {
        string key = "x" + gridX + "y" + gridY;

        gridPropertyDetails.gridX = gridX;
        gridPropertyDetails.gridY = gridY;

        gridPropertyDictionary[key] = gridPropertyDetails;
    }


    public void SetGridPropertyDetails(int gridX, int gridY, GridPropertyDetails gridPropertyDetails)
    {
        SetGridPropertyDetails(gridX, gridY, gridPropertyDetails, gridPropertyDictionary);
    }

    /// <summary>
    /// Returns the gridPropertyDetails at the gridLocation for the supplied dictionary, or null if nothing exists at that exact location
    /// </summary>
    public GridPropertyDetails GetGridPropertyDetails(int gridX, int gridY, Dictionary<string, GridPropertyDetails> gridPropertyDictionary)
    {
        string key = "x" + gridX + "y" + gridY;

        GridPropertyDetails gridPropertyDetails;

        if(!gridPropertyDictionary.TryGetValue(key, out gridPropertyDetails))
        {
            return null;
        }
        else
        {
            return gridPropertyDetails;
        }
    }

    public GridPropertyDetails GetGridPropertyDetails(int gridX, int gridY)
    {
        return GetGridPropertyDetails(gridX, gridY, gridPropertyDictionary);
    }

    private void AfterSceneLoaded()
    {
        grid = GameObject.FindObjectOfType<Grid>();
    }

    public void ISaveableRegister()
    {
        SaveLoadManager.Instance.ISaveableObjectList.Add(this);
    }

    public void ISaveableDeregister()
    {
        SaveLoadManager.Instance.ISaveableObjectList.Remove(this);
    }

    public void ISaveableStoreScene(string sceneName)
    {
        // Remove sceneSave from scene
        GameObjectSave.sceneData.Remove(sceneName);

        // Create sceneSave for the scene
        SceneSave sceneSave = new SceneSave();

        // Create and add dictionary grid property dictionary
        sceneSave.gridPropertyDetailsDictionary = gridPropertyDictionary;

        // Add scene save to game object scene data
        GameObjectSave.sceneData.Add(sceneName, sceneSave);
    }

    public void ISaveableRestoreScene(string sceneName)
    {
        // Get sceneSave for the scene, it must exist because we created it in initialise
        if(GameObjectSave.sceneData.TryGetValue(sceneName, out SceneSave sceneSave))
        {
            // Get grid property details dictionary, it must exist because we created it in initialise
            if (sceneSave.gridPropertyDetailsDictionary != null)
            {
                gridPropertyDictionary = sceneSave.gridPropertyDetailsDictionary;
            }
        }
    }
}
