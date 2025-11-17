using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    private Unit selectedAlly;

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
        Debug.Log(selectedAlly.gameObject.name + " selected");
    }

    public void DeselectAlly()
    {
        selectedAlly = null;
    }

    private void Update()
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
                if (selectedAlly.IsInMoveRange(targetGrid))
                {
                    selectedAlly.DoMoveToTarget(targetGrid);
                }
                else// nhấn ngoài phạm vi di chuyển → bỏ chọn
                {
                    selectedAlly.HideMoveRange();
                    DeselectAlly();
                }
            }
        }
        // Click phải: Hiện attack range
        if (Input.GetMouseButtonDown(1) && selectedAlly != null)
        {
            selectedAlly.ShowAttackRange();
        }
    }
}
