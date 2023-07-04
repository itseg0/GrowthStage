using UnityEngine;

public class Player : SingletonMonobehaviour<Player>
{
    private Rigidbody2D rb;
    private Camera mainCamera;


    protected override void Awake()
    {
        base.Awake();

        rb = GetComponent<Rigidbody2D>();

        // Get reference to main camera, intensive operation so cache it
        mainCamera = Camera.main;
    }


    public Vector3 GetPlayerViewportPosition()
    {
        // Vector3 viewport position for player (0,0) viewport bottom left (1,1) would be top right, etc.
        return mainCamera.WorldToViewportPoint(transform.position);
    }
}
