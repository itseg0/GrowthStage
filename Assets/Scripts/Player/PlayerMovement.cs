using System;
using System.Collections;
using System.Collections.Generic;
using System.Xml;
using UnityEngine;

public class PlayerMovement : SingletonMonobehaviour<PlayerMovement>
{
    public float speed;
    public Animator animator;
    
    public bool PlayerInputIsDisabled { get => _playerInputIsDisabled; set => _playerInputIsDisabled = value; }

    private Vector2 lastMovementDirection = Vector2.down;
    private bool _playerInputIsDisabled = false;



    void Update()
    {

        if (!PlayerInputIsDisabled)
        {
            float horizontal = Input.GetAxisRaw("Horizontal");
            float vertical = Input.GetAxisRaw("Vertical");

            Vector3 direction = new Vector2(horizontal, vertical).normalized;

            if (direction.magnitude > 0)
            {
                lastMovementDirection = direction;
                AnimateMovement(direction);
            }
            else
            {
                AnimateIdle();
            }

            transform.position += direction * speed * Time.deltaTime;
        }

    }

    void AnimateMovement(Vector3 direction)
    {
        if (animator != null)
        {
            if(direction.magnitude > 0)
            {
                animator.SetBool("isMoving", true);
                animator.SetFloat("horizontal", direction.x);
                animator.SetFloat("vertical", direction.y);
            }
            else
            {
                animator.SetBool("isMoving", false);
            }
        }
    }

    void AnimateIdle()
    {
        if(animator != null)
        {
            animator.SetBool("isMoving", false);
            animator.SetFloat("horizontal", lastMovementDirection.x);
            animator.SetFloat("vertical", lastMovementDirection.y);
        }
    }

    public void EnablePlayerInput()
    {
        PlayerInputIsDisabled = false;
    }

    public void DisablePlayerInput()
    {
        PlayerInputIsDisabled = true;
        AnimateIdle();
    }
}
