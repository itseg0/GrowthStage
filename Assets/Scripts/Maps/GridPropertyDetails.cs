using UnityEngine;

[System.Serializable]
public class GridPropertyDetails
{
    public int gridX;
    public int gridY;
    public bool canDropItem = false;
    public bool canPlaceFurniture = false;
    public bool isPath = false;
    public bool isNPCObstacle;
    public int daysSinceWatered = -1;
    public int seedItemCode = -1;
    public int growthDays = -1 ;
    public int daysSinceLastHarvest = -1;
    public bool hasItem;
    public bool hasFurniture;

    public GridPropertyDetails()
    {

    }
}
