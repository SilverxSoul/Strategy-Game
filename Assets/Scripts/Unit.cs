using UnityEngine;

public class Unit : MonoBehaviour
{
    private GridManager gridManager;

    void Start()
    {
        gridManager = FindObjectOfType<GridManager>(); // Tìm GridManager
        Vector2Int startGrid = gridManager.WorldToGrid(transform.position);
        gridManager.OccupyCell(startGrid.x, startGrid.y,gameObject); // Chiếm ô khởi đầu
    }

    // Ví dụ: Di chuyển khi click (thay bằng input turn-based)
    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            Vector3 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            Vector2Int targetGrid = gridManager.WorldToGrid(mousePos);
            gridManager.MoveTo(gameObject, targetGrid.x, targetGrid.y);
        }
    }
}