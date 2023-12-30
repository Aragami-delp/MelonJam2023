using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UIElements;
using static UnityEditor.Experimental.GraphView.GraphView;

[RequireComponent(typeof(MeshRenderer), typeof(MeshFilter))]
public class FieldOfView : MonoBehaviour
{
    public enum DETECTIONTYPE
    {
        NONE = 0,
        PLAYER = 1,
        ASSISTANT = 2,
    }

    [Header("General"), SerializeField] private float _fov = 90f;
    [SerializeField] private float _viewDistance = 50f;
    [SerializeField] int _rayCount = 50;
    [SerializeField] private LayerMask _layerMask;
    [Header("Remove Enemy Renderer"), SerializeField] private bool _hideEnemies = false;
    [SerializeField] private LayerMask _enemiesLayerMask;
    [Header("Detect Player"), SerializeField, Tooltip("Detect player and assistant")]
    private bool _detectPlayerAssistant = true;
    [SerializeField] private LayerMask _assistantLayerMask;
    [SerializeField] private LayerMask _playerLayerMask;
    [SerializeField] private UnityEvent<DETECTIONTYPE> _detectedPlayer;
    [SerializeField] private UnityEvent<DETECTIONTYPE> _lostPlayer;

    private bool _playerCurrentlyDetected = false;
    private bool _assistantCurrentlyDetected = false;

    private List<BaseEnemy> _spritesEnemyInView = new();
    private Mesh _mesh;
    private MeshRenderer _meshRenderer;
    private Vector3 _origin { get; set; }

    private float _startingAngle;

    private bool showFOV = true;
    public bool ShowFOV
    {
        get { return showFOV; }
        set
        {
            _meshRenderer.enabled = value;
            showFOV = value;
        }
    }

    private void Start()
    {
        transform.position = Vector3.zero;
        _mesh = new();
        GetComponent<MeshFilter>().mesh = _mesh;
        _meshRenderer = GetComponent<MeshRenderer>();

        _spritesEnemyInView.Clear();
    }

    private void LateUpdate()
    {
        List<BaseEnemy> newSprites = new();
        bool thisUpdatePlayerDetected = false;
        bool thisUpdateAssistantDetected = false;

        float angle = _startingAngle;
        float angleIncrease = _fov / _rayCount;

        Vector3[] vertices = new Vector3[_rayCount + 1 + 1];
        Vector2[] uv = new Vector2[vertices.Length];
        int[] triangles = new int[_rayCount * 3];

        vertices[0] = _origin;

        int vertexIndex = 1;
        int triangleIndex = 0;
        for (int i = 0; i < _rayCount; i++)
        {
            Vector3 vertex;
            Vector3 vecFromAngle = Utility.GetVectorFromAngle(angle);
            RaycastHit2D raycastHit2D = Physics2D.Raycast(_origin, vecFromAngle, _viewDistance, _layerMask);


            if (raycastHit2D.collider == null)
            {
                vertex = _origin + Utility.GetVectorFromAngle(angle) * _viewDistance;
            }
            else
            {
                vertex = raycastHit2D.point;
            }

            vertices[vertexIndex] = vertex;

            if (i > 0)
            {
                triangles[triangleIndex + 0] = 0;
                triangles[triangleIndex + 1] = vertexIndex - 1;
                triangles[triangleIndex + 2] = vertexIndex;

                triangleIndex += 3;
            }

            vertexIndex++;
            angle -= angleIncrease;

            // HideEnemies
            if (_hideEnemies)
            {
                RaycastHit2D enemiesRaycastHit2D = Physics2D.Raycast(_origin, vecFromAngle, _viewDistance, _enemiesLayerMask | _layerMask);
                if (enemiesRaycastHit2D.collider != null)
                {
                    if (enemiesRaycastHit2D.transform.TryGetComponent(out BaseEnemy enemy))
                    {
                        if (_enemiesLayerMask == (_enemiesLayerMask | (1 << enemy.gameObject.layer)))
                            //enemy.gameObject.layer == LayerMask.NameToLayer("Target"))
                        {
                            newSprites.Add(enemy);
                        }
                    }
                }
            }
            if (_detectPlayerAssistant)
            {
                RaycastHit2D enemiesRaycastHit2D = Physics2D.Raycast(_origin, vecFromAngle, _viewDistance, _playerLayerMask | _assistantLayerMask | _layerMask);
                if (enemiesRaycastHit2D.collider != null)
                {
                    if (_playerLayerMask == (_playerLayerMask | (1 << enemiesRaycastHit2D.transform.gameObject.layer)) /*&&
                        !_playerCurrentlyDetected*/)
                    {
                        thisUpdatePlayerDetected = true;
                    }
                    else if (_assistantLayerMask == (_assistantLayerMask | (1 << enemiesRaycastHit2D.transform.gameObject.layer)) /*&&
                        !_assistantCurrentlyDetected*/)
                    {
                        thisUpdateAssistantDetected = true;
                    }
                }
            }
        }

        _mesh.vertices = vertices;
        _mesh.uv = uv;
        _mesh.triangles = triangles;

        _mesh.RecalculateBounds();

        if (_hideEnemies)
        {
            foreach (BaseEnemy item in _spritesEnemyInView.Except(newSprites).ToList())
            {
                _spritesEnemyInView.Remove(item);
                item.DisableRenderer();
            }
            foreach (BaseEnemy item in newSprites)
            {
                _spritesEnemyInView.Add(item);
                item.EnableRenderer();
            }
        }
        if (_detectPlayerAssistant)
        {
            if (thisUpdatePlayerDetected && !_playerCurrentlyDetected)
            {
                _playerCurrentlyDetected = true;
                _detectedPlayer.Invoke(DETECTIONTYPE.PLAYER);
            }
            else if (!thisUpdatePlayerDetected && _playerCurrentlyDetected)
            {
                _playerCurrentlyDetected = false;
                _lostPlayer.Invoke(DETECTIONTYPE.PLAYER);
            }

            if (thisUpdateAssistantDetected && !_assistantCurrentlyDetected)
            {
                _assistantCurrentlyDetected = true;
                _detectedPlayer.Invoke(DETECTIONTYPE.ASSISTANT);
            }
            else if (!thisUpdateAssistantDetected && _assistantCurrentlyDetected)
            {
                _assistantCurrentlyDetected = false;
                _lostPlayer.Invoke(DETECTIONTYPE.ASSISTANT);
            }
        }
    }

    public void SetOrigin(Vector3 origin)
    {
        this._origin = origin;

    }

    public void SetAimDirection(Vector3 aimDirection)
    {
        this._startingAngle = Utility.GetAngleFromVectorFloat(aimDirection) + _fov / 2f;
    }
}
