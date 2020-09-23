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


    public Vector2 _destination;
    public Vector2 destination
    {
        get { return _destination; }
        set {
            if (_destination != value)
            {
                pathComplete = false;
                pathFound = false;
            }
            _destination = value; 
        }
    }
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
        public PathNode parent;

        public PathNode(Vector2 position, Vector2 worldPosition)
        {
            node.MapPosition = position;
            node.WorldPosition = worldPosition;
        }

        public PathNode(Node me, PathNode parent)
        {
            node = me;
            this.parent = parent;
        }
    }

    public static Node[,] bakedMap;
    public bool baked = false;
    private Node[,] searchMap;

    private LinkedList<PathNode> nodePath;
    private bool pathFound = false, pathComplete = false;

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

        debugLabels = new List<DebugLabel>();
    }

    // Update is called once per frame
    void Update()
    {
        velocity = r_body.velocity;

        if(oldDestination != destination && !pathFound)
        {
            //we got a new location, let's calculate it.
            searchMap = new Node[width, height];
            //Array.Copy(bakedMap, searchMap, bakedMap.Length);
            RePath(transform.position, 0);
        }
        else if(oldDestination != destination && pathComplete)
        {
            if (nodePath.Count != 0)
            {
                if(currentDestination == null)
                {
                    currentDestination = nodePath.First.Value;
                }
                if (Vector2.Distance(transform.position, currentDestination.node.WorldPosition) > stoppingDistance)
                {
                    Vector2 move_dir = (Vector3)currentDestination.node.WorldPosition - transform.position;
                    r_body.velocity = move_dir.normalized * _speed;
                }
                else
                {
                    nodePath.Remove(currentDestination);
                    currentDestination = null;
                    r_body.velocity = Vector2.zero;
                }
            }
            else
            {
                Vector2 move_dir = (Vector3)destination - transform.position;
                if (Vector2.Distance(transform.position, destination) > stoppingDistance)
                {
                    r_body.velocity = move_dir.normalized * _speed;
                }
                else
                {
                    r_body.velocity = Vector2.zero;
                }
            }
        }
    }

    private void RePath(Vector2 position, int movecost)
    {
        int convX = width / 2 + offsetMap.x;
        int convY = height / 2 + offsetMap.y;

        Vector2 convPos = new Vector2(convX, convY);

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
                    aSquare.node.nodeCost += movecost;
                    aSquare.node.nodeToGoalCost = ManhattanDistance(position, destination);
                    aSquare.node.F = aSquare.node.nodeCost + aSquare.node.nodeToGoalCost;
                    aSquare.parent = currentSquare;
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
                        aSquare.parent = currentSquare;
                        ReplaceNode(openList, aSquare);
                    }
                }
            }

            movecost++;
        } while (openList.Count != 0);

       
        int count = 0;
        debugLabels.Clear();
        /*foreach(PathNode n in closedList)
        {
            Vector2Int pos = Vector2Int.RoundToInt(n.node.WorldPosition);
            int x = pos.x;
            int y = pos.y;
            tiles[0].SetTile(new Vector3Int((int)n.parent.WorldPosition.x, (int)n.parent.WorldPosition.y,0), FreeTile);
            debugLabels.Add(new DebugLabel(pos, count.ToString()));
            //debugLabels.Add(new DebugLabel(pos - new Vector2(0,0.2f), n.parent.WorldPosition.ToString()));
            count++;
        }*/

        PathNode nnode = closedList.Last.Value;

        do
        {
            /*PathNode min = nnode;
            foreach(PathNode n in closedList)
            {
                if(n.node.WorldPosition == nnode.node.WorldPosition)
                {
                    int nodeCost = count;
                    int nodeToGoalCost = ManhattanDistance(n.node.WorldPosition, transform.position);
                    int F = nodeCost + nodeToGoalCost;
                    if (F < min.node.F)
                    {
                        min = n;
                    }
                }
            }
            nnode = min;*/
            
            Vector2Int pos = Vector2Int.RoundToInt(nnode.node.WorldPosition);
            int x = pos.x;
            int y = pos.y;
            tiles[0].SetTile(new Vector3Int((int)nnode.node.WorldPosition.x, (int)nnode.node.WorldPosition.y, 0), FreeTile);
            debugLabels.Add(new DebugLabel(pos, count.ToString() + ":" + nnode.node.F));
            count++;
            nodePath.AddFirst(nnode);
            nnode = nnode.parent;
        } while (nnode != null);

        pathComplete = true;
        pathFound = true;
        print("pathing completed");

    }

    private void ReplaceNode(List<PathNode> list, PathNode aSquare)
    {
        foreach (PathNode n in list)
        {
            if (Vector2Int.RoundToInt(n.node.MapPosition) == Vector2Int.RoundToInt(aSquare.node.MapPosition))
            {
                list.Remove(n);
                list.Add(aSquare);
            }

        }
    }

    public class DebugLabel
    {
        public Vector3 position;
        public string output;

        public DebugLabel(Vector2 pos, string str)
        {
            position = pos;
            output = str;
        }
    }
    List<DebugLabel> debugLabels;

    void OnGUI()
    {
        foreach(DebugLabel d in debugLabels)
        {
            //Handles.Label(d.position, d.output);
        }
    }

    private bool SearchForNode(LinkedList<PathNode> list, PathNode aSquare)
    {
        if (list.Count != 0)
        foreach (PathNode n in list)
        {
            if (Vector2Int.RoundToInt(n.node.MapPosition) == Vector2Int.RoundToInt(aSquare.node.MapPosition)) 
                return true;
        }
        return false;
    }

    private bool SearchForNode(List<PathNode> list, PathNode aSquare)
    {
        if(list.Count != 0)
        foreach (PathNode n in list)
        {
            if (Vector2Int.RoundToInt(n.node.MapPosition) == Vector2Int.RoundToInt(aSquare.node.MapPosition)) 
                return true;
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

        
        if (bakedMap[startx-1, starty].closed == false)
        {
            neighbors.Add(new PathNode(bakedMap[startx - 1, starty], node)); 
        }

        if (bakedMap[startx + 1, starty].closed == false)
        {
            neighbors.Add(new PathNode(bakedMap[startx + 1, starty], node));
        }

        if (bakedMap[startx , starty - 1].closed == false)
        {
            neighbors.Add(new PathNode(bakedMap[startx, starty - 1], node));
        }

        if (bakedMap[startx , starty + 1].closed == false)
        {
            neighbors.Add(new PathNode(bakedMap[startx, starty + 1], node));
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
                print("Fnode: " + F);
                if (minCost > F)
                {
                    minCost = F;
                    minNode = n;
                }
                if (minCost == F)
                {
                    //minNode = n;
                }
            }
            else
            {
                tiles[0].SetTile(new Vector3Int(x - convX, y - convY, 0), ClosedTile);
            }
        }
        print("minCost: " + minCost);
        return minNode;
    }

    public Tile ClosedTile;
    public Tile FreeTile;
    private PathNode currentDestination;

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
                        t.SetTile(new Vector3Int(x - convX, y - convY, 0), ClosedTile);
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
