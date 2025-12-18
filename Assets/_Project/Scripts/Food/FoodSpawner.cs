using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FoodSpawner : Singleton<FoodSpawner>
{
    [Header("Food Prefabs")]
    [SerializeField] private GameObject commonFoodPrefab;
    [SerializeField] private GameObject rareFoodPrefab;
    [SerializeField] private GameObject epicFoodPrefab;

    [Header("Spawn Configuration")]
    [SerializeField] private int maxFoodCount = 5;
    [SerializeField] private float spawnInterval = 2f;
    [SerializeField] private bool autoSpawn = true;

    [Header("Spawn Probabilities")]
    [Range(0f, 1f)][SerializeField] private float commonChance = 0.70f;
    [Range(0f, 1f)][SerializeField] private float rareChance = 0.25f;
    [Range(0f, 1f)][SerializeField] private float epicChance = 0.05f;

    [Header("Advanced Settings")]
    [SerializeField] private int minDistanceFromSnakes = 1;

    [Header("Visual Feedback")]
    [SerializeField] private GameObject spawnEffectPrefab;

    private List<GameObject> activeFoods = new List<GameObject>();
    private Coroutine autoSpawnCoroutine;

    private void Start()
    {
        if (autoSpawn)
        {
            StartAutoSpawn();
        }
    }

    public void StartAutoSpawn()
    {
        if (autoSpawnCoroutine != null)
        {
            StopCoroutine(autoSpawnCoroutine);
        }
        autoSpawnCoroutine = StartCoroutine(AutoSpawnCoroutine());
    }

    public void StopAutoSpawn()
    {
        if (autoSpawnCoroutine != null)
        {
            StopCoroutine(autoSpawnCoroutine);
            autoSpawnCoroutine = null;
        }
    }

    private IEnumerator AutoSpawnCoroutine()
    {
        while (true)
        {
            yield return new WaitForSeconds(spawnInterval);

            // ✅ FIX: Chỉ spawn nếu còn slot
            if (activeFoods.Count < maxFoodCount)
            {
                SpawnRandomFood();
            }
        }
    }

    public GameObject SpawnRandomFood()
    {
        GameObject foodPrefab = SelectFoodPrefab();
        return SpawnFood(foodPrefab);
    }

    public GameObject SpawnFood(GameObject foodPrefab)
    {
        if (foodPrefab == null || GridManager.Instance == null)
            return null;

        Vector2Int spawnPos = FindValidSpawnPosition();

        // ✅ FIX: Check nếu không tìm thấy vị trí hợp lệ
        if (spawnPos == Vector2Int.zero && GridManager.Instance.IsOccupied(spawnPos))
        {
            Debug.LogWarning("[FoodSpawner] Không tìm thấy vị trí spawn hợp lệ!");
            return null;
        }

        Vector3 worldPos = GridManager.Instance.GridToWorld(spawnPos);
        GameObject food = Instantiate(foodPrefab, worldPos, Quaternion.identity, transform);

        GridManager.Instance.OccupyCell(spawnPos, food);

        FoodAnimation anim = food.GetComponent<FoodAnimation>();
        if (anim != null)
        {
            anim.UpdateStartPosition(worldPos);
        }

        activeFoods.Add(food);

        if (spawnEffectPrefab != null)
        {
            Instantiate(spawnEffectPrefab, worldPos, Quaternion.identity);
        }

        return food;
    }

    public void SpawnInitialFoods(int count)
    {
        for (int i = 0; i < count; i++)
        {
            if (activeFoods.Count >= maxFoodCount)
                break;

            SpawnRandomFood();
        }
    }

    private GameObject SelectFoodPrefab()
    {
        float random = Random.value;

        if (random <= epicChance && epicFoodPrefab != null)
        {
            return epicFoodPrefab;
        }
        else if (random <= epicChance + rareChance && rareFoodPrefab != null)
        {
            return rareFoodPrefab;
        }
        else if (commonFoodPrefab != null)
        {
            return commonFoodPrefab;
        }

        return commonFoodPrefab;
    }

    // ✅ FIX: Improved spawn position finding
    private Vector2Int FindValidSpawnPosition()
    {
        int maxAttempts = 100;
        int attempts = 0;

        while (attempts < maxAttempts)
        {
            int x = Random.Range(0, GridManager.Instance.GridWidth);
            int y = Random.Range(0, GridManager.Instance.GridHeight);
            Vector2Int pos = new Vector2Int(x, y);

            if (IsValidSpawnPosition(pos))
            {
                return pos;
            }

            attempts++;
        }

        // ✅ Fallback: Tìm bất kỳ ô trống nào
        return GridManager.Instance.GetRandomEmptyCell();
    }

    // ✅ FIX: Check cả snake positions
    private bool IsValidSpawnPosition(Vector2Int pos)
    {
        // Check grid bounds
        if (!GridManager.Instance.IsValidPosition(pos))
            return false;

        // Check if cell is occupied
        if (GridManager.Instance.IsOccupied(pos))
            return false;

        // ✅ FIX: Check khoảng cách với TẤT CẢ snakes
        if (GameManager.Instance != null)
        {
            List<SnakeController> allSnakes = GameManager.Instance.GetAllSnakes();

            foreach (SnakeController snake in allSnakes)
            {
                if (snake == null || snake.IsDead)
                    continue;

                // ✅ QUAN TRỌNG: Check tất cả segments của snake
                List<Vector2Int> segmentPositions = snake.SegmentPositions;

                foreach (Vector2Int segPos in segmentPositions)
                {
                    int distance = GridManager.Instance.GetManhattanDistance(pos, segPos);

                    // ✅ Không spawn gần snake
                    if (distance < minDistanceFromSnakes)
                    {
                        Debug.Log($"[FoodSpawner] Position {pos} too close to snake segment at {segPos} (distance: {distance})");
                        return false;
                    }
                }
            }
        }

        return true;
    }

    public void RemoveFood(GameObject food)
    {
        if (food == null) return;

        Vector2Int gridPos = GridManager.Instance.WorldToGrid(food.transform.position);
        GridManager.Instance.FreeCell(gridPos);

        activeFoods.Remove(food);
    }

    public void ClearAllFood()
    {
        foreach (GameObject food in activeFoods)
        {
            if (food != null)
            {
                Vector2Int gridPos = GridManager.Instance.WorldToGrid(food.transform.position);
                GridManager.Instance.FreeCell(gridPos);
                Destroy(food);
            }
        }

        activeFoods.Clear();
    }

    public int GetActiveFoodCount()
    {
        // ✅ FIX: Clean up null references
        activeFoods.RemoveAll(food => food == null);
        return activeFoods.Count;
    }

    public void AdjustDifficulty(int playerScore)
    {
        if (playerScore > 100)
        {
            maxFoodCount = 6;
            spawnInterval = 1.5f;
        }
        if (playerScore > 200)
        {
            maxFoodCount = 7;
            spawnInterval = 1.2f;
        }
    }

    public List<GameObject> GetAllFoods()
    {
        activeFoods.RemoveAll(food => food == null);
        return new List<GameObject>(activeFoods);
    }

    public GameObject GetNearestFood(Vector2Int position)
    {
        GameObject nearest = null;
        int minDistance = int.MaxValue;

        foreach (GameObject food in activeFoods)
        {
            if (food == null) continue;

            Vector2Int foodPos = GridManager.Instance.WorldToGrid(food.transform.position);
            int distance = GridManager.Instance.GetManhattanDistance(position, foodPos);

            if (distance < minDistance)
            {
                minDistance = distance;
                nearest = food;
            }
        }

        return nearest;
    }
}