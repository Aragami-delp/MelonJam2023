using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Tilemaps;

public class DeamonScript : MonoBehaviour
{
    public static DeamonScript Instance;

    public bool IsbeingChased { get; set; }
    public float StunTime;
    [SerializeField] 
    private FieldOfView _fieldOfView;

    private Animator _animator;
    private SpriteRenderer _renderer;

    [SerializeField]
    private float speed = 5;

    [SerializeField]
    private Tilemap wallTileMap;

    private List<NodeBase> _path = new List<NodeBase>();

    private Vector3? _nextPos = null;

    private Vector3? _mouseClick = null;

    [SerializeField]
    LayerMask enemyLayer;

    private void Awake()
    {
        Instance = this;
        _animator = GetComponent<Animator>();
        _renderer = GetComponent<SpriteRenderer>();
    }

    // Update is called once per frame
    void Update()
    {
        _fieldOfView?.SetOrigin(this.transform.position);

        if(_nextPos != null)
        {
            this.transform.position = Vector3.MoveTowards(this.transform.position, _nextPos.Value, Time.deltaTime * speed);

            if (this.transform.position == _nextPos.Value) 
            {
                SetNextPos();

            }
        }

        if (_nextPos != null)
        {
            Vector3 walkDir = (_nextPos.Value - this.transform.position).normalized;

            _animator.speed = 1;

            if (walkDir.x != 0)
            {
                _animator.speed = 1.5f;

                _animator.SetInteger("HorizontalWalk", 1);
                _renderer.flipX = true;

                if (walkDir.x < 0)
                {
                    _renderer.flipX = false;
                }
            }
            else
            {
                _animator.speed = 1.5f;
                _animator.SetInteger("HorizontalWalk", 0);
            }

            if (walkDir.y != 0)
            {
                _animator.speed = 1.5f;
                if (walkDir.y < 0)
                {
                    _animator.SetInteger("VerticalWalk", 1);
                    _animator.speed = 2;
                }
                else
                {
                    _animator.SetInteger("VerticalWalk", -1);
                    _animator.speed = 2;
                }
            }
            else
            {
                _animator.SetInteger("VerticalWalk", 0);
            }
        }
        else
        {
            _animator.SetInteger("HorizontalWalk", 0);
            _animator.SetInteger("VerticalWalk", 0);
        }
    }

    public void Fire(InputAction.CallbackContext context)
    {
        Debug.Log("Click Event");
        if (context.started)
        {

            var mousclick = Utility.GetMousePos3();

            _path = Pathfinding.GetPath(this.transform.position, mousclick);

            if (_path.Count != 0)
            {
                _mouseClick = mousclick;
                _path.RemoveAt(0);
            }
            

            SetNextPos();
            GameManager.Instance?.PlaySound(AUDIOTYPE.SEND_DEAMON);
        }
    }

    private void SetNextPos()
    {
        if (_path.Count > 1)
        {
            _nextPos = wallTileMap.CellToWorld((Vector3Int)_path[0].OldPosition);
            _path.RemoveAt(0);
        } 
        else
        {
            if(_mouseClick == null)
            {
                _nextPos = null;
            } else
            {
                if (_path.Count != 0)
                { 
                    _path.RemoveAt(0);
                }
                _nextPos = new Vector3(Mathf.RoundToInt(_mouseClick.Value.x), Mathf.RoundToInt(_mouseClick.Value.y - 0.5f) + 0.5f, 0);
                _mouseClick = null;
            }
        }
    }

}
