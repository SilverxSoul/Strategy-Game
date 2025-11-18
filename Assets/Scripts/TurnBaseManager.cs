using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TurnBaseManager : MonoBehaviour
{
    public List<Unit> PlayerTeam = new List<Unit>();
    public List<Unit> EnemyTeam = new List<Unit>();
    public Cinemachine.CinemachineVirtualCamera battleCamera;
    [Header("UI")]
    public UnityEngine.UI.Text turnText; // "Lượt: Player" / "Lượt: Enemy"

    public bool isPlayerTurn = true; // Bắt đầu lượt Player

    public static TurnBaseManager Instance;
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

    void Start()
    {
        GameObject gridMap = GameObject.Find("Grid");
        Unit[] units =gridMap.GetComponentsInChildren<Unit>();
        foreach (Unit unit in units)
        {
            if (unit.team == Team.Player)
            {
                PlayerTeam.Add(unit);
            }
            else
            {
                EnemyTeam.Add(unit);
            }
        }
    }


    //public IEnumerator TeamAttack()
    //{
    //    if (isPlayerTurn)
    //    {
    //        foreach (Unit unit in PlayerTeam)
    //        {
    //            battleCamera.Follow = unit.transform;
    //            unit.PerformAttack();
    //            yield return new WaitForSeconds(0.5f);
    //        }
    //    }
    //    else
    //    {
    //        foreach (Unit unit in EnemyTeam)
    //        {
    //            battleCamera.Follow = unit.transform;
    //            unit.PerformAttack();
    //            yield return new WaitForSeconds(0.5f);
    //        }
    //    }
    //    NextTurn();
    //}

    public IEnumerator PlayerTeamAttack()
    {

        foreach (Unit unit in PlayerTeam)
        {
            battleCamera.Follow = unit.transform;
            unit.PerformAttack();
            yield return new WaitForSeconds(0.5f);
        }
        NextTurn();
    }
    public IEnumerator EnemyTeamAttack()
    {

        foreach (Unit unit in EnemyTeam)
        {
            battleCamera.Follow = unit.transform;
            unit.PerformAttack();
            yield return new WaitForSeconds(0.5f);
        }
        NextTurn();
    }
    private void NextTurn()
    {
        isPlayerTurn = !isPlayerTurn;
        UpdateTurnUI();
    }

    private void UpdateTurnUI()
    {
        //turnText.text = $"Lượt: {(isPlayerTurn ? "Player" : "Enemy")}";
        Debug.Log($"Lượt: {(isPlayerTurn ? "Player" : "Enemy")}");
    }
}
