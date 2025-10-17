using UnityEngine;
using System.Collections.Generic;

public class Obstacle : MonoBehaviour
{
    [SerializeField] private Vector2Int[] occupiedCells; // Các ô tương đối mà vật cản chiếm
    private GridManager gridManager;

    void Start()
    {
        gridManager = FindObjectOfType<GridManager>();
        // Lấy vị trí gốc của vật cản
        Vector2Int origin = gridManager.WorldToGrid(transform.position);
        // Tính toán tất cả ô bị chiếm dựa trên vị trí gốc
        List<Vector2Int> absoluteCells = new List<Vector2Int>();
        foreach (Vector2Int cell in occupiedCells)
        {
            absoluteCells.Add(new Vector2Int(origin.x + cell.x, origin.y + cell.y));
        }
        // Chiếm các ô
        if (!gridManager.OccupyCells(absoluteCells.ToArray(), gameObject))
        {
            Debug.LogWarning($"Cannot place obstacle {name} at {origin}: Some cells are occupied!");
            Destroy(gameObject); // Hủy nếu không thể đặt
        }
    }

    void OnDestroy()
    {
        if (gridManager != null)
        {
            // Giải phóng các ô khi vật cản bị hủy
            Vector2Int origin = gridManager.WorldToGrid(transform.position);
            List<Vector2Int> absoluteCells = new List<Vector2Int>();
            foreach (Vector2Int cell in occupiedCells)
            {
                absoluteCells.Add(new Vector2Int(origin.x + cell.x, origin.y + cell.y));
            }
            gridManager.FreeCells(absoluteCells.ToArray());
        }
    }
}