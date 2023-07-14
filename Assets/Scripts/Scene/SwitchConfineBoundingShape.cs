using UnityEngine;
using Cinemachine;

public class SwitchConfineBoundingShape : MonoBehaviour
{
   private void OnEnable()
    {
        EventHandler.AfterSceneLoadEvent += SceneLoaded;
    }

    private void OnDisable()
    {
        EventHandler.AfterSceneLoadEvent -= SceneLoaded;
    }

    private void SceneLoaded()
    {
        SwitchBoundingShape();
    }

    /// <summary>
    /// Switch the collider that cinemachine uses to define the edges of the screen
    /// </summary>
    private void SwitchBoundingShape()
    {
        // Get polygon collider on the 'boundsconfiner' gameObject which is used by Cinemachine to prevent the camera going beyond the screen edges
        PolygonCollider2D polygonCollider2D = GameObject.FindGameObjectWithTag(Tags.BoundsConfiner).GetComponent<PolygonCollider2D>();

        CinemachineConfiner2D cinemachineConfiner2D = GetComponent<CinemachineConfiner2D>();

        cinemachineConfiner2D.m_BoundingShape2D = polygonCollider2D;

        // Since the confiner bounds have now changed we need to clear cache

        cinemachineConfiner2D.InvalidateCache();
    }
}
