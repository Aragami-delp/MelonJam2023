using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class BaseEnemy : MonoBehaviour
{
    [SerializeField]
    private List<Transform> Waypoints;

    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void AddWaypoint() 
    {
        GameObject newWaypoint = new GameObject("WayPoint" + transform.childCount);
        Waypoints.Add(newWaypoint.transform);
    }

}

[CustomEditor(typeof(BaseEnemy))]
public class BaseEnemyEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        BaseEnemy enemy = (BaseEnemy)target;

        if (GUILayout.Button("Add Waypoint")) 
        {
            enemy.AddWaypoint();
        }
    }
}
