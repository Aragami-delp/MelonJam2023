using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerInputMessages : MonoBehaviour
{
    private Movement2D _movement;
    private Camera _camera;

    private void Awake()
    {
        _movement = GetComponent<Movement2D>();
        _camera = Camera.main;
    }

    public void Move(InputAction.CallbackContext context)
    {
        _movement.MovePlayerInput = context.ReadValue<Vector2>();
    }

    public void Aim(InputAction.CallbackContext context)
    {
        if (context.control.device is Mouse)
        {
            _movement.AimDirPlayerInput = Utility.GetMousePos3(_camera) - _movement.transform.position;
        }
        else
        {
            _movement.AimDirPlayerInput = context.ReadValue<Vector2>().normalized;
        }
    }
}
