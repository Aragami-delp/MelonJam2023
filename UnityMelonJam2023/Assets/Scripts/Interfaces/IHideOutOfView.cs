using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public interface IHideOutOfView
{
    public GameObject gameObject { get; }
    public void DisableRenderer();

    public void EnableRenderer();

    public bool AllowHide { get; }
}
