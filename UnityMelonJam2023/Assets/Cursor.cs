using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CustomCursor : MonoBehaviour
{
#if UNITY_EDITOR
    [SerializeField] private bool _showCursorInEditor = false;
#endif
    // Start is called before the first frame update
    void Start()
    {
        Cursor.visible =
#if UNITY_EDITOR
            _showCursorInEditor;
#else
            false;
#endif
    }

    // Update is called once per frame
    void Update()
    {
        Vector2 mouseScreenPos = Utility.GetMousePos2();
        this.transform.position =  new Vector2(Mathf.RoundToInt(mouseScreenPos.x), Mathf.RoundToInt(mouseScreenPos.y - 0.5f) + 0.5f);
    }

    private void OnDisable()
    {
        Cursor.visible = true;
    }
}
