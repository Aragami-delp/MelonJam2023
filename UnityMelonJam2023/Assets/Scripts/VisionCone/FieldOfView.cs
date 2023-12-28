using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class FieldOfView : MonoBehaviour
{
    [SerializeField] private float _fov = 90f;
    [SerializeField] private float _viewDistance = 50f;
    [SerializeField] int _rayCount = 50;
    [SerializeField] private LayerMask _layerMask;
    private Mesh _mesh;
    private Vector3 _origin = Vector3.zero;
    private float _startingAngle;

    private void Start()
    {
        _mesh = new();
        GetComponent<MeshFilter>().mesh = _mesh;
    }

    private void LateUpdate()
    {
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
            RaycastHit2D raycastHit2D = Physics2D.Raycast(_origin, Utility.GetVectorFromAngle(angle), _viewDistance, _layerMask);
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
        }

        _mesh.vertices = vertices;
        _mesh.uv = uv;
        _mesh.triangles = triangles;
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
