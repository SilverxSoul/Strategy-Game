using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;


public class AIEnemy : MonoBehaviour
{
    private SearchResult searchResult;
    //private List<List<Unit>> ListOfListUnit;// lưu lại các ListUnit copy để xóa đi sau mỗi lần chọn ra kết quả tốt nhất, giải phóng bộ nhớ 
    private void Update()
    {
        if (!TurnBaseManager.Instance.isPlayerTurn)
        {
            

            // cần sử dụng clone list TurnBaseManager.Instance.PlayerTeam, list TurnBaseManager.Instance.EnemyTeam vì trong quá trình đánh giá trạng thái chỉ là giả lập không sử dụng dữ liệu thật, mà ta clone List reference type thế nào đây ?
            if (GameManager.Instance.IsEnemyActionInProgress == false)
            {
                List<UnitCopy> PlayerUnitCopies = CreateListCopy(TurnBaseManager.Instance.PlayerTeam);
                List<UnitCopy> EnemyUnitCopies = CreateListCopy(TurnBaseManager.Instance.EnemyTeam);
                List<Vector2Int> OccupiedCell = CreateCopyOccupiedCells();
                //foreach (Vector2Int cell in OccupiedCell) Debug.Log(cell); //kiểm tra xem có tạo đúng bản sao các ô bị chiếm không (đúng rồi)
                searchResult = Minimax(OccupiedCell, PlayerUnitCopies, EnemyUnitCopies, 2, TurnBaseManager.Instance.isPlayerTurn);//depth tối đa là 2, xem Player là Max, Enemy là Min
                                                                                                                                  //Debug.Log(searchResult.BestAction.targetPosition.ToString());
                                                                                                                                  //Debug.Log(searchResult.BestAction.unitCopy.unitCopied);
                Debug.Log("Best move score:" + searchResult.Score.ToString());
                StartCoroutine(GameManager.Instance.EnemyTeamAction(searchResult.BestAction.unitCopy.unitCopied, searchResult.BestAction.targetPosition));
            }
        }
    }
    public bool IsEndGame(List<UnitCopy> PlayerUnits, List<UnitCopy> EnemyUnits)
    {
        bool PlayerAllDead = true;
        bool EnemyAllDead = true;
        foreach (UnitCopy UnitCopy in PlayerUnits)
        {
            if (UnitCopy.isAlive)
            {
                PlayerAllDead= false;
                break;
            }
        }
        foreach (UnitCopy UnitCopy in EnemyUnits)
        {
            if (UnitCopy.isAlive)
            {
                EnemyAllDead = false;
                break;
            }
        }
        if (PlayerAllDead || EnemyAllDead)
            return true;
        else 
            return false;
    }
    public List<UnitCopy> CreateListCopy(List<Unit> Units)
    {
        List<UnitCopy> unitCopies = new List<UnitCopy>();
        foreach (Unit unit in Units)
        {
            unitCopies.Add(unit.CreateCopy());
        }
        return unitCopies;
    }

    public List<Vector2Int> CreateCopyOccupiedCells()// tạo bản sao các ô bị chiếm trên grid chứ không dùng bản gốc
    {
        List<Vector2Int> gridOccupiedCellsCopy = GridManager.Instance.occupied.Keys.ToList();
        return gridOccupiedCellsCopy;
    }
    public bool IsValidEmptyCopyOccupiedCells(Vector2Int target,List<Vector2Int> gridOccupiedCellsCopy)//kiểm tra một ô target có bị chiếm trong bản sao các ô bị chiếm không, có trong phạm vi grid thực tế không
    {
        if(target.x <0 || target.x> GridManager.Instance.width ||target.y<0|| target.y>GridManager.Instance.height) return false;
        return !gridOccupiedCellsCopy.Contains(target);

    }
    public SearchResult Minimax(List<Vector2Int> OccupiedCellsCopy, List<UnitCopy> PlayerUnits, List<UnitCopy> EnemyUnits, int depth, bool maximizingPlayer)
    {
        return alphabeta(OccupiedCellsCopy,PlayerUnits, EnemyUnits, depth, int.MinValue, int.MaxValue, maximizingPlayer);
    }

