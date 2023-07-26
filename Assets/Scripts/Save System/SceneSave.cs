using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class SceneSave
{
    public List<SceneItem> listSceneItem;
    public Dictionary<string, Vector3Int> itemTileMap;
    public Dictionary<string, GridPropertyDetails> gridPropertyDetailsDictionary;
}