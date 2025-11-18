using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    private Unit selectedAlly;

    // 2 biến bool này đảm bảo cho các corotine của player và enemy không bị chồng chéo lên nhau trong Update
    private bool IsPlayerActionInProgress = false;
    private bool IsEnemyActionInProgress = false;

    private void Awake()
    {
        Instance = this;
    }

    public void SelectAlly(Unit ally)
    {
        if(ally.isAlive == false) return;
        if (selectedAlly != null)
            selectedAlly.HideMoveRange();
        selectedAlly = ally;
        selectedAlly.ShowMoveRange();
    }

    public void DeselectAlly()
    {
        selectedAlly = null;
    }

    private void Update()
    {
        if(TurnBaseManager.Instance.isPlayerTurn)
        {
            Debug.Log(Input.GetMouseButtonDown(0));
            if (Input.GetMouseButtonDown(0) && selectedAlly != null)
            {
                if (!selectedAlly.isAlive)
                {
                    DeselectAlly();
                    return;
                }
                Vector3 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
                Vector2Int targetGrid = GridManager.Instance.WorldToGrid(mousePos);
                if (targetGrid != GridManager.Instance.WorldToGrid(selectedAlly.transform.position))// đảm bảo việc nhấn vào chính nó nhằm chọn nhân vật cần di chuyển không bị tính là chọn vị trí di chuyển, nếu bỏ điều kiện này nó sẽ nhầm lẫn việc chọn nhân vật cũng là chọn vị trí di chuyển mà phạm vi di chuyển của các nhân vật đều không có vị trí ban đầu nên dẫn đến điều kiện "nhấn ngoài phạm vi di chuyển → bỏ chọn"
                {
                    if(IsPlayerActionInProgress == false)//đảm bảo chỉ có 1 corotine PlayerTeamAction hoạt động, không được nhiều hơn 1 corotine PlayerTeamAction hoạt động cùng lúc
                        StartCoroutine(PlayerTeamAction(targetGrid));
                }
            }
            // Click phải: Hiện attack range
            if (Input.GetMouseButtonDown(1) && selectedAlly != null)
            {
                selectedAlly.ShowAttackRange();
            }
        }
        else
        {
            //AI tạm thời cho enemy
            Unit enemy= TurnBaseManager.Instance.EnemyTeam[Random.Range(0, TurnBaseManager.Instance.EnemyTeam.Count)];//chọn random enemy trong list enemy
            //random ô di chuyển đến trong phạm vi di chuyển của enemy
            Vector2Int target=new Vector2Int(GridManager.Instance.WorldToGrid(enemy.transform.position).x+ Random.Range(-1, 2), GridManager.Instance.WorldToGrid(enemy.transform.position).y+ Random.Range(-1, 2));//đáng lý là Random.Range(-enemy.moveRange, enemy.moveRange+1) nhưng do enemy.moveRange chưa được public nên tạm thời như vậy, giải thích thêm vì đây là random kiểu int nên max Exclusive nên mới enemy.moveRange+1
            while(target == GridManager.Instance.WorldToGrid(enemy.transform.position) || !GridManager.Instance.IsValidAndEmpty(target.x,target.y))//đảm bảo vị trí đích là vị trí của enemy hiện tại
            {
                target = new Vector2Int(GridManager.Instance.WorldToGrid(enemy.transform.position).x + Random.Range(-1, 2), GridManager.Instance.WorldToGrid(enemy.transform.position).y + Random.Range(-1, 2));//đáng lý là Random.Range(-enemy.moveRange, enemy.moveRange+1) nhưng do enemy.moveRange chưa được public nên tạm thời như vậy, giải thích thêm vì đây là random kiểu int nên max Exclusive nên mới enemy.moveRange+1
            }
            if (IsEnemyActionInProgress == false)//đảm bảo chỉ có 1 corotine EnemyTeamAction hoạt động, không được nhiều hơn 1 corotine PlayerTeamAction hoạt động cùng lúc. Nếu như bạn không có biến kiểm tra này thì vào frame kế tiếp khi Corotine EnemyTeamAction chạy ở frame trước đó còn chưa thực thi xong nên chưa đổi điều kiện dẫn đến đủ điều kiện ở frame kế tiếp cho phép gọi thêm 1 corotine PlayerTeamAction làm chồng chất corotine PlayerTeamAction thực thi dẫn đến sai kết quả mong muốn
                StartCoroutine(EnemyTeamAction(enemy, target));
            
        }
    }

    IEnumerator PlayerTeamAction(Vector2Int targetGrid)
    {
        IsPlayerActionInProgress = true;
        if (selectedAlly.IsInMoveRange(targetGrid))
        {
            yield return StartCoroutine(selectedAlly.MoveToTarget(targetGrid));
            DeselectAlly();
            //toàn bộ phe đồng minh lần lượt tấn công và next turn
            yield return StartCoroutine(TurnBaseManager.Instance.PlayerTeamAttack());
        }
        else// nhấn ngoài phạm vi di chuyển → bỏ chọn
        {
            selectedAlly.HideMoveRange();
            DeselectAlly();
        }
        IsPlayerActionInProgress = false;
    }
    IEnumerator EnemyTeamAction(Unit enemy,Vector2Int target)
    {
        IsEnemyActionInProgress = true;
        yield return StartCoroutine(enemy.MoveToTarget(target));//di chuyển đến ô ngẫu nhiên
                                                                //toàn bộ phe kẻ địch lần lượt tấn công và next turn
        yield return StartCoroutine(TurnBaseManager.Instance.EnemyTeamAttack());
        IsEnemyActionInProgress = false;
    }
}
