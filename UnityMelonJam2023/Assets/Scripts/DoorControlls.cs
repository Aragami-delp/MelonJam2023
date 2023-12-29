using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class DoorControlls : MonoBehaviour, IInteractionTarget
{
    [SerializeField]
    bool startOpen;
    [SerializeField]
    bool proximityOpen;
    [SerializeField]
    float proximityRange;

    bool doLerp;
    bool openDoor;
    DoorPos leftDoor;
    DoorPos rightDoor;


    void Start()
    {
        Transform leftDoorChild = transform.GetChild(0);
        leftDoor = new DoorPos(leftDoorChild.position, GetTargetPos(leftDoorChild), leftDoorChild);

        Transform rightDoorChild = transform.GetChild(1);
        rightDoor = new DoorPos(rightDoorChild.position, GetTargetPos(rightDoorChild), rightDoorChild);

        if (startOpen) 
        {
            OpenDoor();
        }

        if (proximityOpen) 
        {
            GetComponent<CircleCollider2D>().radius = proximityRange;
        }
        else 
        {
            GetComponent<CircleCollider2D>().enabled = false;
        }
    }

    [SerializeField]
    Vector2 openAmmount;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        OpenDoor();
    }
    private void OnTriggerExit2D(Collider2D collision)
    {
        CloseDoor();
    }

    private void OnDrawGizmos()
    {
        if (proximityOpen)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawWireSphere(transform.position + (transform.right * 0.5f), proximityRange);
        }
    }

    float time = 0f;
    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space)) 
        {
            OpenDoor();
        }
        else if (Input.GetKeyDown(KeyCode.Escape)) 
        {
            CloseDoor();
        }

        if (!doLerp) return;

        foreach (Transform child in transform)
        {
            time += Time.deltaTime;

            if (openDoor)
            {
                leftDoor.Child.position = Vector3.Lerp(leftDoor.StartPos, leftDoor.EndPos, time);
                rightDoor.Child.position = Vector3.Lerp(rightDoor.StartPos, rightDoor.EndPos, time);
            }
            else 
            {
                leftDoor.Child.position = Vector3.Lerp(leftDoor.EndPos, leftDoor.StartPos, time);
                rightDoor.Child.position = Vector3.Lerp(rightDoor.EndPos, rightDoor.StartPos, time);
            }

            if (time >= 1f) 
            {
                doLerp = false;
            }
        }
    }
    [ContextMenu("OpenDoor")]
    public void OpenDoor() 
    {
        if (leftDoor.Child.position != leftDoor.EndPos && !openDoor && !doLerp) 
        {
            doLerp = true;
            openDoor = true;
            time = 0f;
        }
    }
    [ContextMenu("CloseDoor")]
    public void CloseDoor() 
    {
        if (leftDoor.Child.position != leftDoor.StartPos && openDoor && !doLerp) 
        {
            doLerp = true;
            openDoor = false;
            time = 0f;
        }
    }

    private Vector3 GetTargetPos(Transform child) 
    {
            return child.transform.position + child.transform.right * openAmmount.x + child.transform.right * openAmmount.y;
    }

    public void DoStuff(bool _condition)
    {
        if(_condition)
        {
            OpenDoor();
        }
        else
        {
            CloseDoor();
        }
    }

    private struct DoorPos 
    {
        public Vector3 StartPos;
        public Vector3 EndPos;
        public Transform Child;

        public DoorPos(Vector3 startPos, Vector3 endPos,Transform child)
        {
            StartPos = startPos;
            EndPos = endPos;
            Child = child;
        }
    }
}
[CustomEditor(typeof(DoorControlls))]
public class DoorControllsEditor : Editor
{
    SerializedProperty proximityOpenProperty;
    SerializedProperty proximityRangeProperty;

    private void OnEnable()
    {
        proximityOpenProperty = serializedObject.FindProperty("proximityOpen");
        proximityRangeProperty = serializedObject.FindProperty("proximityRange");
    }

    public override void OnInspectorGUI()
    {

        serializedObject.Update();


        DrawDefaultInspector();



        serializedObject.ApplyModifiedProperties();
    }
}
