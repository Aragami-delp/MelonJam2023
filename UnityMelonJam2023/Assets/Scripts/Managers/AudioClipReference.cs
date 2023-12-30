using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum AUDIOTYPE
{
    NONE = 0,
    MUSIC = 10,
    DOOR_OPEN = 100,
    DOOR_CLOSE = 101,
    LEVER = 102,
    ENEMY_QUESTION = 103,
    ENEMY_STUN = 104,
    MAINMENU_BUTTON = 105,
    SEND_DEAMON = 106,
}

[Serializable]
public struct AudioClipReference
{
    [SerializeField] public AUDIOTYPE Audio;
    [SerializeField] public AudioClip Reference;
}