    public SearchResult alphabeta(List<Vector2Int>OccupiedCellsCopy,List<UnitCopy> PlayerUnits, List<UnitCopy> EnemyUnits, int depth, int alpha, int beta,bool maximizingPlayer)
    {
        if (depth == 0 || IsEndGame(PlayerUnits, EnemyUnits))
        {
            return new SearchResult(EvaluateState(PlayerUnits,EnemyUnits), new Action(new UnitCopy(Team.Player,Type.Sword,0,0,AttackShape.Melee,0,new Vector2Int(0,0),0,null), new Vector2Int(0, 0)));// cần kiểm tra chỗ nào sử dụng đến Action thì unitCopied của nó không được phép null, nếu null thì bỏ qua vì dữ liệu trong unitCopied này là dữ liệu rác thôi
        }
        if (maximizingPlayer)
        {
            foreach (Action action in GetAllPossibleAction(PlayerUnits, OccupiedCellsCopy))
            {
                if(action.unitCopy.unitCopied!=null)
                {
                    var State = ResultAction(OccupiedCellsCopy,PlayerUnits, EnemyUnits, action, maximizingPlayer);

                    //phục vụ debug
                    UnitCopy a = new UnitCopy();
                    if (action.unitCopy.team == Team.Player)
                    {
                        for (int i = 0; i < PlayerUnits.Count; i++)
                        {
                            if (PlayerUnits[i].unitCopied == action.unitCopy.unitCopied)
                            {
                                a = PlayerUnits[i];
                                break;
                            }
                        }
                    }
                    else
                    {
                        for (int i = 0; i < EnemyUnits.Count; i++)
                        {
                            if (EnemyUnits[i].unitCopied == action.unitCopy.unitCopied)
                            {
                                a = EnemyUnits[i];
                                break;
                            }
                        }
                    }
                    Debug.Log("State after action " + action.unitCopy.unitCopied.gameObject.name + "from " + a.currentGridPos + " to pos:" + action.targetPosition);
                    //foreach (UnitCopy unit in State.PlayerUnits)
                    //{
                    //    Debug.Log(unit.unitCopied.gameObject.name + " pos :" + unit.currentGridPos);
                    //}
                    //foreach (UnitCopy unit in State.EnemyUnits)
                    //{
                    //    Debug.Log(unit.unitCopied.gameObject.name + " pos :" + unit.currentGridPos);
                    //}
                    //foreach (Vector2Int occupiedCell in State.OccupiedCellsCopy)
                    //{
                    //    Debug.Log("occupied cell:" + occupiedCell);
                    //}


                    ////
                    searchResult = new SearchResult(alphabeta(State.OccupiedCellsCopy,State.PlayerUnits, State.EnemyUnits, depth - 1, alpha, beta, false).Score, action);
                    alpha = Mathf.Max(alpha, searchResult.Score);
                    searchResult.Score = alpha;
                    
                    if (beta <= alpha)
                    {
                        break; // Beta cut-off
                    }
                }
                //Debug.Log("Depth" + depth.ToString() + ": valueInProcess :" + searchResult.Score);
            }
            Debug.Log("Depth" + depth.ToString() + ": value :" + searchResult.Score);
            Debug.Log("Beta value :" + beta + "Alpha value :" + alpha);
            return searchResult;
        }
        else
        {
            foreach (Action action in GetAllPossibleAction(EnemyUnits, OccupiedCellsCopy))
            {
                if (action.unitCopy.unitCopied != null)
                {
                    var State = ResultAction(OccupiedCellsCopy,PlayerUnits, EnemyUnits, action, maximizingPlayer);

                    //phục vụ debug
                    UnitCopy a = new UnitCopy();
                    if (action.unitCopy.team == Team.Player)
                    {
                        for (int i = 0; i < PlayerUnits.Count; i++)
                        {
                            if (PlayerUnits[i].unitCopied == action.unitCopy.unitCopied)
                            {
                                a = PlayerUnits[i];
                                break;
                            }
                        }
                    }
                    else
                    {
                        for (int i = 0; i < EnemyUnits.Count; i++)
                        {
                            if (EnemyUnits[i].unitCopied == action.unitCopy.unitCopied)
                            {
                                a = EnemyUnits[i];
                                break;
                            }
                        }
                    }
                    Debug.Log("State after action " + action.unitCopy.unitCopied.gameObject.name + "from " + a.currentGridPos + " to pos:" + action.targetPosition);
                    //foreach (UnitCopy unit in State.PlayerUnits)
                    //{
                    //    Debug.Log(unit.unitCopied.gameObject.name + " pos :" + unit.currentGridPos);
                    //    Debug.Log(unit.unitCopied.gameObject.name + " HP :" + unit.currentHP);
                    //    Debug.Log(unit.unitCopied.gameObject.name + " IsAlive :" + unit.isAlive);
                    //}
                    //foreach (UnitCopy unit in State.EnemyUnits)
                    //{
                    //    Debug.Log(unit.unitCopied.gameObject.name + " pos :" + unit.currentGridPos);
                    //    Debug.Log(unit.unitCopied.gameObject.name + " HP :" + unit.currentHP);
                    //    Debug.Log(unit.unitCopied.gameObject.name + " IsAlive :" + unit.isAlive);
                    //}
                    //foreach (Vector2Int occupiedCell in State.OccupiedCellsCopy)
                    //{
                    //    Debug.Log("occupied cell:" + occupiedCell);
                    //}
                    searchResult = new SearchResult(alphabeta(State.OccupiedCellsCopy, State.PlayerUnits, State.EnemyUnits, depth - 1, alpha, beta, true).Score, action);
                    beta = Mathf.Min(beta, searchResult.Score);
                    searchResult.Score = beta;


                    if (beta <= alpha)
                    {
                        break; // Alpha cut-off
                    }
                }
                //Debug.Log("Depth" + depth.ToString() + ": valueInProcess :" + searchResult.Score);
            }
            Debug.Log("Depth" + depth.ToString() + ": value :" + searchResult.Score);
            Debug.Log("Beta value :" + beta + "Alpha value :" + alpha);
            return searchResult;
        }
    }

