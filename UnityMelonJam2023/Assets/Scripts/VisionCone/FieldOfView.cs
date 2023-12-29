using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UIElements;

public class FieldOfView : MonoBehaviour
{
    [SerializeField] private float _fov = 90f;
    [SerializeField] private float _viewDistance = 50f;
    [SerializeField] int _rayCount = 50;
    [SerializeField] private LayerMask _layerMask;
    [SerializeField] private LayerMask _enemiesLayerMask;
    [SerializeField] private bool _hideEnemies = false;
    private List<SpriteRenderer> _spritesEnemyInView = new();
    private Mesh _mesh;
    private Vector3 _origin { get; set; }
    private float _startingAngle;

    private void Start()
    {
        transform.position = Vector3.zero;
        _mesh = new();
        GetComponent<MeshFilter>().mesh = _mesh;

        _spritesEnemyInView.Clear();
    }

    private void LateUpdate()
    {
        List<SpriteRenderer> newSprites = new();

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
                    if (enemiesRaycastHit2D.transform.TryGetComponent<SpriteRenderer>(out SpriteRenderer enemy))
                    {
                        if (enemy.gameObject.layer == LayerMask.NameToLayer("Target"))
                        {
                            newSprites.Add(enemy);
                        }
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
            foreach (SpriteRenderer item in _spritesEnemyInView.Except(newSprites).ToList())
            {
                _spritesEnemyInView.Remove(item);
                item.enabled = false;
            }
            foreach (SpriteRenderer item in newSprites)
            {
                _spritesEnemyInView.Add(item);
                item.enabled = true;
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
