using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Tilemaps;

public class Pathfinding : MonoBehaviour
{
    public static Pathfinding Instance;
    [SerializeField]
    private Tilemap wallTilemap;

    [SerializeField]
    private int moveCost = 10, moveDiagonalCost = 14;
    NodeBase[,] NodeBase;

    Vector2Int offset;

    [SerializeField]
    TileBase DEBUGG_TILE;
    private void Awake()
    {
        Instance = this;

        if (wallTilemap == null)
        {
            wallTilemap = GetComponent<Tilemap>();
        }
        InitPathfinding();

    }
    void Start()
    {

    }

    public void InitPathfinding()
    {
        BoundsInt bounds = wallTilemap.cellBounds;

        NodeBase = new NodeBase[bounds.size.x, bounds.size.y];

        offset = new Vector2Int(Mathf.Abs(bounds.xMin), Mathf.Abs(bounds.yMin));

        Debug.Log("Pathing Map init size: "+ bounds.size.x + " "+ bounds.size.y +"!");

        for (int x = 0; x < bounds.size.x; x++)
        {
            for (int y = 0; y < bounds.size.y; y++)
            {
                NodeBase newNode = new NodeBase();
                newNode.Position = new Vector2Int(x, y);
                newNode.OldPosition = newNode.Position - offset;
                newNode.CanWalkOver = !wallTilemap.HasTile((Vector3Int)newNode.OldPosition);
                NodeBase[x, y] = newNode;
            }
        }


    }

    public static void ChangeTileWalkable(Vector3 TileWorldPos, bool should)
    {
        try
        {
            Instance.PositionToNodeBase(TileWorldPos).CanWalkOver = should;
        }
        catch (NullReferenceException e)
        {
            // This thing might not be on a tileset
            Debug.Log("Pathfinding error, probably not on a grid: " + e.InnerException);
        }
    }
    // TODO: Work in progress
    public List<NodeBase> FindPath(Vector3 startPos, Vector3 endPos)
    {

        if (!PositionInPathfinding(startPos) || !PositionInPathfinding(endPos))
        {
            Debug.LogWarning("Start or end point not in Pathing!");
            return new List<NodeBase>();
        }

        NodeBase startNode = PositionToNodeBase(startPos);
        NodeBase endNode = PositionToNodeBase(endPos);

        List<NodeBase> toCheck = new List<NodeBase>();
        List<NodeBase> checkedNodes = new List<NodeBase>();
        toCheck.Add(startNode);

        //Reset from last Calc
        for (int x = 0; x < GetWith(); x++)
        {
            for (int y = 0; y < GetHight(); y++)
            {
                NodeBase node = NodeBase[x, y];

                node.GCost = int.MaxValue;
                node.PreviousNode = null;
            }
        }

        startNode.GCost = 0;
        startNode.HCost = CalculateDistanceCost(startNode, endNode);

        while (toCheck.Count > 0)
        {
            NodeBase current = GetLowestFCostNode(toCheck);

            if (current == endNode)
            {
                // reached final node
                return CalculatePath(endNode);
            }

            toCheck.Remove(current);
            checkedNodes.Add(current);
            foreach (NodeBase neighbourNode in GetNeighbours(current))
            {
                if (checkedNodes.Contains(neighbourNode)) continue;

                int GCost = current.GCost + CalculateDistanceCost(current, neighbourNode);
                if (GCost < neighbourNode.GCost)
                {
                    neighbourNode.PreviousNode = current;
                    neighbourNode.GCost = GCost;
                    neighbourNode.HCost = CalculateDistanceCost(neighbourNode, endNode);

                    if (!toCheck.Contains(neighbourNode))
                    {
                        toCheck.Add(neighbourNode);
                    }
                }
            }
        }

        Debug.LogWarning("No path found from: " + startPos + " To: " + endPos);
        Debug.LogWarning("start interpreted to: " + startNode.OldPosition);
        Debug.LogWarning("end interpreted to: " + endNode.OldPosition);
        // checked all nodes. no path found
        return new List<NodeBase>();
    }

