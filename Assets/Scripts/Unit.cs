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

    void Start()
    {
        gridManager = FindObjectOfType<GridManager>();
        Vector2Int startGrid = gridManager.WorldToGrid(transform.position);
        gridManager.OccupyCell(startGrid.x, startGrid.y, gameObject);
        animator = GetComponent<Animator>();
    }

    void Update()
    {
        if (isMoving) return;

        if (Input.GetMouseButtonDown(0))
        {
            Vector3 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            Vector2Int targetGrid = gridManager.WorldToGrid(mousePos);

            // Bắt đầu di chuyển đến đích
            StartCoroutine(MoveToTarget(targetGrid));
        }
        if(Input.GetKeyDown(KeyCode.S))
        {
            animator.SetTrigger("Attack");
        }
    }

    // Coroutine: Di chuyển theo đường đi
    private IEnumerator MoveToTarget(Vector2Int target)
    {
        if (isMoving) yield break;
        animator.SetBool("Moving", true);
        isMoving = true;

        Vector2Int start = gridManager.WorldToGrid(transform.position);
        if (start == target)
        {
            isMoving = false;
            yield break;
        }

        List<Vector2Int> path = gridManager.FindPath(start, target);
        if (path == null || path.Count == 0)
        {
            Debug.Log("Không tìm thấy đường đi!");
            isMoving = false;
            yield break;
        }

        // Giải phóng ô hiện tại trước khi di chuyển
        gridManager.FreeCell(start.x, start.y);

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
        }
        // Chiếm ô đích sau khi di chuyển
        gridManager.OccupyCell(target.x, target.y, gameObject);

        isMoving = false;
        animator.SetBool("Moving", false);
        animator.SetFloat("Horizontal", 0);
        animator.SetFloat("Vertical", 0);
    }
}