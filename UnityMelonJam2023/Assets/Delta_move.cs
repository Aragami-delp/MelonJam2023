using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Delta_move : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        transform.position += transform.right * 0.05f;
    }
}
