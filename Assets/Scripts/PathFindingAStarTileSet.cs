using UnityEngine;
using System.Collections.Generic;
using UnityEngine.Tilemaps;
using UnityEngine.XR.WSA.Input;

public class PathFindingAStarTileSet : MonoBehaviour
{

	private const int OBSTACLE_ID = 0;
	private const float BASE_LINE_WIDTH = 3.0f;
	private readonly Color DRAW_COLOR = new Color(0, 0, 0);

	private Vector2 mapSize;

	private Vector2 CellSize;
	private Vector2 halfCellSize;
	private Vector2 pathStartPosition;
	private Vector2 pathEndPosition;
	private AStar2D aStarNode;
	private List<Vector2> cellPath;
	private List<Vector2> obstacles;

	private Tilemap tilemap;
	private Grid theGrid;


	private void Start()
	{
		this.tilemap = GameObject.Find("Walls").GetComponent<Tilemap>();
		this.CellSize = (Vector2Int)tilemap.cellBounds.size;
		this.halfCellSize = this.CellSize / 2;
		this.mapSize = new Vector2(16, 16);
		this.aStarNode = new AStar2D();
		this.cellPath = new List<Vector2>();
		this.obstacles = new List<Vector2>();

		this.theGrid = GameObject.Find("Grid").GetComponent<Grid>();

		GameObject[] obstaclesArray = GameObject.FindGameObjectsWithTag("NavigationObstacle");
		Tilemap[] maps = new Tilemap[obstaclesArray.Length];

		for (int i = 0; i < obstaclesArray.Length; i++)
		{
			maps[i] = obstaclesArray[i].GetComponent<Tilemap>();

			foreach(Vector3Int pos in maps[i].cellBounds.allPositionsWithin)
			{
				if(maps[i].GetTile(pos) != null)
				{
					this.obstacles.Add((Vector3)pos);
				}
			}
			
		}

		List<Vector2> walkableCells = CalculateAStarWalkableCells(this.obstacles);
		ConnectAStarWalkableCells(walkableCells);
	}

	public List<Vector2> GetPath(Vector2Int startCell, Vector2Int endCell)
	{
		ChangePathStartPosition(theGrid.CellToWorld((Vector3Int)startCell));
		ChangePathEndPosition((Vector2Int)theGrid.WorldToCell((Vector3Int)endCell));
		RecalculatePath();

		List<Vector2> pathWorld = new List<Vector2>();
		foreach (Vector2 cell in this.cellPath)
		{
			Vector2Int c = new Vector2Int((int)cell.x, (int)cell.y);
			Vector2 cellWorld = (Vector2)theGrid.CellToWorld(new Vector3Int(c.x, c.y,0)) + this.halfCellSize;
			pathWorld.Add(cellWorld);
		}

		return pathWorld;
	}

	private List<Vector2> CalculateAStarWalkableCells(List<Vector2> obstacleCells)
	{
		List<Vector2> walkableCells = new List<Vector2>();
		for (int y = 0; y < this.mapSize.y; y++)
		{
			for (int x = 0; x < this.mapSize.x; x++)
			{
				Vector2 cell = new Vector2(x, y);

				if (!obstacleCells.Contains(cell))
				{
					walkableCells.Add(cell);

					int cellIndex = CalculateCellIndex(cell);
					aStarNode.AddPoint(cellIndex, new Vector2(cell.x, cell.y));
				}
			}
		}

		return walkableCells;
	}

	private void ConnectAStarWalkableCells(List<Vector2> walkableCells)
	{
		foreach (Vector2 cell in walkableCells)
		{
			int cellIndex = CalculateCellIndex(cell);

			List<Vector2> neighborCells = new List<Vector2>();
			neighborCells.Add(new Vector2(cell.x + 1, cell.y));
			neighborCells.Add(new Vector2(cell.x - 1, cell.y));
			neighborCells.Add(new Vector2(cell.x, cell.y + 1));
			neighborCells.Add(new Vector2(cell.x, cell.y - 1));

			foreach (Vector2 neighborCell in neighborCells)
			{
				int neighborCellIndex = CalculateCellIndex(neighborCell);

				if (!IsCellOutsideMapBounds(neighborCell) && this.aStarNode.HasPoint(neighborCellIndex))
				{
					this.aStarNode.ConnectPoints(cellIndex, neighborCellIndex, false);
				}
			}
		}
	}

	private void ClearPreviousPathDrawing()
	{
		if (this.cellPath != null && this.cellPath.Count != 0)
		{
			Vector2 startCell = this.cellPath[0];
			Vector2 endCell = this.cellPath[this.cellPath.Count - 1];

		}
	}

	private void RecalculatePath()
	{
		ClearPreviousPathDrawing();
		int startCellIndex = CalculateCellIndex(this.pathStartPosition);
		int endCellIndex = CalculateCellIndex(this.pathEndPosition);

		this.cellPath.Clear();
		Vector2[] cellPathArray = this.aStarNode.GetPointPath(startCellIndex, endCellIndex);
		for (int i = 0; i < cellPathArray.Length; i++)
		{
			this.cellPath.Add(cellPathArray[i]);
		}
	}

	private void ChangePathStartPosition(Vector2 newPathStartPosition)
	{
		if (!this.obstacles.Contains(newPathStartPosition) && !IsCellOutsideMapBounds(newPathStartPosition))
		{
			this.pathStartPosition = newPathStartPosition;

			if (this.pathEndPosition == null && !this.pathEndPosition.Equals(this.pathStartPosition))
			{
				RecalculatePath();
			}
		}
	}

	private void ChangePathEndPosition(Vector2 newPathEndPosition)
	{
		if (!this.obstacles.Contains(newPathEndPosition) && !IsCellOutsideMapBounds(newPathEndPosition))
		{
			this.pathEndPosition = newPathEndPosition;

			if (!this.pathStartPosition.Equals(newPathEndPosition))
			{
				RecalculatePath();
			}
		}
	}

	private int CalculateCellIndex(Vector2 cell)
	{
		return (int)(cell.x + this.mapSize.x * cell.y);
	}

	private bool IsCellOutsideMapBounds(Vector2 cell)
	{
		return cell.x < 0 || cell.y < 0 || cell.x >= this.mapSize.x || cell.y >= this.mapSize.y;
	}

}
