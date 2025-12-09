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
    public bool IsEnemyActionInProgress = false;

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
    }

    public IEnumerator PlayerTeamAction(Vector2Int targetGrid)
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
    public IEnumerator EnemyTeamAction(Unit enemy,Vector2Int target)
    {
        if(enemy !=null)
        {
            IsEnemyActionInProgress = true;
            yield return StartCoroutine(enemy.MoveToTarget(target));//di chuyển đến ô ngẫu nhiên
                                                                    //toàn bộ phe kẻ địch lần lượt tấn công và next turn
            yield return StartCoroutine(TurnBaseManager.Instance.EnemyTeamAttack());
            IsEnemyActionInProgress = false;
        }
    }
}
