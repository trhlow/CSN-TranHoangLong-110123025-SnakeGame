using System.Collections.Generic;
using UnityEngine;

public class GridManager : Singleton<GridManager>
{
    [Header("Grid Configuration")]
    [SerializeField] private int gridWidth = 40;
    [SerializeField] private int gridHeight = 30;
    [SerializeField] private float cellSize = 0.5f;

    [Header("Visual Settings")]
    [SerializeField] private bool showGrid = true;
    [SerializeField] private Color gridColor = new Color(0f, 1f, 1f, 0.3f);
    [SerializeField] private Color borderColor = Color.white;

    private HashSet<Vector2Int> occupiedCells = new HashSet<Vector2Int>();
    private Dictionary<Vector2Int, GameObject> cellContents = new Dictionary<Vector2Int, GameObject>();

    private Vector3 gridOrigin;
    private Vector3 gridCenter;

    public int GridWidth => gridWidth;
    public int GridHeight => gridHeight;
    public float CellSize => cellSize;
    public Vector3 GridOrigin => gridOrigin;
    public Vector3 GridCenter => gridCenter;

    protected override void Awake()
    {
        base.Awake();
        InitializeGrid();
    }

    private void InitializeGrid()
    {
        float totalWidth = gridWidth * cellSize;
        float totalHeight = gridHeight * cellSize;

        gridOrigin = new Vector3(-totalWidth * 0.5f, -totalHeight * 0.5f, 0);
        gridCenter = new Vector3(0, 0, 0);

        Debug.Log($"[GridManager] Khởi tạo lưới {gridWidth}x{gridHeight}, kích thước ô:  {cellSize}");
    }

    public Vector2Int WorldToGrid(Vector3 worldPosition)
    {
        Vector3 localPos = worldPosition - gridOrigin;
        int x = Mathf.FloorToInt(localPos.x / cellSize);
        int y = Mathf.FloorToInt(localPos.y / cellSize);
        return new Vector2Int(x, y);
    }

    public Vector3 GridToWorld(Vector2Int gridPosition)
    {
        float x = gridOrigin.x + (gridPosition.x + 0.5f) * cellSize;
        float y = gridOrigin.y + (gridPosition.y + 0.5f) * cellSize;
        return new Vector3(x, y, 0);
    }

    public bool IsValidPosition(Vector2Int gridPosition)
    {
        return gridPosition.x >= 0 && gridPosition.x < gridWidth &&
               gridPosition.y >= 0 && gridPosition.y < gridHeight;
    }

    public bool IsOccupied(Vector2Int gridPosition)
    {
        return occupiedCells.Contains(gridPosition);
    }

    public void OccupyCell(Vector2Int gridPosition, GameObject occupant = null)
    {
        if (!IsValidPosition(gridPosition))
            return;

        occupiedCells.Add(gridPosition);

        if (occupant != null)
        {
            cellContents[gridPosition] = occupant;
        }
    }

    public void FreeCell(Vector2Int gridPosition)
    {
        occupiedCells.Remove(gridPosition);
        cellContents.Remove(gridPosition);
    }

    public GameObject GetOccupant(Vector2Int gridPosition)
    {
        return cellContents.ContainsKey(gridPosition) ? cellContents[gridPosition] : null;
    }

    public void ClearAllCells()
    {
        occupiedCells.Clear();
        cellContents.Clear();
    }

    public Vector2Int GetRandomEmptyCell()
    {
        int maxAttempts = 100;
        int attempts = 0;

        while (attempts < maxAttempts)
        {
            int x = Random.Range(0, gridWidth);
            int y = Random.Range(0, gridHeight);
            Vector2Int pos = new Vector2Int(x, y);

            if (!IsOccupied(pos))
            {
                return pos;
            }

            attempts++;
        }

        return new Vector2Int(gridWidth / 2, gridHeight / 2);
    }

    public List<Vector2Int> GetNeighbors(Vector2Int gridPosition)
    {
        List<Vector2Int> neighbors = new List<Vector2Int>();

        Vector2Int[] directions = {
            Vector2Int.up,
            Vector2Int.down,
            Vector2Int.left,
            Vector2Int.right
        };

        foreach (Vector2Int dir in directions)
        {
            Vector2Int neighbor = gridPosition + dir;
            if (IsValidPosition(neighbor))
            {
                neighbors.Add(neighbor);
            }
        }

        return neighbors;
    }

    public int GetManhattanDistance(Vector2Int from, Vector2Int to)
    {
        return Mathf.Abs(from.x - to.x) + Mathf.Abs(from.y - to.y);
    }

    private void OnDrawGizmos()
    {
        if (!showGrid || !Application.isPlaying)
            return;

        Gizmos.color = gridColor;

        for (int x = 0; x <= gridWidth; x++)
        {
            Vector3 start = GridToWorld(new Vector2Int(x, 0));
            Vector3 end = GridToWorld(new Vector2Int(x, gridHeight));
            start.y -= cellSize * 0.5f;
            end.y += cellSize * 0.5f;
            Gizmos.DrawLine(start, end);
        }

        for (int y = 0; y <= gridHeight; y++)
        {
            Vector3 start = GridToWorld(new Vector2Int(0, y));
            Vector3 end = GridToWorld(new Vector2Int(gridWidth, y));
            start.x -= cellSize * 0.5f;
            end.x += cellSize * 0.5f;
            Gizmos.DrawLine(start, end);
        }

        Gizmos.color = borderColor;
        Vector3 bottomLeft = gridOrigin;
        Vector3 bottomRight = gridOrigin + new Vector3(gridWidth * cellSize, 0, 0);
        Vector3 topLeft = gridOrigin + new Vector3(0, gridHeight * cellSize, 0);
        Vector3 topRight = gridOrigin + new Vector3(gridWidth * cellSize, gridHeight * cellSize, 0);

        Gizmos.DrawLine(bottomLeft, bottomRight);
        Gizmos.DrawLine(bottomRight, topRight);
        Gizmos.DrawLine(topRight, topLeft);
        Gizmos.DrawLine(topLeft, bottomLeft);
    }
}