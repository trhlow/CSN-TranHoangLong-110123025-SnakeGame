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

        if (collision.CompareTag("Food"))
        {
            Debug.Log($"[Head] Hit food: {collision.gameObject.name}");
            
            // Gọi method trong SnakeController
            snake.OnHeadHitFood(collision);
        }
        else if (collision.CompareTag("SnakeBody") || collision.CompareTag("SnakeHead"))
        {
            Debug.Log($"[Head] Hit snake: {collision.gameObject.name}");
            
            // Gọi method trong SnakeController
            snake.OnHeadHitSnake(collision);
        }
    }
}