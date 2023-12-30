using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using System.Linq;
using System;

[DefaultExecutionOrder(100)]
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
    protected Animator ExpressionAnimator,guardAnimator;
    [SerializeField]
    protected SpriteRenderer animationSpriteRenderer;
    [SerializeField]
    protected bool dontHideOnStart;
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

        if(!dontHideOnStart) DisableRenderer();
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


        EnemyAnim();
        switch (aiState) 
        {
            case EnemyBehaviorState.WANDER:
                if (!(Waypoints.Count == 2 && Waypoints[0].Equals(Waypoints[1])))
                {
                    SetEnemyAimDirection(((Vector3)pathfindingNodes[currentWaypoint].GetCorrectPosition() - transform.position).normalized);
                }
                else
                {
                    SetEnemyAimDirection(null);
                }
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
                            GameManager.Instance.ReloadScene();
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
                    SetEnemyAimDirection((chaseTarget.position - transform.position).normalized);
                }
                else 
                {
                    SetEnemyAimDirection(((Vector3)pathToLastSeeonChasePos[pathToSeenPlayer].GetCorrectPosition() - transform.position).normalized);
                }


                transform.position = Vector3.MoveTowards(this.transform.position,pathToLastSeeonChasePos[pathToSeenPlayer].GetCorrectPosition(), Time.deltaTime * chaseSpeed);


                break;

            case EnemyBehaviorState.SEARCH:
                break;
        }
    }

    private void EnemyAnim() 
    {
        if ( aiState != EnemyBehaviorState.ALERT)
        {
            Vector3 walkDir;
            if (aiState == EnemyBehaviorState.WANDER)
            {
                walkDir = ((Vector3)pathfindingNodes[currentWaypoint].GetCorrectPosition() - this.transform.position).normalized;
            }
            else if(aiState == EnemyBehaviorState.CHASE)
            { 
                walkDir = ((Vector3)pathToLastSeeonChasePos?[pathToSeenPlayer].GetCorrectPosition() - this.transform.position).normalized;
            }
            else 
            {
                walkDir = transform.forward;
            }

            guardAnimator.speed = 1;

            if (walkDir.x != 0)
            {
                guardAnimator.speed = 1.5f;

                guardAnimator.SetInteger("HorizontalWalk", 1);
                animationSpriteRenderer.flipX = false;

                if (walkDir.x < 0)
                {
                    animationSpriteRenderer.flipX = true;
                }
            }
            else
            {
                guardAnimator.speed = 1.5f;
                guardAnimator.SetInteger("HorizontalWalk", 0);
            }

            if (walkDir.y != 0)
            {
                guardAnimator.speed = 1.5f;
                if (walkDir.y < 0)
                {
                    guardAnimator.SetInteger("VerticalWalk", 1);
                    guardAnimator.speed = 2;
                }
                else
                {
                    guardAnimator.SetInteger("VerticalWalk", -1);
                    guardAnimator.speed = 2;
                }
            }
            else
            {
                guardAnimator.SetInteger("VerticalWalk", 0);
            }
        }
        else
        {
            guardAnimator.SetInteger("HorizontalWalk", 0);
            guardAnimator.SetInteger("VerticalWalk", 0);
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
            GameManager.Instance?.PlaySound(AUDIOTYPE.ENEMY_QUESTION);
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
        catch
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
        GameManager.Instance?.PlaySound(AUDIOTYPE.ENEMY_STUN);
        stunBoy = StartCoroutine(WaitForStun(stunTime));
    }

    private IEnumerator WaitBeforePlayerEscape() 
    {
        yield return new WaitForSeconds(3f);
        stunned = false;

        if (chaseTarget == null) 
        { 
            aiState = EnemyBehaviorState.WANDER;
            PlayExpression("None");
        }
        else
        {
            PlayExpression("None");
            aiState = EnemyBehaviorState.ALERT;
            PlayExpression("QuestionMark");
        }
        PlayExpression("None");
    }

    private IEnumerator WaitForStun(float stunTime) 
    {
        
        fov.ShowFOV = false;
        stunned = true;
        yield return new WaitForSeconds(stunTime);
        PlayExpression("None");
        stunned = false;

        if (chaseTarget == null) aiState = EnemyBehaviorState.WANDER;
        else
        {
            PlayExpression("None");
            PlayExpression("QuestionMark");
            aiState = EnemyBehaviorState.ALERT;
        }
        if (spriteRenderer.enabled) 
        {
            fov.ShowFOV = true;
        }
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

    private void SetEnemyAimDirection(Vector3? aimDirection)
    {
        fov.SetAimDirection(aimDirection);
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
