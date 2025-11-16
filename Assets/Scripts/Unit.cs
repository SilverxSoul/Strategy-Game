using UnityEngine;
using System.Collections;
using System.Collections.Generic;
public enum AttackShape { Cross, XShape, Melee } // Hình dạng phạm vi tấn công
public enum Team { Player, Enemy }// Cho Turn base
public enum Type {Sword, Magic, Shuriken } // Loại vũ khí sử dụng
public class Unit : MonoBehaviour
{
    [Header("Team")]
    public Team team = Team.Player;

    [Header("Weapon")]
    public Type type = Type.Sword;

    [Header("Stats")]
    [SerializeField] private int maxHP = 100;
    [SerializeField] private int damage = 25;
    [SerializeField] private int attackRange = 1; // 1 = 3x3 xung quanh

    [Header("Attack Shape")]
    [SerializeField] public AttackShape attackShape = AttackShape.Cross;

    [Header("Movement Settings")]
    [SerializeField] private float moveDurationPerCell = 0.3f; // Thời gian di chuyển 1 ô
    [SerializeField] private int moveRange = 1;// 1 = 3x3, 2 = 5x5,...
    private Vector2Int currentGridPos;

    // Private vars
    private int currentHP;
    public bool isAlive { get { return currentHP > 0; } }
    private bool hasMoved = false;
    private bool hasAttacked = false;
    private GridManager gridManager;
    private bool isMoving = false;
    private Animator animator;
    [SerializeField] private GameObject shurikenPrefab;
    [SerializeField] private GameObject explosionPrefab;

    void Start()
    {
        gridManager = FindObjectOfType<GridManager>();
        currentHP = maxHP;
        currentGridPos = gridManager.WorldToGrid(transform.position);
        gridManager.OccupyCell(currentGridPos.x, currentGridPos.y, gameObject);
        animator = GetComponent<Animator>();
        if (team == Team.Player)
            StartTurn();



    }
    public void StartTurn()
    {
        hasMoved = false;
        hasAttacked = false;
        ShowMoveRange();
        Debug.Log($"{name} ready to move & attack!");
        Debug.Log(hasMoved);
        Debug.Log(isAlive);
    }

    void Update()
    {

        if (!isAlive || hasMoved) return;

        // Player chỉ di chuyển, Enemy không cần input
        if (team == Team.Player && Input.GetMouseButtonDown(0))
        {
            Vector3 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            Vector2Int targetGrid = gridManager.WorldToGrid(mousePos);

            if (IsInMoveRange(targetGrid))
            {
                StartCoroutine(MoveToTarget(targetGrid));
            }
        }
        // Click phải: Hiện attack range
        if (team == Team.Player && Input.GetMouseButtonDown(1))
        {
            ShowAttackRange();
        }

        if (team == Team.Player && Input.GetKeyDown(KeyCode.A))
        {
            PerformAttack();
        }


    }

    public void ShowAttackRange()
    {
        gridManager.DrawAttackRangeHighlight(currentGridPos, attackShape, Color.gray);
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
            animator.SetFloat("Horizontal", next.x - path[i - 1].x);
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

        //gridManager.AttackRange(currentGridPos, attackShape);//reset lại range attack khi tới vị trí mới
        isMoving = false;
        animator.SetBool("Moving", false);
        animator.SetFloat("Horizontal", 0);
        animator.SetFloat("Vertical", 0);

    }

    public void Hurt()
    {
        //HP giảm

        //Animation hurt
        animator.SetTrigger("Hurt");
    }

    private void PerformAttack()
    {
        Unit UnitTakeDamage = gridManager.GetUnitOnAttackRange(currentGridPos, attackShape);
        if (UnitTakeDamage != null)
        {
            if(type!= Type.Magic)
            {
                animator.SetFloat("Horizontal", UnitTakeDamage.currentGridPos.x - currentGridPos.x);
                animator.SetFloat("Vertical", UnitTakeDamage.currentGridPos.y - currentGridPos.y);
            }
            else//Magic thì không quan tâm đến vị trí kẻ thù nên chọn animation attack hướng xuống dưới cho toàn bộ loại attack trong animator
            {
                animator.SetFloat("Horizontal",0);
                animator.SetFloat("Vertical", -1);
            }
            animator.SetTrigger("Attack");
            UnitTakeDamage.Hurt();
        }

    }



    private void ThrowShuriken()
    {
        float shurikenSpeed = 10f;
        Unit UnitTakeDamage = gridManager.GetUnitOnAttackRange(currentGridPos, attackShape);
        Vector2 direction = new Vector2(UnitTakeDamage.currentGridPos.x - currentGridPos.x, UnitTakeDamage.currentGridPos.y - currentGridPos.y).normalized;
        GameObject shuriken = Instantiate(shurikenPrefab, transform.position, Quaternion.identity);
        shuriken.GetComponent<Projectile>().isPlayerProjectile = (team == Team.Player);
        shuriken.GetComponent<Rigidbody2D>().velocity = direction * shurikenSpeed;
        
    }

    private void CastExplosion()
    {
        Unit UnitTakeDamage = gridManager.GetUnitOnAttackRange(currentGridPos, attackShape);
        GameObject explosion = Instantiate(explosionPrefab, UnitTakeDamage.transform.position, Quaternion.identity);
    }
}