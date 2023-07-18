using System.Collections.Generic;
using UnityEngine.SceneManagement;

public class SaveLoadManager : SingletonMonobehaviour<SaveLoadManager>
{
    public List<ISaveable> ISaveableObjectList;

    protected override void Awake()
    {
        base.Awake();

        ISaveableObjectList = new List<ISaveable>();
    }

    public void StoreCurrentSceneData()
    {
        // Loop through all ISaveable objects and trigger store scene data for each
        foreach (ISaveable iSaveableObject in ISaveableObjectList)
        {
            iSaveableObject.ISaveableStoreScene(SceneManager.GetActiveScene().name);
        }
    }

    public void RestoreCurrentSceneData()
    {
        // Loop through all ISaveable objects and trigger restore scene data for each
        foreach(ISaveable iSaveableObject in ISaveableObjectList)
        {
            iSaveableObject.ISaveableRestoreScene(SceneManager.GetActiveScene().name);
        }
    }
}
