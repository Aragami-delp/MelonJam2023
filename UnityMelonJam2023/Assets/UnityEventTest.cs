using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UnityEventTest : MonoBehaviour
{
    public void PlayerFound(FieldOfView.DETECTIONTYPE detectiontype)
    {
        Debug.Log("Found " + detectiontype.ToString());
    }

    public void PlayerLost(FieldOfView.DETECTIONTYPE detectiontype)
    {
        Debug.Log("Lost " + detectiontype.ToString());
    }
}
