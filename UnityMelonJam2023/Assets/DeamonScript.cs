using System.Collections;
using System.Collections.Generic;
using UnityEditor.ShaderKeywordFilter;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Tilemaps;

public class DeamonScript : MonoBehaviour
{

    [SerializeField] 
    private FieldOfView _fieldOfView;

    [SerializeField]
    private float speed = 5;

    [SerializeField]
    private Tilemap wallTileMap;

    private List<NodeBase> _path = new List<NodeBase>();

    private Vector3? _nextPos = null;

    private Vector3? _mouseClick = null;


    // Start is called before the first frame update
    void Start()
    {
        
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

    }

    public void Fire(InputAction.CallbackContext context)
    {
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
