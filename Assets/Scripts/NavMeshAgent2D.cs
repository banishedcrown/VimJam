using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using UnityEditor;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.EventSystems;
using UnityEngine.Tilemaps;

[RequireComponent(typeof(Rigidbody2D))]
public class NavMeshAgent2D : MonoBehaviour
{

    [Header("Steering")]
    [SerializeField] float _speed = 3.5f;
    [SerializeField] float _angularSpeed = 120;
    [SerializeField] float _acceleration = 8;
    [SerializeField] float _stoppingDistance = 0.25f;
    [SerializeField] bool _autoBraking = true; // false is too weird, true by default.

    
    public Vector2 destination { get; set; }
    public Vector2 velocity { get; set; }

    public int width = 50, height = 50;
    public Vector2Int offsetMap = Vector2Int.zero;

    private Rigidbody2D r_body;
    private Vector2 oldDestination;
    public Tilemap[] tiles;

    public struct Node
    {
        public bool closed;
        public int F;
        public int nodeCost; //cost of traveling onto node
        public int nodeToGoalCost; //guessed cost of node to goal.
        public Vector2 MapPosition;
        public Vector2 WorldPosition;
    }

    public class PathNode
    {
        public Node node;
        public Node parent;

        public PathNode(Vector2 position, Vector2 worldPosition)
        {
            node.MapPosition = position;
            node.WorldPosition = worldPosition;
        }

        public PathNode(Node me, Node parent)
        {
            node = me;
            this.parent = parent;
        }
    }

    public static Node[,] bakedMap;
    public bool baked = false;
    private Node[,] searchMap;

    private LinkedList<PathNode> nodePath;

    // Start is called before the first frame update
    void Start()
    {
        r_body = GetComponent<Rigidbody2D>();
        destination = oldDestination = transform.position;

        if (tiles.Length == 0)
        {
            tiles = GameObject.FindObjectsOfType<Tilemap>();
        }

        if (!baked)
        {
            bakedMap = new Node[width, height];         
            BakeMap();
        }
    }

    // Update is called once per frame
    void Update()
    {
        velocity = r_body.velocity;

        if(oldDestination != destination)
        {
            //we got a new location, let's calculate it.
            searchMap = new Node[width, height];
            //Array.Copy(bakedMap, searchMap, bakedMap.Length);
            RePath(transform.position, 0);
        }

        

    }

    private void RePath(Vector2 position, int movecost)
    {
        int convX = width / 2 + offsetMap.x;
        int convY = height / 2 + offsetMap.y;

        Vector2 convPos = new Vector2(convX, convY);

        print("start position:" + position + "vs " + destination);

        Vector2Int origin = new Vector2Int(width / 2, height / 2);
        /*int convX = (int)position.x + origin.x;
        int convY = (int)position.y + origin.y;*/

        int startx = Mathf.RoundToInt(position.x + convX);
        int starty = Mathf.RoundToInt(position.y + convY);

        int minCost = int.MaxValue;
        Vector2 minNode = Vector2.zero;

        /*for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                
                if (!bakedMap[x, y].closed)
                {
                tiles[0].SetTile(new Vector3Int(x - convX, y - convY, 0), testTile);
                }
                else
                {
                tiles[0].SetTile(new Vector3Int(x - convX, y - convY, 0), testTile2);
                }
            }
        }

        return;*/
        /*for (int x = startx - 1; x <= startx + 1; x++) //let's check all 9 spots. 
        {
            for (int y = starty - 1; y <= starty + 1; y++)
            {
                *//*for (int x = 0; x < width; x++)
                {
                    for (int y = 0; y < height; y++)
                    {*//*
                if (!bakedMap[x, y].closed && !searchMap[x, y].closed)
                {
                    searchMap[x, y].nodeCost = (x == y? movecost +10 : movecost);
                    searchMap[x, y].nodeToGoalCost = ManhattanDistance(position, destination);
                    int F = searchMap[x, y].nodeCost + searchMap[x, y].nodeToGoalCost;
                    tiles[0].SetTile(new Vector3Int(x - convX, y - convY, 0), FreeTile);

                    if (minCost > F)
                    {
                        minCost = F;
                        minNode = new Vector2(x, y);
                        position = minNode - origin;
                        print("position:" + position + "vs " + destination);
                        navPath.Add(position);
                    }
                    if (minCost == F)
                    {
                        minNode = new Vector2(x, y);
                        position = minNode - origin;
                        navPath.Add(position);
                    }

                    
                }
                else
                {
                    tiles[0].SetTile(new Vector3Int( x - convX, y - convY, 0), ClosedTile);
                }

                if (searchMap[x, y].touched) return -1;
            }
        }*/

        List<PathNode> openList = new List<PathNode>();
        LinkedList<PathNode> closedList = new LinkedList<PathNode>();

        nodePath = new LinkedList<PathNode>();

        PathNode currentSquare = new PathNode(position+convPos, position);
        openList.Add(currentSquare);
        //openList.AddRange(GetAdjacentSquares(currentSquare));
        
        do
        {

            currentSquare = GetLowestSquare(openList, movecost);

            closedList.AddLast(currentSquare);
            openList.Remove(currentSquare);


            bool foundNode = false;
            foreach(PathNode n in closedList)
            {
                float dist = Vector2.Distance(n.node.WorldPosition, destination);
                if (dist < 1.25f)
                {
                    foundNode = true;
                    break;
                }
            }

            if (foundNode) break;


            List<PathNode> adjacentSquares = GetAdjacentSquares(currentSquare);

            foreach(PathNode aSquare in adjacentSquares)
            {
                if (SearchForNode(closedList,aSquare)) 
                    continue;
                if (!SearchForNode(openList,aSquare))
                {
                    aSquare.node.nodeCost = movecost;
                    aSquare.node.nodeToGoalCost = ManhattanDistance(position, destination);
                    aSquare.node.F = aSquare.node.nodeCost + aSquare.node.nodeToGoalCost;
                    aSquare.parent = currentSquare.node;
                    openList.Add(aSquare);
                }
                else
                {
                    int prevF = aSquare.node.F;
                    aSquare.node.nodeCost = movecost;
                    aSquare.node.nodeToGoalCost = ManhattanDistance(position, destination);
                    int F = aSquare.node.nodeCost + aSquare.node.nodeToGoalCost;
                    if (F < prevF)
                    {
                        aSquare.node.F = F;
                        aSquare.parent = currentSquare.node;
                    }
                }
            }

            movecost++;
        } while (openList.Count != 0);

        /*print("minNode:" + minNode);
        print("node Data: " + searchMap[(int)minNode.x, (int)minNode.y]);


       *//* position = minNode - origin;
        print("position:" + position + "vs " + destination);
        navPath.Add(position);*//*

        movecost++;

        if (position == destination) return 1;
        if(navPath.Count > 20) throw new Exception();
        else return RePath(position, movecost);*/
        foreach(PathNode n in closedList)
        {
            Vector2Int pos = Vector2Int.RoundToInt(n.node.WorldPosition);
            int x = pos.x;
            int y = pos.y;
            tiles[0].SetTile(new Vector3Int(x, y, 0), FreeTile);
        }
        print("pathing completed");

    }

