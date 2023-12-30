using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using System.Linq;
public class BaseEnemy : MonoBehaviour
{
    [SerializeField]
    FieldOfView fov;
    [SerializeField]
    // List is only for show. WaypointVectors is used for logic
    protected List<Transform> Waypoints = new List<Transform>();
    protected List<Vector3> WaypointVectors = new List<Vector3>();
    [SerializeField]
    protected int currentWaypoint = 0;

    [SerializeField]
    protected float speed = 5;

    [SerializeField]
    protected float chaseTargetPositionUpdates;

    [SerializeField]
    protected bool stunned;

    [SerializeField]
    protected EnemyBehaviorState aiState;

    protected List<NodeBase> pathfindingNodes;
    protected int pathfindingProgress = 0;
    
    protected float alertLevel;

    protected Transform chaseTarget;

    protected float chasePositionUpdate = 1f;
    protected List<NodeBase> pathToLastSeeonChasePos;
    [SerializeField]
    protected int pathToSeenPlayer = 0;
    
    protected virtual void Start()
    {
        pathfindingNodes = Pathfinding.GetPath(Waypoints[0].position, Waypoints[1].position);

        for (int i = 2; i < Waypoints.Count; i++)
        {
            pathfindingNodes.AddRange(Pathfinding.GetPath(Waypoints[i - 1].position, Waypoints[i].position));
        }

        pathfindingNodes.AddRange(Enumerable.Reverse<NodeBase>(pathfindingNodes));

        foreach (Transform point in Waypoints)
        {
            WaypointVectors.Add(point.position);
        }

    
        transform.position = Waypoints[0].position;
    
    }

    protected virtual void Update()
    {
        fov.SetOrigin(transform.position);

        if (stunned) return;

        switch (aiState) 
        {
            case EnemyBehaviorState.WANDER:
                fov.SetAimDirection(((Vector3Int)pathfindingNodes[currentWaypoint].OldPosition - transform.position).normalized);
                transform.position = Vector3.MoveTowards(this.transform.position, (Vector3Int)pathfindingNodes[currentWaypoint].OldPosition, Time.deltaTime * speed);
                if (ReachedWaypoint())
                {
                    currentWaypoint = (currentWaypoint + 1 >= pathfindingNodes.Count) ? 0 : currentWaypoint + 1;

                    pathfindingProgress = 0;

                }
                break;
            
            case EnemyBehaviorState.ALERT:
                alertLevel += Time.deltaTime;
                if(alertLevel >= 3 ) 
                {
                    if (chaseTarget == null) 
                    {
                        aiState = EnemyBehaviorState.WANDER;
                        alertLevel = 0;
                        return;
                    }

                    alertLevel = 0;
                    aiState = EnemyBehaviorState.CHASE;
                    chasePositionUpdate = 0f;
                    pathToLastSeeonChasePos = Pathfinding.GetPath(transform.position, chaseTarget.position);
                    //TODO: Add alert Anim
                }
                break;

            case EnemyBehaviorState.CHASE:
                chasePositionUpdate -= Time.deltaTime;

                fov.SetAimDirection(((Vector3Int)pathToLastSeeonChasePos[pathToSeenPlayer].OldPosition - transform.position).normalized);

                transform.position = Vector3.MoveTowards(this.transform.position, (Vector3Int)pathToLastSeeonChasePos[pathToSeenPlayer].OldPosition, Time.deltaTime * speed);
                
                if (chasePositionUpdate <= 0) 
                {
                    pathToSeenPlayer = 0;
                    chasePositionUpdate = chaseTargetPositionUpdates;

                    if (chaseTarget != null) 
                    {
                        pathToLastSeeonChasePos = Pathfinding.GetPath(transform.position, chaseTarget.position);
                    }

                }

                if (ReachedChaseWaypoint()) 
                {
                    if (chaseTarget?.position == transform.position)
                    {
                        Debug.LogWarning(" u ded bro");
                        return;
                    }

                    if (pathToSeenPlayer >= pathToLastSeeonChasePos.Count) 
                    {
                        stunned = true;
                        StartCoroutine(WaitBeforePlayerEscape());
                    }
                    else 
                    {
                        pathToSeenPlayer++;
                    }
                }

                break;

            case EnemyBehaviorState.SEARCH:
                break;
        }
    }

    public void OnFindingChaseTarget(FieldOfView.DETECTIONTYPE detectedThing) 
    {
        aiState = EnemyBehaviorState.ALERT;

        if (detectedThing == FieldOfView.DETECTIONTYPE.PLAYER && (chaseTarget != DeamonScript.Instance.transform || DeamonScript.Instance.IsbeingChased))
        {
            chaseTarget = Movement2D.Instance.transform;
        }
        else 
        {
            chaseTarget = DeamonScript.Instance.transform;
            DeamonScript.Instance.IsbeingChased = true;
        }
    }
    public void OnLossingChaseTarget() 
    {
        if (chaseTarget == DeamonScript.Instance.transform) 
        {
            DeamonScript.Instance.IsbeingChased = false;
        }
        chaseTarget = null;
    }

    protected bool ReachedWaypoint() 
    {
        return transform.position == (Vector3Int)pathfindingNodes[currentWaypoint].OldPosition;
    }
    protected bool ReachedChaseWaypoint()
    {
        return transform.position == (Vector3Int)pathToLastSeeonChasePos[pathToSeenPlayer].OldPosition;
    }
    public void AddWaypoint() 
    {
        GameObject newWaypoint = new GameObject("WayPoint" + transform.childCount);
        newWaypoint.transform.parent = transform;
        newWaypoint.transform.position = transform.position;
        Waypoints.Add(newWaypoint.transform);
    }

    private void OnDrawGizmos()
    {
        if (Application.isPlaying) 
        {
            foreach (Vector3 child in WaypointVectors)
            {
                Gizmos.color = Color.red;
                Gizmos.DrawWireCube(child, Vector3.one * 0.25f);
            }
        }
        else 
        {
            foreach (Transform child in Waypoints)
            {
                Gizmos.color = Color.red;
                Gizmos.DrawWireCube(child.position, Vector3.one * 0.25f);
            }
        }
    }

    public void StunEnemy(float stunTime) 
    {
        StopCoroutine(WaitForStun(stunTime));
        StartCoroutine(WaitForStun(stunTime));
    }

    private IEnumerator WaitBeforePlayerEscape() 
    {
        yield return new WaitForSeconds(3f);
        aiState = EnemyBehaviorState.WANDER;
    }

    private IEnumerator WaitForStun(float stunTime) 
    {
        stunned = true;
        yield return new WaitForSeconds(stunTime);
        stunned = false;
        aiState = EnemyBehaviorState.WANDER;
    }
    public enum EnemyBehaviorState 
    { 
        WANDER,
        ALERT,
        CHASE,
        SEARCH
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
