using UnityEngine;

[System.Serializable]
public class CropDetails
{
    [ItemCodeDescription]
    [Tooltip("item code for the corresponding seed")] public int seedItemCode;  // this is the item code for the corresponding seed
    [Tooltip("days growth for each stage, 1 in Element 0 is no growth")] public int[] growthDays; // days growth for each stage
    [Tooltip("total growth days")] public int totalGrowthDays; // total growth days
    [Tooltip("prefab for every growth stage which is entered in Growth Days")] public GameObject[] growthPrefab;// prefab to use when instantiating growth stages
    [Tooltip("sprite for every growth stage which is entered in Growth Days")] public Sprite[] growthSprite; // growth sprite
    [Tooltip("growth only in this seasons")] public Season[] seasons; // growth seasons
    [Tooltip("sprite used once harvested")] public Sprite harvestedSprite; // sprite used once harvested
    [ItemCodeDescription]
    [Tooltip("if crop transforms in an other item after harvested, this is the code for the new item")] public int harvestedTransformItemCode; // if the item transforms into another item when harvested this item code will be populated
    [Tooltip("disable the crop before harvesting")] public bool hideCropBeforeHarvestedAnimation; // if the crop should be disabled before the harvested animation
    [Tooltip("if colliders on crop should be disabled to avoid the harvested animation effecting any other game objects")] public bool disableCropCollidersBeforeHarvestedAnimation; // if colliders on crop should be disabled to avoid the harvested animation effecting any other game objects
    [Tooltip("plays animation if harvested at the final growth stage")] public bool isHarvestedAnimation; // true if harvested animation to be played on final growth stage prefab
    [Tooltip("if there is an action effect")] public bool isHarvestActionEffect = false; // flag to determine whether there is a harvest action effect
    [Tooltip("spawn at players position")] public bool spawnCropProducedAtPlayerPosition;
    // [Tooltip("action effect for the crop")] public HarvestActionEffect harvestActionEffect; // the harvest action effect for the crop

    [ItemCodeDescription]
    [Tooltip("tool item codes for harvesting, 0 if no tool required")] public int[] harvestToolItemCode; // array of item codes for the tools that can harvest or 0 array elements if no tool required
    [Tooltip("number of harvest actions required")] public int[] requiredHarvestActions; // number of harvest actions required for corressponding tool in harvest tool item code array
    [ItemCodeDescription]
    [Tooltip("items produced for harvesting crop")] public int[] cropProducedItemCode; // array of item codes produced for the harvested crop
    [Tooltip("min quantities produced for the harvested crop")] public int[] cropProducedMinQuantity; // array of minimum quantities produced for the harvested crop
    [Tooltip("max quantities produced for the harvested crop")] public int[] cropProducedMaxQuantity; // if max quantity is > min quantity then a random number of crops between min and max are produced
    [Tooltip("days to regrow next crop or -1 if a single crop")] public int daysToRegrow; // days to regrow next crop or -1 if a single crop

    public bool CanUseToolToHarvestCrop(int toolItemCode)
    {
        if(RequiredHarvestActionForTool(toolItemCode) == -1)
        {
            return false;
        }
        else
        {
            return true;
        }
    }

    public int RequiredHarvestActionForTool(int toolItemCode)
    {
        for(int i = 0; i < harvestToolItemCode.Length; i++)
        {
            if (harvestToolItemCode[i] == toolItemCode)
            {
                return requiredHarvestActions[i];
            }
        }
        return -1;
    }
}
