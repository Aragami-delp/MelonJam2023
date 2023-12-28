using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class DoorControlls : MonoBehaviour
{
    [SerializeField]
    bool proximityOpen;
    [SerializeField]
    float proximityRange;

    Vector3 targetPos;
    bool doLerp;

    void Start()
    {

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
        Debug.Log(collision.name);
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


    private void Update()
    {
        if (!doLerp) return;

        foreach (Transform child in transform)
        {

            child.transform.position += Vector3.Lerp(child.transform.position,targetPos,0f);
        }
    }

    public void OpenDoor() 
    {
        foreach (Transform child in transform)
        {
            Debug.Log(child.transform.right * openAmmount.x + child.transform.right * openAmmount.y);
            child.transform.position += child.transform.right * openAmmount.x + child.transform.right * openAmmount.y;
        }
    }

    public void CloseDoor() 
    {
        foreach (Transform child in transform)
        {
            child.transform.position += -(child.transform.right) * openAmmount.x + -(child.transform.right) * openAmmount.y;
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

        if (!proximityOpenProperty.boolValue)
        {
            SerializedProperty property = serializedObject.FindProperty("proximityRange");

            Rect rect = EditorGUILayout.GetControlRect(false, 0);
            property.isExpanded = EditorGUI.Foldout(rect, property.isExpanded, GUIContent.none);
        }

        serializedObject.ApplyModifiedProperties();
    }
}
