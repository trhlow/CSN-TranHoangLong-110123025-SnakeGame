using UnityEngine;

public class WallSpawner : MonoBehaviour
{
    public GameObject wallPrefab;
    public int gridWidth = 30;
    public int gridHeight = 17;
    public float cellSize = 1.0f;

    void Start()
    {
        SpawnWalls();
    }

    void SpawnWalls()
    {
        // Spawn viền trên cùng và dưới cùng
        for (int x = -1; x <= gridWidth; x++)
        {
            Instantiate(wallPrefab, new Vector3(x * cellSize, gridHeight * cellSize, 0), Quaternion.identity, transform); // Top
            Instantiate(wallPrefab, new Vector3(x * cellSize, -1 * cellSize, 0), Quaternion.identity, transform);       // Bottom
        }

        // Spawn viền trái và phải
        for (int y = 0; y < gridHeight; y++)
        {
            Instantiate(wallPrefab, new Vector3(-1 * cellSize, y * cellSize, 0), Quaternion.identity, transform);           // Left
            Instantiate(wallPrefab, new Vector3(gridWidth * cellSize, y * cellSize, 0), Quaternion.identity, transform);    // Right
        }
    }
}