using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Manages the 40x30 grid system for the Snake game
/// Handles coordinate conversion, collision detection, and cell management
/// </summary>
public class GridManager : Singleton<GridManager>
{
    [Header("Grid Configuration")]
    [SerializeField] private int gridWidth = 40;
    [SerializeField] private int gridHeight = 30;
    [SerializeField] private float cellSize = 0.5f;

    [Header("Play Area (for snake spawning)")]
    [SerializeField] private int playMinX = 0;
    [SerializeField] private int playMaxX = 0;
    [SerializeField] private int playMinY = 0;
    [SerializeField] private int playMaxY = 0;

    [Header("Visual Settings")]
    [SerializeField] private bool showGrid = true;
    [SerializeField] private Color gridColor = new Color(0f, 1f, 1f, 0.3f);
    [SerializeField] private Color borderColor = Color.white;
    [SerializeField] private float gridLineWidth = 0.01f;

    // Grid state management
    private HashSet<Vector2Int> occupiedCells = new HashSet<Vector2Int>();
    private Dictionary<Vector2Int, GameObject> cellContents = new Dictionary<Vector2Int, GameObject>();

    private Vector3 gridOrigin;
    private Vector3 gridCenter;

    // Public properties
    public int GridWidth => gridWidth;
    public int GridHeight => gridHeight;
    public float CellSize => cellSize;
    public Vector3 GridOrigin => gridOrigin;
    public Vector3 GridCenter => gridCenter;
    public RectInt PlayArea => new RectInt(playMinX, playMinY, playMaxX - playMinX + 1, playMaxY - playMinY + 1);

    protected override void Awake()
    {
        base.Awake();
        InitializeGrid();
    }

    private void InitializeGrid()
    {
        // Calculate grid positioning (centered at world origin)
        float totalWidth = gridWidth * cellSize;
        float totalHeight = gridHeight * cellSize;

        gridOrigin = new Vector3(-totalWidth * 0.5f, -totalHeight * 0.5f, 0);
        gridCenter = Vector3.zero;

        Debug.Log($"[GridManager] ✅ Initialized {gridWidth}x{gridHeight} grid, cell size: {cellSize}");
    }

    #region Coordinate Conversion
    /// <summary>
    /// Converts world position to grid coordinates
    /// </summary>
    public Vector2Int WorldToGrid(Vector3 worldPosition)
    {
        Vector3 localPos = worldPosition - gridOrigin;
        int x = Mathf.FloorToInt(localPos.x / cellSize);
        int y = Mathf.FloorToInt(localPos.y / cellSize);
        return new Vector2Int(x, y);
    }

    /// <summary>
    /// Converts grid coordinates to world position (center of cell)
    /// </summary>
    public Vector3 GridToWorld(Vector2Int gridPosition)
    {
        float x = gridOrigin.x + (gridPosition.x + 0.5f) * cellSize;
        float y = gridOrigin.y + (gridPosition.y + 0.5f) * cellSize;
        return new Vector3(x, y, 0);
    }
    #endregion

    #region Position Validation
    /// <summary>
    /// Check if grid position is within bounds
    /// </summary>
    public bool IsValidPosition(Vector2Int gridPosition)
    {
        return gridPosition.x >= 0 && gridPosition.x < gridWidth &&
               gridPosition.y >= 0 && gridPosition.y < gridHeight;
    }

    /// <summary>
    /// Check if position is within the play area
    /// </summary>
    public bool IsInPlayArea(Vector2Int gridPosition)
    {
        return gridPosition.x >= playMinX && gridPosition.x <= playMaxX &&
               gridPosition.y >= playMinY && gridPosition.y <= playMaxY;
    }
    #endregion

    #region Cell Management
    /// <summary>
    /// Check if a cell is occupied
    /// </summary>
    public bool IsOccupied(Vector2Int gridPosition)
    {
        return occupiedCells.Contains(gridPosition);
    }

    /// <summary>
    /// Mark a cell as occupied
    /// </summary>
    public void OccupyCell(Vector2Int gridPosition, GameObject occupant = null)
    {
        if (!IsValidPosition(gridPosition))
        {
            Debug.LogWarning($"[GridManager] Cannot occupy invalid position: {gridPosition}");
            return;
        }

        occupiedCells.Add(gridPosition);

        if (occupant != null)
        {
            cellContents[gridPosition] = occupant;
        }
    }

