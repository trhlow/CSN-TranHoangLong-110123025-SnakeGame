using UnityEngine;

/// <summary>
/// Xử lý collision cho Head của snake
/// </summary>
public class SnakeHeadCollision : MonoBehaviour
{
    private SnakeController snake;

    public void SetSnake(SnakeController snakeController)
    {
        snake = snakeController;
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (snake == null || snake.IsDead)
            return;

        // ✅ FIX: Gọi OnSegmentTriggerEnter thay vì OnHeadHitFood/OnHeadHitSnake
        snake.OnSegmentTriggerEnter(collision, gameObject);
    }
}