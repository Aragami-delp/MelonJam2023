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
            Debug.Log("Pos: " + Utility.GetMousePos3());

            _path = Pathfinding.GetPath(this.transform.position, Utility.GetMousePos3());

            Debug.Log("Test: " + _path.Count);

            SetNextPos();
        }
    }

    private void SetNextPos()
    {
        if (_path.Count > 0)
        {

            _nextPos = wallTileMap.CellToWorld((Vector3Int)_path[0].OldPosition);
            Debug.Log("Current Pos:" + this.transform.position  +"_nextPos: " + _nextPos);
            _path.RemoveAt(0);
        } else
        {
            _nextPos = null;
            Debug.Log("Am ziel: " + _nextPos);
        }
    }
}
