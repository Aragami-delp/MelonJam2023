using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class A_StarDebugg : MonoBehaviour
{
    [SerializeField]
    Tilemap wall;

    [SerializeField]
    TileBase DebuggTile;

    [SerializeField]
    Transform startPos, endPos;

    void Start()
    {
        List<NodeBase> nodes = Pathfinding.GetPath(startPos.position,endPos.position);

        foreach (NodeBase node in nodes)
        {
            wall.SetTile((Vector3Int)node.OldPosition,DebuggTile);
        }

    }

}
