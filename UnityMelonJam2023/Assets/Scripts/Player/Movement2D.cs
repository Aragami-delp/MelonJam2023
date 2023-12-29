using Unity.VisualScripting;
using UnityEngine;

public class Movement2D : MonoBehaviour
{
    [SerializeField] private float _moveSpeed = 6f;
    [SerializeField] private FieldOfView _fieldOfView;
    [SerializeField] private Transform _rotation;
    [SerializeField] private Animator _animator;
    [SerializeField] private SpriteRenderer _renderer;

    private Rigidbody2D _rigidbody;
    private Camera _viewCamera;
    private Vector2 _velocity;

    [HideInInspector] public Vector2 MovePlayerInput;
    [HideInInspector] public Vector2 AimDirPlayerInput;

    private void Start()
    {
        _rigidbody = GetComponent<Rigidbody2D>();
        _viewCamera = _viewCamera ?? Camera.main;


        _animator = GetComponent<Animator>();
        _renderer = GetComponent<SpriteRenderer>();
        this.AddComponent<PlayerInputMessages>();
    }

    private void Update()
    {
        Vector3 aimDir = AimDirPlayerInput;
        _rotation.right = aimDir - _rotation.position;
        _velocity = MovePlayerInput * _moveSpeed;

        _animator.speed = 1;

        if (MovePlayerInput.x != 0)
        {
            _animator.speed = 1.5f;

            _animator.SetInteger("HorizontalWalk", 1);
            _renderer.flipX = false;

            if (MovePlayerInput.x < 0)
            {
                _renderer.flipX = true;
            }
        }
        else
        {
            _animator.speed = 1.5f;
            _animator.SetInteger("HorizontalWalk", 0);
        }

        if (MovePlayerInput.y != 0)
        {
            _animator.speed = 1.5f;
            if (MovePlayerInput.y < 0)
            {
                _animator.SetInteger("VerticalWalk", 1);
                _animator.speed = 2; 
            }
            else
            {
                // down walk anim
            }
        }
        else
        {
            _animator.SetInteger("VerticalWalk", 0);
        }
        if (aimDir.x != 0 && aimDir.y != 0) { _fieldOfView?.SetAimDirection(aimDir); }
        _fieldOfView?.SetOrigin(this.transform.position/*this.transform.position*/);
    }

    private void FixedUpdate()
    {
        _rigidbody.MovePosition(_rigidbody.position + _velocity * Time.fixedDeltaTime);
    }
}