    /// <summary>
    /// Free a cell
    /// </summary>
    public void FreeCell(Vector2Int gridPosition)
    {
        occupiedCells.Remove(gridPosition);
        cellContents.Remove(gridPosition);
    }

    /// <summary>
    /// Get the occupant of a cell
    /// </summary>
    public GameObject GetOccupant(Vector2Int gridPosition)
    {
        return cellContents.ContainsKey(gridPosition) ? cellContents[gridPosition] : null;
    }

    /// <summary>
    /// Clear all occupied cells
    /// </summary>
    public void ClearAllCells()
    {
        occupiedCells.Clear();
        cellContents.Clear();
        Debug.Log("[GridManager] All cells cleared");
    }
    #endregion

    #region Pathfinding & Utilities
    /// <summary>
    /// Get random empty cell within grid
    /// </summary>
 
    /// <summary>
    /// Get all valid neighbors (up, down, left, right)
    /// </summary>
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

    /// <summary>
    /// Calculate Manhattan distance between two grid positions
    /// </summary>
    public int GetManhattanDistance(Vector2Int from, Vector2Int to)
    {
        return Mathf.Abs(from.x - to.x) + Mathf.Abs(from.y - to.y);
    }

    public Vector2Int GetRandomEmptyCell()
    {
        List<Vector2Int> emptyCells = new List<Vector2Int>();

        for (int x = 0; x < GridWidth; x++)
        {
            for (int y = 0; y < GridHeight; y++)
            {
                Vector2Int pos = new Vector2Int(x, y);
                if (!IsOccupied(pos))
                {
                    emptyCells.Add(pos);
                }
            }
        }

        return emptyCells.Count > 0 ?
               emptyCells[Random.Range(0, emptyCells.Count)] :
               new Vector2Int(-1, -1);
    }
    #endregion

    #region Debug Visualization
    private void OnDrawGizmos()
    {
        if (!showGrid || !Application.isPlaying)
            return;

        // Draw grid lines
        Gizmos.color = gridColor;

        // Vertical lines
        for (int x = 0; x <= gridWidth; x++)
        {
            Vector3 start = GridToWorld(new Vector2Int(x, 0));
            Vector3 end = GridToWorld(new Vector2Int(x, gridHeight));
            start.y -= cellSize * 0.5f;
            end.y += cellSize * 0.5f;
            Gizmos.DrawLine(start, end);
        }

        // Horizontal lines
        for (int y = 0; y <= gridHeight; y++)
        {
            Vector3 start = GridToWorld(new Vector2Int(0, y));
            Vector3 end = GridToWorld(new Vector2Int(gridWidth, y));
            start.x -= cellSize * 0.5f;
            end.x += cellSize * 0.5f;
            Gizmos.DrawLine(start, end);
        }

        // Draw border
        Gizmos.color = borderColor;
        Vector3 bottomLeft = gridOrigin;
        Vector3 bottomRight = gridOrigin + new Vector3(gridWidth * cellSize, 0, 0);
        Vector3 topLeft = gridOrigin + new Vector3(0, gridHeight * cellSize, 0);
        Vector3 topRight = gridOrigin + new Vector3(gridWidth * cellSize, gridHeight * cellSize, 0);

        Gizmos.DrawLine(bottomLeft, bottomRight);
        Gizmos.DrawLine(bottomRight, topRight);
        Gizmos.DrawLine(topRight, topLeft);
        Gizmos.DrawLine(topLeft, bottomLeft);

        // Draw play area
        Gizmos.color = Color.yellow;
        Vector3 playBL = GridToWorld(new Vector2Int(playMinX, playMinY));
        Vector3 playBR = GridToWorld(new Vector2Int(playMaxX, playMinY));
        Vector3 playTL = GridToWorld(new Vector2Int(playMinX, playMaxY));
        Vector3 playTR = GridToWorld(new Vector2Int(playMaxX, playMaxY));

        playBL -= new Vector3(cellSize * 0.5f, cellSize * 0.5f, 0);
        playBR += new Vector3(cellSize * 0.5f, -cellSize * 0.5f, 0);
        playTL += new Vector3(-cellSize * 0.5f, cellSize * 0.5f, 0);
        playTR += new Vector3(cellSize * 0.5f, cellSize * 0.5f, 0);

        Gizmos.DrawLine(playBL, playBR);
        Gizmos.DrawLine(playBR, playTR);
        Gizmos.DrawLine(playTR, playTL);
        Gizmos.DrawLine(playTL, playBL);
    }
    #endregion
}