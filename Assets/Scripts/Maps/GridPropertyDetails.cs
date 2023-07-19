[System.Serializable]
public class GridPropertyDetails
{
    public int gridX;
    public int gridY;
    public bool canDropItem = false;
    public bool canPlaceFurniture = false;
    public bool isPath = false;
    public bool isNPCObstacle;
    public int daysSinceWatered;
    public int seedItemCode;
    public int growthDays;
    public int daysSinceLastHarvest;

    public GridPropertyDetails()
    {

    }
}
