using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;

public static class Utility
{
    /// <summary>
    /// Gets 2D mouse position in world
    /// </summary>
    /// <param name="camera">Main camera as parameter since its expensive to call main camera</param>
    /// <returns></returns>
    public static Vector2 GetMousePos(Camera camera = null)
    {
        camera = camera ?? Camera.main;
        return camera.ScreenToWorldPoint(
            new Vector3(Mouse.current.position.value.x, Mouse.current.position.value.y, 0f));
    }

    public static Vector3 ToVector3(this Vector2 vec2)
    {
        return new Vector3(vec2.x, vec2.y, 0f);
    }

    public static Vector3 GetVectorFromAngle(float angle)
    {
        float angleRad = angle * (Mathf.PI/180f);
        return new Vector3(Mathf.Cos(angleRad), Mathf.Sin(angleRad));
    }

    public static float GetAngleFromVectorFloat(Vector3 dir)
    {
        dir = dir.normalized;
        float n = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
        if (n < 0f) { n += 360; }
        return n;
    }
}
