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

    [SerializeField]
    protected Animator ExpressionAnimator;
    
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
                fov.SetAimDirection(((Vector3)pathfindingNodes[currentWaypoint].GetCorrectPosition() - transform.position).normalized);
                transform.position = Vector3.MoveTowards(this.transform.position, pathfindingNodes[currentWaypoint].GetCorrectPosition(), Time.deltaTime * speed);
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
                        ExpressionAnimator.SetTrigger("None");
                        aiState = EnemyBehaviorState.WANDER;
                        alertLevel = 0;
                        return;
                    }

                    alertLevel = 0;
                    aiState = EnemyBehaviorState.CHASE;
                    GetNewChasePath();
                    //TODO: Add alert Anim
                }
                break;

            case EnemyBehaviorState.CHASE:
                chasePositionUpdate -= Time.deltaTime;

                if (chaseTarget != null)
                {
                    if (Vector3.Distance(transform.position, chaseTarget.position) <= 1f)
                    {
                        if (chaseTarget == DeamonScript.Instance.transform) 
                        {
                            StunEnemy(DeamonScript.Instance.StunTime);
                        }
                        else 
                        {
                            Time.timeScale = 0;
                            Debug.LogWarning("Player ded boyyyyyy");
                        }
                        return;
                    }
                }

                if (chasePositionUpdate <= 0)
                {
                    chasePositionUpdate = chaseTargetPositionUpdates;

                    if (chaseTarget != null)
                    {
                        GetNewChasePath();
                    }

                }

                if (ReachedChaseWaypoint())
                {

                    if (pathToSeenPlayer >= pathToLastSeeonChasePos.Count -1)
                    {
                        if (chaseTarget != null ) 
                        {
                            GetNewChasePath();
                            return;
                        }
                        
                        stunned = true;
                        StartCoroutine(WaitBeforePlayerEscape());
                    
                    }
                    else
                    {
                        pathToSeenPlayer++;
                    }
                }

                Debug.Log(pathToLastSeeonChasePos.Count);

                if (chaseTarget != null) 
                {
                    fov.SetAimDirection((chaseTarget.position - transform.position).normalized);
                }
                else 
                {
                    fov.SetAimDirection(((Vector3)pathToLastSeeonChasePos[pathToSeenPlayer].GetCorrectPosition() - transform.position).normalized);
                }


                transform.position = Vector3.MoveTowards(this.transform.position,pathToLastSeeonChasePos[pathToSeenPlayer].GetCorrectPosition(), Time.deltaTime * speed);


                break;

            case EnemyBehaviorState.SEARCH:
                break;
        }
    }

    protected void GetNewChasePath() 
    {
        pathToSeenPlayer = 0;
        pathToLastSeeonChasePos = Pathfinding.GetPath(transform.position, chaseTarget.position);
        
        if (pathToLastSeeonChasePos.Count > 1) 
        {
            pathToLastSeeonChasePos.RemoveAt(0);
        }
        
        if (pathToLastSeeonChasePos == null) Debug.Log("Why the fuck you null bro???");
    }
    public void OnFindingChaseTarget(FieldOfView.DETECTIONTYPE detectedThing) 
    {
        if (aiState != EnemyBehaviorState.CHASE) 
        {
            aiState = EnemyBehaviorState.ALERT;
            ExpressionAnimator.SetTrigger("QuestionMark");
        }
        else 
        {
            GetNewChasePath();
        }

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

    public void AlertEnemyTo( Transform alertToObject) 
    {
        chaseTarget = alertToObject;
        aiState = EnemyBehaviorState.CHASE;
        chasePositionUpdate = 0f;
        pathToLastSeeonChasePos = Pathfinding.GetPath(transform.position, chaseTarget.position);
    }
    public void AlertEnemyToPoint(Vector3 point) 
    {
        aiState = EnemyBehaviorState.CHASE;
        chasePositionUpdate = 0f;
        pathToLastSeeonChasePos = Pathfinding.GetPath(transform.position, point);
    }
    protected bool ReachedWaypoint() 
    {
        return transform.position == (Vector3) pathfindingNodes[currentWaypoint].GetCorrectPosition();
    }
    protected bool ReachedChaseWaypoint()
    {
        return transform.position == (Vector3) pathToLastSeeonChasePos[pathToSeenPlayer].GetCorrectPosition();
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
        ExpressionAnimator.SetTrigger("Hearth");
        ExpressionAnimator.SetTrigger("None");
        StopCoroutine(WaitForStun(stunTime));
        StartCoroutine(WaitForStun(stunTime));
    }

    private IEnumerator WaitBeforePlayerEscape() 
    {
        yield return new WaitForSeconds(3f);
        stunned = false;
        aiState = EnemyBehaviorState.WANDER;
        ExpressionAnimator.SetTrigger("None");
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
