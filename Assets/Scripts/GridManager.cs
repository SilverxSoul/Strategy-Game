using UnityEngine;
using UnityEngine.Tilemaps;
using System.Collections.Generic;
using System.Linq;

public class GridManager : MonoBehaviour
{
    [SerializeField] private int width = 10; // Chiều rộng lưới
    [SerializeField] private int height = 10; // Chiều cao lưới
    [SerializeField] private Tilemap tilemap; // Kết nối với Tilemap
    [SerializeField] private float cellSize = 1f; // Kích thước ô

    // Struct lưu thông tin ô
    public struct CellData
    {
        public bool isOccupied; // Ô có bị chiếm không
        public GameObject occupant; // Đối tượng chiếm ô (nhân vật, vật cản,...)
    }

    // Dictionary lưu trạng thái ô
    private Dictionary<Vector2Int, CellData> occupied = new Dictionary<Vector2Int, CellData>();

    void Start()
    {
        // Không cần khởi tạo toàn bộ lưới
        // Có thể thêm vật cản ở đây (xem Bước 2)
    }

    // Chuyển tọa độ world sang grid
    public Vector2Int WorldToGrid(Vector3 worldPos)
    {
        Vector3Int cellPos = tilemap.WorldToCell(worldPos);
        return new Vector2Int(cellPos.x, cellPos.y);
    }

    // Chuyển tọa độ grid sang world
    public Vector3 GridToWorld(int x, int y)
    {
        return tilemap.CellToWorld(new Vector3Int(x, y, 0)) + new Vector3(cellSize / 2, cellSize / 2, 0);
    }

    // Kiểm tra ô có hợp lệ và trống không
    public bool IsValidAndEmpty(int x, int y)
    {
        Vector2Int pos = new Vector2Int(x, y);
        if (x < 0 || x >= width || y < 0 || y >= height)
            return false;
        return !occupied.ContainsKey(pos) || !occupied[pos].isOccupied;
    }

    // Chiếm một ô
    public void OccupyCell(int x, int y, GameObject occupant)
    {
        Vector2Int pos = new Vector2Int(x, y);
        if (IsValidAndEmpty(x, y))
        {
            occupied[pos] = new CellData { isOccupied = true, occupant = occupant };
        }
    }

    // Chiếm nhiều ô (cho vật cản lớn)
    public bool OccupyCells(Vector2Int[] cells, GameObject occupant)
    {
        // Kiểm tra tất cả ô có trống không
        foreach (Vector2Int cell in cells)
        {
            if (!IsValidAndEmpty(cell.x, cell.y))
            {
                return false; // Nếu một ô không hợp lệ, hủy thao tác
            }
        }

        // Chiếm tất cả ô
        foreach (Vector2Int cell in cells)
        {
            OccupyCell(cell.x, cell.y, occupant);
        }
        return true;
    }

    // Giải phóng một ô
    public void FreeCell(int x, int y)
    {
        Vector2Int pos = new Vector2Int(x, y);
        if (occupied.ContainsKey(pos))
        {
            occupied.Remove(pos);
        }
    }

    // Giải phóng nhiều ô
    public void FreeCells(Vector2Int[] cells)
    {
        foreach (Vector2Int cell in cells)
        {
            FreeCell(cell.x, cell.y);
        }
    }

    // Di chuyển unit
    public bool MoveTo(GameObject unit, int targetX, int targetY)
    {
        Vector2Int currentGrid = WorldToGrid(unit.transform.position);
        if (IsValidAndEmpty(targetX, targetY))
        {
            FreeCell(currentGrid.x, currentGrid.y);
            OccupyCell(targetX, targetY, unit);
            unit.transform.position = GridToWorld(targetX, targetY);
            return true;
        }
        return false;
    }

    // Debug: Vẽ lưới và ô bị chiếm
    void OnDrawGizmos()
    {
        Gizmos.color = Color.yellow;
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                Vector3 pos = GridToWorld(x, y);
                Gizmos.DrawWireCube(pos, new Vector3(cellSize, cellSize, 0));
            }
        }
        Gizmos.color = Color.red;
        foreach (var pos in occupied.Keys)
        {
            if (occupied[pos].isOccupied)
            {
                Vector3 worldPos = GridToWorld(pos.x, pos.y);
                Gizmos.DrawCube(worldPos, new Vector3(cellSize * 0.8f, cellSize * 0.8f, 0));
            }
        }
    }
    // === A* PATHFINDING ===
    public List<Vector2Int> FindPath(Vector2Int start, Vector2Int end)
    {
        if (!IsValidAndEmpty(end.x, end.y)) return null; // Không thể đến đích

        var openSet = new List<Node>();
        var closedSet = new HashSet<Vector2Int>();
        var cameFrom = new Dictionary<Vector2Int, Vector2Int>();
        var gScore = new Dictionary<Vector2Int, float> { [start] = 0 };
        var fScore = new Dictionary<Vector2Int, float> { [start] = Heuristic(start, end) };

        openSet.Add(new Node(start, fScore[start]));

        while (openSet.Count > 0)
        {
            var current = openSet.OrderBy(n => n.f).First();
            openSet.Remove(current);

            if (current.pos == end)
                return ReconstructPath(cameFrom, current.pos);

            closedSet.Add(current.pos);

            foreach (var neighbor in GetNeighbors(current.pos))
            {
                if (closedSet.Contains(neighbor)) continue;

                float tentativeG = gScore[current.pos] + 1; // Chi phí di chuyển = 1

                if (!gScore.ContainsKey(neighbor) || tentativeG < gScore[neighbor])
                {
                    cameFrom[neighbor] = current.pos;
                    gScore[neighbor] = tentativeG;
                    fScore[neighbor] = tentativeG + Heuristic(neighbor, end);

                    if (!openSet.Any(n => n.pos == neighbor))
                        openSet.Add(new Node(neighbor, fScore[neighbor]));
                }
            }
        }

        return null; // Không tìm thấy đường
    }

    private List<Vector2Int> GetNeighbors(Vector2Int pos)
    {
        var neighbors = new List<Vector2Int>();
        int[,] directions = { { 0, 1 }, { 1, 0 }, { 0, -1 }, { -1, 0 } }; // 4 hướng

        for (int i = 0; i < 4; i++)
        {
            int nx = pos.x + directions[i, 0];
            int ny = pos.y + directions[i, 1];
            if (nx >= 0 && nx < width && ny >= 0 && ny < height && IsValidAndEmpty(nx, ny))
                neighbors.Add(new Vector2Int(nx, ny));
        }
        return neighbors;
    }

    private float Heuristic(Vector2Int a, Vector2Int b)
    {
        return Mathf.Abs(a.x - b.x) + Mathf.Abs(a.y - b.y); // Manhattan distance
    }

    private List<Vector2Int> ReconstructPath(Dictionary<Vector2Int, Vector2Int> cameFrom, Vector2Int current)
    {
        var path = new List<Vector2Int> { current };
        while (cameFrom.ContainsKey(current))
        {
            current = cameFrom[current];
            path.Add(current);
        }
        path.Reverse();
        return path;
    }

    // Class hỗ trợ A*
    private class Node
    {
        public Vector2Int pos;
        public float f;
        public Node(Vector2Int pos, float f) { this.pos = pos; this.f = f; }
    }
}