    private int GetTypeUnitValue(Type t)
    {
        switch (t)
        {
            case Type.Sword:
                return 100;
            case Type.Magic:
                return 300;
            case Type.Shuriken:
                return 300;
            default: return 0;
        }
    }
    private int EvaluateState(List<UnitCopy> PlayerUnits, List<UnitCopy> EnemyUnits)// đánh giá trạng thái của game qua giá trị của các unit còn sống của 2 bên
    {
        int score = 0;
        foreach (UnitCopy unit in PlayerUnits)
        {
            if (unit.isAlive)
            {
                score += unit.currentHP*10;
                score += GetTypeUnitValue(unit.type);
            }
        }
        foreach (UnitCopy unit in EnemyUnits)
        {
            if (unit.isAlive)
            {
                score -= unit.currentHP * 10;
                score -= GetTypeUnitValue(unit.type);
            }
        }
        Debug.Log(score);
        return score;
    }

    public List<Action>GetAllPossibleAction(List<UnitCopy> units,List<Vector2Int> OccupiedCellsCopy)
    {
        List<Action> possibleActions = new List<Action>();
        // Lấy tất cả các đơn vị của AI (Enemy)
        foreach (UnitCopy unit in units)
        {
            if (!unit.isAlive) continue; // Bỏ qua các đơn vị đã chết
            //Debug.Log("Unit :"+unit.unitCopied.gameObject.name);
            // Lấy tất cả các vị trí mà đơn vị có thể di chuyển đến
            List<Vector2Int> possibleMovePositions = new List<Vector2Int>();
            int minX = Mathf.Max(0, unit.currentGridPos.x - unit.moveRange);
            int maxX = Mathf.Min(GridManager.Instance.width - 1, unit.currentGridPos.x + unit.moveRange);
            int minY = Mathf.Max(0, unit.currentGridPos.y - unit.moveRange);
            int maxY = Mathf.Min(GridManager.Instance.height - 1, unit.currentGridPos.y + unit.moveRange);

            for (int x = minX; x <= maxX; x++)
            {
                for (int y = minY; y <= maxY; y++)
                {
                    Vector2Int pos = new Vector2Int(x, y);
                    if (pos == unit.currentGridPos) continue; // Bỏ qua vị trí hiện tại

                    if (IsValidEmptyCopyOccupiedCells(pos, OccupiedCellsCopy))
                    {
                        possibleMovePositions.Add(pos);
                        //Debug.Log("  possible position:" + pos); 
                    }
                }
            }

            foreach (Vector2Int movePos in possibleMovePositions)
            {
                possibleActions.Add(new Action(unit, movePos));
            }
        }
        return possibleActions;
    }

