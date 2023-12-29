using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class Movement2D : MonoBehaviour
{
    [SerializeField] private float _moveSpeed = 6f;
    [SerializeField] private FieldOfView _fieldOfView;
    [SerializeField] private Transform _rotation;

    private Rigidbody2D _rigidbody;
    private Camera _viewCamera;
    private Vector2 _velocity;

    [HideInInspector] public Vector2 MovePlayerInput;
    [HideInInspector] public Vector2 AimDirPlayerInput;

    private void Start()
    {
        _rigidbody = GetComponent<Rigidbody2D>();
        _viewCamera = _viewCamera ?? Camera.main;

        this.AddComponent<PlayerInputMessages>();
    }

    private void Update()
    {
        Vector3 aimDir = AimDirPlayerInput;
        _rotation.right = aimDir - _rotation.position;
        _velocity = MovePlayerInput * _moveSpeed;

        if (aimDir.x != 0 && aimDir.y != 0) { _fieldOfView?.SetAimDirection(aimDir); }
        _fieldOfView?.SetOrigin(this.transform.position/*this.transform.position*/);
    }

    private void FixedUpdate()
    {
        _rigidbody.MovePosition(_rigidbody.position + _velocity * Time.fixedDeltaTime);
    }
}
