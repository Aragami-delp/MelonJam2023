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

    List<NodeBase> oldNodes = new List<NodeBase>();
    void Start()
    {
        //ShowPath();
    }

    [ContextMenu("Test Show path")]
    public void ShowPath() 
    {
        foreach (var oldNodes in oldNodes)
        {
            wall.SetTile((Vector3Int)oldNodes.OldPosition,null);
        }

        List<NodeBase> nodes = Pathfinding.GetPath(startPos.position, endPos.position);
        oldNodes = nodes;
        foreach (NodeBase node in nodes)
        {
            wall.SetTile((Vector3Int)node.OldPosition, DebuggTile);
        }


    }
}