    public (List<Vector2Int> OccupiedCellsCopy,List<UnitCopy> PlayerUnits, List<UnitCopy> EnemyUnits) ResultAction(List<Vector2Int> OccupiedCellsCopy, List<UnitCopy> PlayerUnits, List<UnitCopy> EnemyUnits,Action action, bool maximizingPlayer)
    {
        // Tạo bản sao của danh sách đơn vị để tránh thay đổi trạng thái gốc
        //List<Unit> newPlayerUnits = new List<Unit>();
        //foreach (unit unit in PlayerUnits)
        //{
        //    Unit newUnit = Instantiate(unit);//nghe nói không thể dùng cho component, vậy nếu là instantiate một gameObject luôn thì ta chỉ cần Inactive nó đi được không ?
        //    newUnit.currentHP = unit.currentHP;
        //    newPlayerUnits.Add(newUnit);
        //}

        //List<Unit> newEnemyUnits = new List<Unit>();
        //foreach (Unit unit in EnemyUnits)
        //{
        //    Unit newUnit = Instantiate(unit);
        //    newUnit.currentHP = unit.currentHP;
        //    newEnemyUnits.Add(newUnit);
        //}

        //Lưu lại các bản sao để xóa sau khi dùng xong
        //ListOfListUnit.Add(newPlayerUnits);
        //ListOfListUnit.Add(newEnemyUnits);


        //Thay thế phần comment trên
        // tạo một bản sao mới
        List<UnitCopy> newPlayerUnits = new List<UnitCopy>();
        List<UnitCopy> newEnemyUnits = new List<UnitCopy>();
        List<Vector2Int> newOccupiedCells = new List<Vector2Int>();
        foreach (Vector2Int cell in OccupiedCellsCopy)
        {
            newOccupiedCells.Add(cell);
        }
        foreach (UnitCopy unit in PlayerUnits)
        {
            newPlayerUnits.Add(unit);
        }
        foreach (UnitCopy unit in EnemyUnits)
        {
            newEnemyUnits.Add(unit);
        }
        // Tìm đơn vị tương ứng trong danh sách mới
        UnitCopy actingUnit =new UnitCopy();// phải việc khởi tạo =new UnitCopy() để có giá trị default cho struct tránh lỗi biến chưa được gán giá trị, nếu ta bỏ  =new UnitCopy() thì sẽ bị lỗi dùng biến không gán giá trị. Nó sẽ không trả về giá trị mặc định của loại struct này khi ta chưa thêm =new UnitCopy() !!
        foreach (UnitCopy unit in newEnemyUnits)
        {
            if (unit.currentGridPos == action.unitCopy.currentGridPos)
            {
                actingUnit = unit;
                break;
            }
        }

        foreach (UnitCopy unit in newPlayerUnits)
        {
            if (unit.currentGridPos == action.unitCopy.currentGridPos)
            {
                actingUnit = unit;
                break;
            }
        }
        if (actingUnit.unitCopied!=null )
        {
            // Cập nhật vị trí của đơn vị sau khi di chuyển
            actingUnit.currentGridPos = action.targetPosition;// struct là value type nên bạn thay đổi trên actingUnit thì không đồng nghĩa thay đổi unitCopy mà nó copy từ List newPlayerUnits hoặc List newEnemyUnits. thế nên ta phải cập nhật thẳng nó trên List mà nó copy nữa vì hàm này trả về List sau khi cập nhật
            //cập nhật lại CopyOccupiedCells
            newOccupiedCells.Remove(action.unitCopy.currentGridPos);
            newOccupiedCells.Add(action.targetPosition);
            //foreach (Vector2Int cell in newOccupiedCells)
            //{
            //    Debug.Log("newOccupiedCells:"+cell);
            //}
            if (actingUnit.team == Team.Player)
            {
                for (int i = 0; i < newPlayerUnits.Count; i++)
                {
                    if (newPlayerUnits[i].currentGridPos == action.unitCopy.currentGridPos)
                    {
                        newPlayerUnits[i] = actingUnit;
                        break;
                    }
                }
            }
            else
            {
                for (int i = 0; i < newEnemyUnits.Count; i++)
                {
                    if (newEnemyUnits[i].currentGridPos == action.unitCopy.currentGridPos)
                    {
                        newEnemyUnits[i] = actingUnit;
                        break;
                    }
                }
            }
            
            if (maximizingPlayer)
            {
                // Giả lập tấn công nếu có kẻ địch trong phạm vi tấn công của unit người chơi
                foreach (UnitCopy playerUnit in newPlayerUnits)
                {
                    UnitCopy EnemyInAttackRange = GetUnitCopyInAttackRange(playerUnit.currentGridPos, playerUnit.attackShape,newEnemyUnits, playerUnit);
                    if (playerUnit.isAlive && EnemyInAttackRange.unitCopied != null)// EnemyInAttackRange.unitCopied != null là ám chỉ việc không tìm thấy enemy nào nằm trên AttackRange, tôi không dùng EnemyInAttackRange!= null vì struct không thể so sánh null được
                    {
                        EnemyInAttackRange.currentHP -= playerUnit.damage;
                        if (EnemyInAttackRange.currentHP <= 0)
                        {
                            EnemyInAttackRange.currentHP = 0;
                        }
                        //Struct là kiểu tham trị nên ta cần cập nhật lại currentHP EnemyInAttackRange tương ứng trong newEnemyUnits
                        for (int i = 0; i < newEnemyUnits.Count; i++)
                        {
                            if (newEnemyUnits[i].currentGridPos == EnemyInAttackRange.currentGridPos)
                            {
                                newEnemyUnits[i] = EnemyInAttackRange;
                                break;
                            }
                        }
                    }
                }
            }
            else
            {
                // Giả lập tấn công nếu có unit người chơi trong phạm vi tấn công của unit địch
                foreach (UnitCopy enemyUnit in newEnemyUnits)
                {
                    UnitCopy PlayerInAttackRange = GetUnitCopyInAttackRange(enemyUnit.currentGridPos, enemyUnit.attackShape, newPlayerUnits, enemyUnit);
                    if (enemyUnit.isAlive && PlayerInAttackRange.unitCopied != null)
                    {
                        //Debug.Log(PlayerInAttackRange.unitCopied.name+" Attacked");
                        PlayerInAttackRange.currentHP -= enemyUnit.damage;
                        if (PlayerInAttackRange.currentHP <= 0)
                        {
                            PlayerInAttackRange.currentHP = 0;
                        }
                        //Struct là kiểu tham trị nên ta cần cập nhật lại currentHP PlayerInAttackRange tương ứng trong newPlayerUnits
                        for (int i = 0; i < newPlayerUnits.Count; i++)
                        {
                            if (newPlayerUnits[i].currentGridPos == PlayerInAttackRange.currentGridPos)
                            {
                                newPlayerUnits[i] = PlayerInAttackRange;
                                break;
                            }
                        }
                    }
                }
            }
        }

        return (newOccupiedCells, newPlayerUnits, newEnemyUnits);
    }

