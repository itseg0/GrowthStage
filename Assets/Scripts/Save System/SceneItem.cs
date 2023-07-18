using UnityEngine;

[System.Serializable]
public class SceneItem
{
    public int itemCode;
    public Vector3Serializable position;
    public string itemName;
    public Sprite sprite;

    public SceneItem()
    {
        position = new Vector3Serializable();
    }
}
