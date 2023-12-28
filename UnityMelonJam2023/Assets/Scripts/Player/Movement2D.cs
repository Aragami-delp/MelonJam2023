using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Movement2D : MonoBehaviour
{
    [SerializeField] private float _moveSpeed = 6f;
    [SerializeField] private FieldOfView _fieldOfView;

    private Rigidbody2D _rigidbody;
    private Camera _viewCamera;
    private Vector2 _velocity;

    private void Start()
    {
        _rigidbody = GetComponent<Rigidbody2D>();
        _viewCamera = _viewCamera ?? Camera.main;
    }

    private void Update()
    {
        Vector3 mousePos = Utility.GetMousePos(_viewCamera);
        transform.right = mousePos - transform.position;
        _velocity = Utility.GetInputDirection() * _moveSpeed;

        _fieldOfView?.SetAimDirection(mousePos - this.transform.position);
        _fieldOfView?.SetOrigin(this.transform.position);
    }

    private void FixedUpdate()
    {
        _rigidbody.MovePosition(_rigidbody.position + _velocity * Time.fixedDeltaTime);
    }
}