    public UnitCopy GetUnitCopyInAttackRange(Vector2Int unitAttackPosition, AttackShape attackShape,List<UnitCopy> units,UnitCopy unitAttack)// units ở đây là List UnitCopy của 1 phe (Player hoặc Enemy)
    {
        GridManager.Instance.AttackRange(unitAttackPosition, attackShape);// AttackRange của actingUnit sẽ được lưu trữ vào attackCells của GridManager
        foreach (Vector2Int pos in GridManager.Instance.attackCells)
        {
            foreach (UnitCopy unit in units)
            {
                if (unit.currentGridPos == pos && unit.team != unitAttack.team && unit.isAlive)
                {
                    return unit;
                }
            }
        }
        return new UnitCopy();
    }

    //Struct hỗ trợ alpha-beta pruning Tìm kiếm bước đi tốt nhất
    public struct SearchResult
    {
        public int Score;     // giá trị đánh giá
        public Action BestAction; // Hành động đi tốt nhất

        public SearchResult(int score, Action bestMove)
        {
            Score = score;
            BestAction = bestMove;
        }
    }
    public struct Action// sẽ sử dụng UnitCopy vì ta không thể sử dụng Unit gốc trong quá trình giả lập đánh giá trạng thái,UnitCopy vẫn có lưu lại Unit gốc nên ta vẫn có thể truy ngược lại Unit gốc qua unitCopy của Action
    {
        public UnitCopy unitCopy; //UnitCopy cho unit thực hiện hành động 
        public Vector2Int targetPosition; // Vị trí di chuyển đến trong hành động

        public Action(UnitCopy unitCopy, Vector2Int targetPosition)
        {
            this.unitCopy = unitCopy;
            this.targetPosition = targetPosition;//tọa độ trên grid
        }
    }
    
}