    public static List<NodeBase> GetPath(Vector3 startPos, Vector3 endPos)
    {
        return Instance.FindPath(startPos, endPos);
    }
    private List<NodeBase> GetNeighbours(NodeBase current)
    {
        List<NodeBase> nodes = new();
        if (current.Position.x -1 >= 0)
        {
            //left
            CheckNeighbour(nodes, current.Position.x - 1, current.Position.y);

            // Left Down
            if (current.Position.y - 1 >= 0)
            {
                CheckNeighbour(nodes, current.Position.x - 1, current.Position.y-1);
            }
            // Left up
            if (current.Position.y + 1 < GetHight()) CheckNeighbour(nodes, current.Position.x - 1, current.Position.y + 1);
        }
        if (current.Position.x +1 < GetWith())
        {
            // right
            int newX = current.Position.x + 1;
            CheckNeighbour(nodes, newX, current.Position.y);
            // right down
            if (current.Position.y - 1 >= 0) CheckNeighbour(nodes, newX, current.Position.y -1);
            // right up
            if (current.Position.y + 1 < GetHight()) CheckNeighbour(nodes, newX, current.Position.y + 1);
        }
        // down
        if (current.Position.y - 1 >= 0) CheckNeighbour(nodes, current.Position.x, current.Position.y - 1);
        // up
        if (current.Position.y + 1 < GetHight()) CheckNeighbour(nodes, current.Position.x, current.Position.y + 1);
        return nodes;
    }
    private void CheckNeighbour(List<NodeBase> ListToAdd, int x, int y)
    {
        NodeBase left = GetNode(x, y);
        if (left.CanWalkOver)
        {
            ListToAdd.Add(left);
        }
    }
    private List<NodeBase> CalculatePath(NodeBase endNode)
    {
        List<NodeBase> path = new();

        NodeBase currentNode = endNode;
        path.Add(currentNode);

        while (currentNode.PreviousNode != null)
        {
            path.Add(currentNode.PreviousNode);
            currentNode = currentNode.PreviousNode;
        }
        path.Reverse();
        Debug.Log("Pathfinding ended with a success. Length: " + path.Count);
        return path;
    }

    public int CalculateDistanceCost(NodeBase start, NodeBase end)
    {

        int xDis = Mathf.Abs(start.Position.x - end.Position.x);
        int yDis = Mathf.Abs(start.Position.y - end.Position.y);

        int remaining = Mathf.Abs(xDis - yDis);

        return moveDiagonalCost * Mathf.Min(xDis, yDis) + moveCost * remaining;
    }
    public int Distance(NodeBase start, NodeBase end)
    {

        int xDis = Mathf.Abs(start.Position.x - end.Position.x);
        int yDis = Mathf.Abs(start.Position.y - end.Position.y);

        return Mathf.Abs(xDis - yDis);
    }

    public NodeBase PositionToNodeBase(Vector3 pos)
    {
        Vector2Int posV2 = (Vector2Int)wallTilemap.WorldToCell(pos);
        int newX = posV2.x + offset.x;
        int newY = posV2.y + offset.y;

        return NodeBase[newX, newY];
    }

    public bool PositionInPathfinding(Vector3 pos)
    {
        Vector2Int posV2 = (Vector2Int)wallTilemap.WorldToCell(pos);

        Debug.Log("pos:" + pos + " translated to: " + posV2);
        int newX = posV2.x + offset.x;
        int newY = posV2.y + offset.y;



        if (GetWith() > newX && GetHight() > newY)
        {
            NodeBase test = GetNode(newX, newY);
            //if (GetNode(newX, newY).CanWalkOver) return true;
            if (test.CanWalkOver) 
            {
                return true;
            }
            Debug.Log("Tilemap has tile there: "  + wallTilemap.HasTile((Vector3Int)test.OldPosition));
            Debug.Log("Tile name: " + wallTilemap.GetTile((Vector3Int)test.OldPosition).name);
            //wallTilemap.SetTile((Vector3Int)test.OldPosition, DEBUGG_TILE);
        }

        return false;
    }
    private NodeBase GetLowestFCostNode(List<NodeBase> nodeList)
    {
        NodeBase lowest = nodeList[0];
        for (int i = 1; i < nodeList.Count; i++)
        {
            if (nodeList[i].F < lowest.F)
            {
                lowest = nodeList[i];
            }
        }
        return lowest;

    }

    public NodeBase GetNode(Vector2Int pos)
    {
        return NodeBase[pos.x, pos.y];
    }

    public NodeBase GetNode(int x, int y)
    {
        return NodeBase[x, y];
    }

    public int GetHight()
    {
        return NodeBase.GetLength(1);
    }
    public int GetWith()
    {
        return NodeBase.GetLength(0);
    }
}

public class NodeBase
{
    public Vector2Int Position { get; set; }
    public Vector2Int OldPosition { get; set; }
    public NodeBase PreviousNode { get; set; }
    public int GCost { get; set; }
    public int HCost { get; set; }
    public int F => GCost + HCost;

    public bool CanWalkOver;

    public Vector2 GetCorrectPosition()
    {
        return new Vector2(OldPosition.x + 0.7f, OldPosition.y + 0.7f);
    }
}

