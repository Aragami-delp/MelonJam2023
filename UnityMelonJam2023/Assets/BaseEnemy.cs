using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using System.Linq;
using System;

public class BaseEnemy : MonoBehaviour, IHideOutOfView
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
    protected float speed = 5,chaseSpeed = 7;

    [SerializeField]
    protected bool loopToBeginning;
    [SerializeField]
    protected float timeUntillChase = 2f;

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

    [SerializeField]
    private SpriteRenderer spriteRenderer;
    
    protected virtual void Start()
    {
        DisableRenderer();
        if (Waypoints.Count == 1) 
        {
            Waypoints.Add(Waypoints.First());
        }

        if (loopToBeginning) 
        {
            GetPathfindingLoop();
        }
        else 
        {
            GetPathfindingLinear();
        }


        foreach (Transform point in Waypoints)
        {
            WaypointVectors.Add(point.position);
        }

    
        transform.position = Waypoints[0].position;
    
    }

    protected virtual void Update()
    {
        fov.SetOrigin(transform.position);
        
        if (Vector3.Distance(transform.position, DeamonScript.Instance.transform.position) <= 2f)
        {
            
            StunEnemy(DeamonScript.Instance.StunTime);

        }

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
                if(alertLevel >= timeUntillChase) 
                {
                    if (chaseTarget == null) 
                    {
                        //ExpressionAnimator.SetTrigger("None");
                        PlayExpression("None");
                        aiState = EnemyBehaviorState.WANDER;
                        alertLevel = 0;
                        return;
                    }

                    alertLevel = 0;
                    aiState = EnemyBehaviorState.CHASE;
                    GetNewChasePath();
                    
                }
                break;

            case EnemyBehaviorState.CHASE:
                chasePositionUpdate -= Time.deltaTime;

                if (chaseTarget != null)
                {
                    if (Vector3.Distance(transform.position, chaseTarget.position) <= 1.1f)
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


                if (chaseTarget != null) 
                {
                    fov.SetAimDirection((chaseTarget.position - transform.position).normalized);
                }
                else 
                {
                    fov.SetAimDirection(((Vector3)pathToLastSeeonChasePos[pathToSeenPlayer].GetCorrectPosition() - transform.position).normalized);
                }


                transform.position = Vector3.MoveTowards(this.transform.position,pathToLastSeeonChasePos[pathToSeenPlayer].GetCorrectPosition(), Time.deltaTime * chaseSpeed);


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
            PlayExpression("QuestionMark");
        }
        else 
        {
            //GetNewChasePath();
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
        Debug.LogWarning("target yeetus");
    }

    private void GetPathfindingLinear()
    {
        pathfindingNodes = Pathfinding.GetPath(Waypoints[0].position, Waypoints[1].position);

        for (int i = 2; i < Waypoints.Count; i++)
        {
            pathfindingNodes.AddRange(Pathfinding.GetPath(Waypoints[i - 1].position, Waypoints[i].position));
        }

        pathfindingNodes.AddRange(Enumerable.Reverse<NodeBase>(pathfindingNodes));
    }

    private void GetPathfindingLoop() 
    {
        pathfindingNodes = Pathfinding.GetPath(Waypoints[0].position, Waypoints[1].position);

        for (int i = 2; i < Waypoints.Count; i++)
        {
            pathfindingNodes.AddRange(Pathfinding.GetPath(Waypoints[i - 1].position, Waypoints[i].position));
        }

        Pathfinding.GetPath(Waypoints[Waypoints.Count -1].position, Waypoints[0].position);
    }
    private void PlayExpression(string play) 
    {
        switch (play) 
        {
            case "None":
                ExpressionAnimator.ResetTrigger("Hearth");
                ExpressionAnimator.ResetTrigger("QuestionMark");
                ExpressionAnimator.SetTrigger("None");
                break;
            case "Hearth":
                ExpressionAnimator.ResetTrigger("None");
                ExpressionAnimator.ResetTrigger("QuestionMark");
                ExpressionAnimator.SetTrigger("Hearth");
                break;
            case "QuestionMark":
                ExpressionAnimator.ResetTrigger("Hearth");
                ExpressionAnimator.ResetTrigger("None");
                ExpressionAnimator.SetTrigger("QuestionMark");
                break;
        }
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
        try
        {
            if (pathToLastSeeonChasePos.Count == 0) 
            {
                return true;
            }
            return transform.position == (Vector3)pathToLastSeeonChasePos[pathToSeenPlayer].GetCorrectPosition();
        }
        catch (Exception e) 
        {
            Debug.Log(pathToLastSeeonChasePos.Count);
            return false;
        }
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

    Coroutine stunBoy;

    public void StunEnemy(float stunTime)
    { 
        if (stunBoy != null) 
        {
            StopCoroutine(stunBoy);
        }
        PlayExpression("Hearth");
        stunBoy = StartCoroutine(WaitForStun(stunTime));
    }

    private IEnumerator WaitBeforePlayerEscape() 
    {
        yield return new WaitForSeconds(3f);
        stunned = false;
        aiState = EnemyBehaviorState.WANDER;
        PlayExpression("None");
    }

    private IEnumerator WaitForStun(float stunTime) 
    {
        
        fov.ShowFOV = false;
        stunned = true;
        yield return new WaitForSeconds(stunTime);
        PlayExpression("None");
        stunned = false;
        aiState = EnemyBehaviorState.WANDER;
        fov.ShowFOV = true;
    }
    public enum EnemyBehaviorState 
    { 
        WANDER,
        ALERT,
        CHASE,
        SEARCH
    }

    public void DisableRenderer()
    {
        spriteRenderer.enabled = false;
        fov.ShowFOV = false;
    }

    public void EnableRenderer()
    {
        spriteRenderer.enabled = true;
        fov.ShowFOV = true;
    }

    public bool AllowHide => true;
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