    private bool SearchForNode(LinkedList<PathNode> list, PathNode aSquare)
    {
        foreach(PathNode n in list)
        {
            if (Vector2Int.RoundToInt(n.node.MapPosition) == Vector2Int.RoundToInt(aSquare.node.MapPosition)) return true;
        }
        return false;
    }

    private bool SearchForNode(List<PathNode> list, PathNode aSquare)
    {
        foreach (PathNode n in list)
        {
            if (Vector2Int.RoundToInt(n.node.MapPosition) == Vector2Int.RoundToInt(aSquare.node.MapPosition)) return true;
        }
        return false;
    }

    private List<PathNode> GetAdjacentSquares(PathNode node)
    {
        Vector3 position = node.node.WorldPosition;
        int convX = width / 2 + offsetMap.x;
        int convY = height / 2 + offsetMap.y;
        int startx = Mathf.RoundToInt(position.x + convX);
        int starty = Mathf.RoundToInt(position.y + convY);

        List<PathNode> neighbors = new List<PathNode>();

        for (int x = startx - 1; x <= startx + 1; x++) //let's check all 9 spots. 
        {
            for (int y = starty - 1; y <= starty + 1; y++)
            {

                if(bakedMap[x,y].closed == false)
                {
                    neighbors.Add(new PathNode(bakedMap[x,y],node.node));

                }
            }
        }

        return neighbors;
    }

    private PathNode GetLowestSquare(List<PathNode> openList, int cost)
    {
        int convX = width / 2 + offsetMap.x;
        int convY = height / 2 + offsetMap.y;
        Vector2Int origin = new Vector2Int(width / 2, height / 2);

        int minCost = int.MaxValue;
        PathNode minNode = null;

        foreach(PathNode n in openList)
        {
            Vector2 position = n.node.MapPosition;
            Vector2 worldPosition = n.node.WorldPosition;
            int x = Mathf.RoundToInt(position.x);
            int y = Mathf.RoundToInt(position.y);
            //if (!bakedMap[x, y].closed && !searchMap[x, y].closed)
            if(!n.node.closed)
            {
                /*n.node.nodeCost = cost;
                n.node.nodeToGoalCost = ManhattanDistance(worldPosition, destination);
                int F = n.node.nodeCost + n.node.nodeToGoalCost;*/
                int F = n.node.F;
                //tiles[0].SetTile(new Vector3Int(x - convX, y - convY, 0), FreeTile);

                if (minCost > F)
                {
                    minCost = F;
                    minNode = n;
                }
                if (minCost == F)
                {
                    minNode = n;
                }
            }
            else
            {
                tiles[0].SetTile(new Vector3Int(x - convX, y - convY, 0), ClosedTile);
            }
        }
        return minNode;
    }

    public Tile ClosedTile;
    public Tile FreeTile;
    private void BakeMap()
    {
        int convX = width / 2 + offsetMap.x;
        int convY = height / 2 + offsetMap.y;
        for(int x = 0; x < width; x++)
        {
            for(int y = 0; y < height; y++)
            {
                foreach(Tilemap t in tiles) if(t.GetComponent<TilemapCollider2D>())
                {
                    TileBase newTile = t.GetTile(new Vector3Int(x - convX, y - convY, 0));
                    if ( newTile != null)
                    {
                        bakedMap[x , y].closed = true;
                        //t.SetTile(new Vector3Int(x - convX, y - convY, 0), ClosedTile);
                    }
                    else
                    {
                        
                        bakedMap[x, y].closed = bakedMap[x, y].closed;
                        //t.SetTile(new Vector3Int(x - convX, y - convY, 0), FreeTile);
                    }
                }

                bakedMap[x, y].MapPosition = new Vector2(x, y);
                bakedMap[x, y].WorldPosition = new Vector2(x - convX, y - convY);
            }
        }
        baked = true;
    }

    public int ManhattanDistance(Vector2 a, Vector2 b)
    {
        checked
        {
            return (int) (Mathf.Abs(a.x - b.x) + Mathf.Abs(a.y - b.y));
        }
    }

    public float stoppingDistance
    {
        get { return _stoppingDistance; }
        set { _stoppingDistance = value; }
    }

    public float remainingDistance { get; } = 0;
}
