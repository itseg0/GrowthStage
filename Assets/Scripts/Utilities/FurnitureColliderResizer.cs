using UnityEngine;

public class FurnitureColliderResizer : MonoBehaviour
{
    private SpriteRenderer spriteRenderer;
    private BoxCollider2D boxCollider;

    private void Awake()
    {
        spriteRenderer = GetComponentInChildren<SpriteRenderer>();
        boxCollider = GetComponent<BoxCollider2D>();

        if (spriteRenderer == null)
        {
            Debug.Log("We require a sprite");
            return;
        }
        if (boxCollider == null)
        {
            Debug.Log("We require a box collider");
            return;
        }

        // Use a coroutine to delay the ResizeCollider method slightly
        StartCoroutine(DelayedResizeCollider());
    }

    private System.Collections.IEnumerator DelayedResizeCollider()
    {
        yield return null; // Wait for one frame

        ResizeCollider();
    }

    public void ResizeCollider()
    {
        Vector2 spriteSize = spriteRenderer.sprite.bounds.size;
        boxCollider.size = spriteSize;
    }
}
