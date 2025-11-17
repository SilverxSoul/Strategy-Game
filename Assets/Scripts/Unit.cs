using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
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
    [SerializeField] private int maxHP = 3;
    [SerializeField] private int damage = 1;
    [SerializeField] private int attackRange = 1; // 1 = 3x3 xung quanh

    [Header("Attack Shape")]
    [SerializeField] public AttackShape attackShape = AttackShape.Cross;

    [Header("Movement Settings")]
    [SerializeField] private float moveDurationPerCell = 0.3f; // Thời gian di chuyển 1 ô
    [SerializeField] private int moveRange = 1;// 1 = 3x3, 2 = 5x5,...
    private Vector2Int currentGridPos;

    public bool isAlive { get { return currentHP > 0; } }
    // Private vars
    private int currentHP;
    private bool hasAttacked = false;
    private bool isMoving = false;
    private Animator animator;
    [SerializeField] private GameObject shurikenPrefab;
    [SerializeField] private GameObject explosionPrefab;
    [SerializeField] private Slider HealthBar;

    void Start()
    {
        currentHP = maxHP;
        currentGridPos = GridManager.Instance.WorldToGrid(transform.position);
        GridManager.Instance.OccupyCell(currentGridPos.x, currentGridPos.y, gameObject);
        animator = GetComponent<Animator>();
        UpdateHealth();
    }
    public void StartTurn()
    {
        hasAttacked = false;
    }

    public void ShowAttackRange()
    {
        GridManager.Instance.DrawAttackRangeHighlight(currentGridPos, attackShape, Color.gray);
    }
    // Hiển thị phạm vi di chuyển 3x3
    public void ShowMoveRange()
    {
        GridManager.Instance.HighlightMoveRange(currentGridPos, moveRange);
    }

    // Ẩn phạm vi
    public void HideMoveRange()
    {
        GridManager.Instance.ClearHighlights();
    }

    // Kiểm tra ô đích có trong phạm vi không
    public bool IsInMoveRange(Vector2Int target)
    {
        int dx = Mathf.Abs(target.x - currentGridPos.x);
        int dy = Mathf.Abs(target.y - currentGridPos.y);
        return dx <= moveRange && dy <= moveRange && target != currentGridPos;
    }
    public void DoMoveToTarget(Vector2Int target)
    {
        StartCoroutine(MoveToTarget(target));
    }
    // Coroutine: Di chuyển theo đường đi
    private IEnumerator MoveToTarget(Vector2Int target)
    {
        if (isMoving) yield break;
        animator.SetBool("Moving", true);
        isMoving = true;

        // Ẩn highlight khi bắt đầu di chuyển
        HideMoveRange();


        List<Vector2Int> path = GridManager.Instance.FindPath(currentGridPos, target); if (path == null || path.Count == 0)
        {
            Debug.Log("Không tìm thấy đường đi!");
            isMoving = false;
            ShowMoveRange(); // Hiện lại range
            yield break;
        }

        // Giải phóng ô hiện tại trước khi di chuyển
        GridManager.Instance.FreeCell(currentGridPos.x, currentGridPos.y);

        // Di chuyển từng ô theo đường đi
        for (int i = 1; i < path.Count; i++) // Bỏ ô đầu (đang đứng)
        {
            Vector2Int next = path[i];
            animator.SetFloat("Horizontal", next.x - path[i - 1].x);
            animator.SetFloat("Vertical", next.y - path[i - 1].y);
            Vector3 targetWorld = GridManager.Instance.GridToWorld(next.x, next.y);

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
        GridManager.Instance.OccupyCell(target.x, target.y, gameObject);

        //GridManager.Instance.AttackRange(currentGridPos, attackShape);//reset lại range attack khi tới vị trí mới
        isMoving = false;
        animator.SetBool("Moving", false);
        animator.SetFloat("Horizontal", 0);
        animator.SetFloat("Vertical", 0);

    }

    public void Hurt(int dmg)
    {
        //HP giảm
        currentHP= currentHP - dmg;
        if (currentHP <= 0)
        {
            currentHP = 0;
            Death();
            return;
        }
        //Animation hurt
        animator.SetTrigger("Hurt");
        
    }

    public void Death()
    {
        animator.SetBool("Death",true);
        Debug.Log(isAlive);
    }

    public void PerformAttack()
    {
        Unit UnitTakeDamage = GridManager.Instance.GetUnitOnAttackRange(currentGridPos, attackShape);
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
            UnitTakeDamage.Hurt(damage);
            UnitTakeDamage.UpdateHealth();
        }

    }



    private void ThrowShuriken()
    {
        float shurikenSpeed = 10f;
        Unit UnitTakeDamage = GridManager.Instance.GetUnitOnAttackRange(currentGridPos, attackShape);
        Vector2 direction = new Vector2(UnitTakeDamage.currentGridPos.x - currentGridPos.x, UnitTakeDamage.currentGridPos.y - currentGridPos.y).normalized;
        GameObject shuriken = Instantiate(shurikenPrefab, transform.position, Quaternion.identity);
        shuriken.GetComponent<Projectile>().isPlayerProjectile = (team == Team.Player);
        shuriken.GetComponent<Rigidbody2D>().velocity = direction * shurikenSpeed;
        
    }

    private void CastExplosion()
    {
        Unit UnitTakeDamage = GridManager.Instance.GetUnitOnAttackRange(currentGridPos, attackShape);
        GameObject explosion = Instantiate(explosionPrefab, UnitTakeDamage.transform.position, Quaternion.identity);
    }

    private void UpdateHealth()
    {
        HealthBar.maxValue = maxHP;
        HealthBar.value = currentHP;
        
    }

    private void OnMouseDown()
    {
        //có gì thêm điều kiện trong lượt đồng minh 

        if (team == Team.Player)
        {
            GameManager.Instance.SelectAlly(this);
            Debug.Log(gameObject.name + " được chọn!");
        }
    }
}