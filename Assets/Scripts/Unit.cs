using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Unit : MonoBehaviour
{
    private GridManager gridManager;
    private bool isMoving = false;
    private Animator animator;

    [Header("Movement Settings")]
    [SerializeField] private float moveDurationPerCell = 0.3f; // Thời gian di chuyển 1 ô
    [SerializeField] private int moveRange = 1;// 1 = 3x3, 2 = 5x5,...
    private Vector2Int currentGridPos;
    void Start()
    {
        gridManager = FindObjectOfType<GridManager>();
        currentGridPos = gridManager.WorldToGrid(transform.position);
        gridManager.OccupyCell(currentGridPos.x, currentGridPos.y, gameObject);
        animator = GetComponent<Animator>();

        // Highlight phạm vi ban đầu
        ShowMoveRange();
    }

    void Update()
    {
        if (isMoving) return;

        if (Input.GetMouseButtonDown(0))
        {
            Vector3 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            Vector2Int targetGrid = gridManager.WorldToGrid(mousePos);

            // Kiểm tra target có trong phạm vi 3x3 không
            if (IsInMoveRange(targetGrid))
            {
                StartCoroutine(MoveToTarget(targetGrid));
            }
        }
        if(Input.GetKeyDown(KeyCode.S))
        {
            animator.SetTrigger("Attack");
        }
    }
    // Hiển thị phạm vi di chuyển 3x3
    public void ShowMoveRange()
    {
        gridManager.HighlightMoveRange(currentGridPos, moveRange);
    }

    // Ẩn phạm vi
    public void HideMoveRange()
    {
        gridManager.ClearHighlights();
    }

    // Kiểm tra ô đích có trong phạm vi không
    private bool IsInMoveRange(Vector2Int target)
    {
        int dx = Mathf.Abs(target.x - currentGridPos.x);
        int dy = Mathf.Abs(target.y - currentGridPos.y);
        return dx <= moveRange && dy <= moveRange && target != currentGridPos;
    }
    // Coroutine: Di chuyển theo đường đi
    private IEnumerator MoveToTarget(Vector2Int target)
    {
        if (isMoving) yield break;
        animator.SetBool("Moving", true);
        isMoving = true;

        // Ẩn highlight khi bắt đầu di chuyển
        HideMoveRange();


        List<Vector2Int> path = gridManager.FindPath(currentGridPos, target); if (path == null || path.Count == 0)
        {
            Debug.Log("Không tìm thấy đường đi!");
            isMoving = false;
            ShowMoveRange(); // Hiện lại range
            yield break;
        }

        // Giải phóng ô hiện tại trước khi di chuyển
        gridManager.FreeCell(currentGridPos.x, currentGridPos.y);

        // Di chuyển từng ô theo đường đi
        for (int i = 1; i < path.Count; i++) // Bỏ ô đầu (đang đứng)
        {
            Vector2Int next = path[i];
            animator.SetFloat("Horizontal",next.x - path[i-1].x);
            animator.SetFloat("Vertical", next.y - path[i - 1].y);
            Vector3 targetWorld = gridManager.GridToWorld(next.x, next.y);

            // Di chuyển mượt mà
            float elapsed = 0;
            Vector3 startPos = transform.position;
            while (elapsed < moveDurationPerCell)
            {
                elapsed += Time.deltaTime;
                transform.position = Vector3.Lerp(startPos, targetWorld, elapsed / moveDurationPerCell);
                yield return null;
            }
            transform.position = targetWorld; // Đảm bảo đúng vị trí
            currentGridPos = next;
        }
        // Chiếm ô đích sau khi di chuyển
        gridManager.OccupyCell(target.x, target.y, gameObject);
        

        isMoving = false;
        animator.SetBool("Moving", false);
        animator.SetFloat("Horizontal", 0);
        animator.SetFloat("Vertical", 0);
        // Hiện lại phạm vi mới
        ShowMoveRange();
    }
}