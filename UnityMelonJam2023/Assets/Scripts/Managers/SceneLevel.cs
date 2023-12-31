using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum LEVEL
{
    MAINMENU = 101,
    NEXT = 102,
    LEVEL_0 = 0,
    LEVEL_1 = 1,
    LEVEL_2 = 2,
    LEVEL_3 = 3,
    LEVEL_4 = 4,
    LEVEL_5 = 5,
    LEVEL_6 = 6,
    LEVEL_7 = 7,
    LEVEL_8 = 8,
    LEVEL_9 = 9,
    LEVEL_10 = 10,
}

[Serializable]
public struct SceneLevel
{
    [SerializeField] public LEVEL Level;
    [SerializeField] public string SceneName;
}
