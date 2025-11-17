using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TurnBaseManager : MonoBehaviour
{
    public List<Unit> PlayerTeam = new List<Unit>();
    public List<Unit> EnemyTeam = new List<Unit>();
    public Cinemachine.CinemachineVirtualCamera battleCamera;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if(Input.GetKeyDown(KeyCode.B))
        {
            StartCoroutine(PlayerAttack());
        }
    }

    IEnumerator PlayerAttack()
    {
        foreach (Unit unit in PlayerTeam)
        {
            battleCamera.Follow = unit.transform;
            unit.PerformAttack();
            yield return new WaitForSeconds(0.5f);
        }
    }
}
