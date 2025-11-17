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
                                                  // === HIGHLIGHT SYSTEM ===
    [Header("Highlight Movement Settings")]
    [SerializeField] private Color validMoveColor = new Color(1f, 1f, 0f, 0.3f); // Vàng nhạt
    [SerializeField] private Color invalidMoveColor = new Color(1f, 0f, 0f, 0.2f); // Đỏ nhạt
    [SerializeField] private Sprite HightlightSprite; // Sprite dùng để highlight
                                                      // Thêm enum (nếu chưa có)
    private Dictionary<Vector2Int, Color> highlightCells = new Dictionary<Vector2Int, Color>();

    [Header("Hightlight Attack Range Settings")]
    [SerializeField] private Color crossColor = Color.yellow;
    [SerializeField] private Color xShapeColor = Color.yellow;
    public List<Vector2Int> attackCells = new List<Vector2Int>();

    
    [Header("Grid Line Settings")]
    [SerializeField] private Color gridLineColor = new Color(0.3f, 0.3f, 0.3f, 1f);
    [SerializeField] private float gridLineWidth = 0.03f;

    private LineRenderer horizontalLinesRenderer;
    private LineRenderer verticalLinesRenderer;


    // Struct lưu thông tin ô
    public struct CellData
    {
        public bool isOccupied; // Ô có bị chiếm không
        public GameObject occupant; // Đối tượng chiếm ô (nhân vật, vật cản,...)
    }

    public static GridManager Instance;
    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }
    public Unit GetUnitAt(int x, int y)
    {
        Vector2Int pos = new Vector2Int(x, y);
        if (occupied.ContainsKey(pos) && occupied[pos].occupant != null)
        {
            return occupied[pos].occupant.GetComponent<Unit>();//nếu occupant này không có component Unit thì cũng tự trả về null
        }
        return null;
    }
    // Dictionary lưu trạng thái ô
    private Dictionary<Vector2Int, CellData> occupied = new Dictionary<Vector2Int, CellData>();

    void Start()
    { 
        // Không cần khởi tạo toàn bộ lưới
        // Có thể thêm vật cản ở đây (xem Bước 2)

        CreateGridLines();
    }
    void CreateGridLines()
    {
        // Tạo container
        GameObject gridLinesObj = new GameObject("GridLines");
        gridLinesObj.transform.SetParent(transform);

        //  LINE RENDERER NGANG RIÊNG
        GameObject hLinesObj = new GameObject("HorizontalLines");
        hLinesObj.transform.SetParent(gridLinesObj.transform);
        horizontalLinesRenderer = hLinesObj.AddComponent<LineRenderer>();
        SetupLineRenderer(horizontalLinesRenderer);
        DrawHorizontalLines();

        //  LINE RENDERER DỌC RIÊNG  
        GameObject vLinesObj = new GameObject("VerticalLines");
        vLinesObj.transform.SetParent(gridLinesObj.transform);
        verticalLinesRenderer = vLinesObj.AddComponent<LineRenderer>();
        SetupLineRenderer(verticalLinesRenderer);
        DrawVerticalLines();
    }

    void SetupLineRenderer(LineRenderer lr)
    {
        lr.material = new Material(Shader.Find("Sprites/Default"));
        lr.material.color = gridLineColor;
        lr.startWidth = gridLineWidth;
        lr.endWidth = gridLineWidth;
        lr.useWorldSpace = true;
    }

    void DrawHorizontalLines()
    {
        List<Vector3> points = new List<Vector3>();

        for (int y = 0; y <= height; y++)
        {
            if (y % 2 == 0) // Chẵn: Trái → Phải
            {
                points.Add(tilemap.CellToWorld(new Vector3Int(0, y, 0)));
                points.Add(tilemap.CellToWorld(new Vector3Int(width, y, 0)));
            }
            else // Lẻ: Phải → Trái
            {
                points.Add(tilemap.CellToWorld(new Vector3Int(width, y, 0)));
                points.Add(tilemap.CellToWorld(new Vector3Int(0, y, 0)));
            }
        }

        horizontalLinesRenderer.positionCount = points.Count;
        horizontalLinesRenderer.SetPositions(points.ToArray());
    }

    void DrawVerticalLines()
    {
        List<Vector3> points = new List<Vector3>();

        for (int x = 0; x <= width; x++)
        {
            if (x % 2 == 0) // Chẵn: Trên → Dưới
            {
                points.Add(tilemap.CellToWorld(new Vector3Int(x, 0, 0)));
                points.Add(tilemap.CellToWorld(new Vector3Int(x, height, 0)));
            }
            else // Lẻ: Dưới → Trên
            {
                points.Add(tilemap.CellToWorld(new Vector3Int(x, height, 0)));
                points.Add(tilemap.CellToWorld(new Vector3Int(x, 0, 0)));
            }
        }

        verticalLinesRenderer.positionCount = points.Count;
        verticalLinesRenderer.SetPositions(points.ToArray());
    }


    // Highlight ô di chuyển hợp lệ
    public void HighlightMoveRange(Vector2Int center, int range = 1)
    {
        ClearHighlights();

        int minX = Mathf.Max(0, center.x - range);
        int maxX = Mathf.Min(width - 1, center.x + range);
        int minY = Mathf.Max(0, center.y - range);
        int maxY = Mathf.Min(height - 1, center.y + range);

        for (int x = minX; x <= maxX; x++)
        {
            for (int y = minY; y <= maxY; y++)
            {
                Vector2Int pos = new Vector2Int(x, y);
                if (pos == center) continue; // Bỏ qua vị trí hiện tại

                if (IsValidAndEmpty(x, y))
                {
                    highlightCells[pos] = validMoveColor;
                }
                else
                {
                    highlightCells[pos] = invalidMoveColor;
                }
            }
        }

        DrawHighlights();
    }

    //phạm vi tấn công của 1 unit
    public void AttackRange(Vector2Int center, AttackShape shape)
    {
        attackCells.Clear();// xóa phạm vi tấn công ở các vị trí khác để cập nhật phạm vi tấn công hiện tại
        if (shape == AttackShape.Cross)
        {
            // === ĐƯỜNG NGANG TOÀN BẢN ĐỒ ===
            for (int x = 0; x < width; x++)
            {
                if (x != center.x) // Bỏ ô trung tâm (nhân vật)
                    attackCells.Add(new Vector2Int(x, center.y));
            }

            // === ĐƯỜNG DỌC TOÀN BẢN ĐỒ ===
            for (int y = 0; y < height; y++)
            {
                if (y != center.y)
                    attackCells.Add(new Vector2Int(center.x, y));
            }
        }
        else if (shape == AttackShape.XShape)
        {
            // === ĐƯỜNG CHÉO 1: \ (từ trên trái xuống dưới phải) ===
            // Từ (center.x, center.y) → tăng x, tăng y
            for (int step = 1; step < Mathf.Max(width, height); step++)
            {
                int x1 = center.x + step;
                int y1 = center.y + step;
                if (x1 < width && y1 < height)
                    attackCells.Add(new Vector2Int(x1, y1));
                else
                    break;
            }
            for (int step = 1; step < Mathf.Max(width, height); step++)
            {
                int x1 = center.x - step;
                int y1 = center.y - step;
                if (x1 >= 0 && y1 >= 0)
                    attackCells.Add(new Vector2Int(x1, y1));
                else
                    break;
            }

            // === ĐƯỜNG CHÉO 2: / (từ trên phải xuống dưới trái) ===
            for (int step = 1; step < Mathf.Max(width, height); step++)
            {
                int x2 = center.x + step;
                int y2 = center.y - step;
                if (x2 < width && y2 >= 0)
                    attackCells.Add(new Vector2Int(x2, y2));
                else
                    break;
            }
            for (int step = 1; step < Mathf.Max(width, height); step++)
            {
                int x2 = center.x - step;
                int y2 = center.y + step;
                if (x2 >= 0 && y2 < height)
                    attackCells.Add(new Vector2Int(x2, y2));
                else
                    break;
            }
        }
        else if (shape == AttackShape.Melee)// phạm vi loại attack cận chiến này là 4 ô xung quanh nhân vật 
        {
            attackCells.Add(new Vector2Int(center.x, center.y - 1));
            attackCells.Add(new Vector2Int(center.x, center.y + 1));
            attackCells.Add(new Vector2Int(center.x - 1, center.y));
            attackCells.Add(new Vector2Int(center.x + 1, center.y));
        }
    }    

    public Unit GetUnitOnAttackRange(Vector2Int center, AttackShape shape)
    {
        AttackRange(center, shape);
        foreach(Vector2Int pos in attackCells)
        {
            if(GetUnitAt(pos.x, pos.y) != null)
            {
                Unit unit = GetUnitAt(pos.x, pos.y);
                if (unit.team == Team.Enemy)
                    return unit;
            }
        }
        return null;
    }
    //Hightlight phạm vi tấn công 
    public void DrawAttackRangeHighlight(Vector2Int center, AttackShape shape, Color color)
    {
        ClearHighlights(); // Xóa highlight cũ
        AttackRange(center, shape);

        // === HIGHLIGHT TẤT CẢ Ô TRONG PHẠM VI ===
        foreach (Vector2Int pos in attackCells)
        {
            // Chỉ highlight nếu ô trống hoặc có unit khác
            if (IsValidAndEmpty(pos.x, pos.y) || GetUnitAt(pos.x, pos.y) != null)
            {
                highlightCells[pos] = color;
            }
        }

        DrawHighlights(); // Vẽ highlight như move range
    }


    // Xóa highlight
    public void ClearHighlights()
    {
        highlightCells.Clear();
        DrawHighlights();
    }

    void DrawHighlights()
    {
        // Tạo hoặc cập nhật GameObject hiển thị highlight
        GameObject highlightObj = GameObject.Find("MoveHighlights");
        if (highlightObj == null)
        {
            highlightObj = new GameObject("MoveHighlights");
            highlightObj.transform.SetParent(transform);
        }

        SpriteRenderer[] highlightRenderers = highlightObj.GetComponentsInChildren<SpriteRenderer>();
        for (int i = 0; i < highlightRenderers.Length; i++)
        {
            DestroyImmediate(highlightRenderers[i].gameObject);
        }

        foreach (var kvp in highlightCells)
        {
            GameObject highlightTile = new GameObject("Highlight_" + kvp.Key);
            highlightTile.transform.SetParent(highlightObj.transform);

            SpriteRenderer sr = highlightTile.AddComponent<SpriteRenderer>();
            sr.sprite = HightlightSprite; // Tạo sprite vuông
            sr.color = kvp.Value;
            sr.sortingOrder = 1; // Hiển thị trên grid

            highlightTile.transform.position = GridToWorld(kvp.Key.x, kvp.Key.y);
        }
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
        // Gizmos highlight
        foreach (var kvp in highlightCells)
        {
            Gizmos.color = kvp.Value;
            Vector3 worldPos = GridToWorld(kvp.Key.x, kvp.Key.y);
            Gizmos.DrawCube(worldPos, new Vector3(cellSize * 0.9f, cellSize * 0.9f, 0));
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