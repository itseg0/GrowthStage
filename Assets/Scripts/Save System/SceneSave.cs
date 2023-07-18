using System.Collections.Generic;

[System.Serializable]
public class SceneSave
{
    // String key is an indentifier name we choose for the list
    public Dictionary<string, List<SceneItem>> listSceneItemDictionary;
}
