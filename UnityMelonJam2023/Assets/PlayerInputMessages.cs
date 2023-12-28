using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerInputMessages : MonoBehaviour
{
    private Movement2D _movement;
    private void Awake()
    {
        _movement = GetComponent<Movement2D>();
    }

    public void Move(InputAction.CallbackContext context)
    {
        _movement.MovePlayerInput = context.ReadValue<Vector2>();
    }

    public void Aim(InputAction.CallbackContext context)
    {
        if (context.control.device is Mouse)
        {
            _movement.AimDirPlayerInput = Utility.GetMousePos().ToVector3() - _movement.transform.position;
        }
        else
        {
            _movement.AimDirPlayerInput = context.ReadValue<Vector2>().normalized;
        }
    }
}
