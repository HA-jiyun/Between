using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Unit : MonoBehaviour
{
    public Vector3Int gridPos;
    public bool isEnemy = false;

    [Header("UnitData")]
    public string code;
    public string myName;
    public UnitElement element;
    public int maxHP;
    public int basicDamage;
    public int specialDamage;
    public int attackDis;
    public int moveRange;

    [Header("Animation")]
    public float moveSpeed = 4f;
    public bool isMoving = false;
    protected List<Vector3> pathPoints = new();
    protected Animator animator;

    [Header("Runtime")]
    public int currentHP;
    public Sprite myImage;
    public Sprite myElementImage;

    private bool _isActed = false;
    public bool isActed
    {
        get => _isActed;
        set
        {
            _isActed = value;
            if (_isActed == true)
            {
                TurnManager.instance.CountCharacterActed();
            }
        }
    }

    protected virtual void Awake()
    {
        animator = GetComponentInChildren<Animator>();
        isMoving = false;
        animator.SetBool("isMoving", false);
    }

    protected virtual void Start()
    {
        InputManager.instance.targetPosition = transform.position;

        gridPos = MapManager.instance.ToGridPos(transform.position);
        MapManager.instance.AddPosition(this, gridPos);

        InitStatus();
    }

    protected void InitStatus()
    {
        UnitData myData = DataManager.instance.GetUnitData(code.Trim());
        if (myData != null)
        {
            myName = myData.myName;
            element = myData.element;
            maxHP = myData.hp;
            currentHP = maxHP;
            basicDamage = myData.basicDamage;
            specialDamage = myData.specialDamage;
            attackDis = myData.dis;
            moveRange = myData.moveRange;
        }
    }

    public void Look(Transform target)
    {
        Vector3 targetPos = new Vector3(target.position.x, target.position.y, target.position.z);
        transform.LookAt(targetPos);
    }

    public virtual IEnumerator Move(Vector3 newPos)
    {
        if (isMoving) yield break;

        pathPoints = PathFinder.GetPath(InputManager.instance.grid, transform.position, newPos);
        if (pathPoints.Count > 0)
        {
            yield return StartCoroutine(MoveRoutine(newPos));
        }
    }
    IEnumerator MoveRoutine(Vector3 targetPos)
    {
        isMoving = true;
        animator.SetBool("isMoving", true);

        Vector3Int oldPos = gridPos;
        Vector3Int newPos = MapManager.instance.ToGridPos(targetPos);

        MapManager.instance.RemovePosition(oldPos);
        MapManager.instance.AddPosition(this, newPos);

        foreach (Vector3 pos in pathPoints)
        {
            Vector3 lookDir = (pos - transform.position).normalized;
            if (lookDir != Vector3.zero)
                transform.forward = new Vector3(lookDir.x, 0, lookDir.z);

            while (Vector3.Distance(transform.position, pos) > 0.05f)
            {
                transform.position = Vector3.MoveTowards(transform.position, pos, moveSpeed * Time.deltaTime);
                yield return null;
            }
            transform.position = pos;
        }

        gridPos = newPos;
        isMoving = false;
        animator.SetBool("isMoving", false);

    }

    public bool CanAttack()
    {
        Vector3Int myPos = MapManager.instance.ToGridPos(transform.position);
        List<Vector3Int> list = GetAttackablePositions(myPos, attackDis, attackDis);

        foreach (Vector3Int pos in list)
        {
            if (MapManager.instance.unitList.ContainsKey(pos))
            {
                Unit target = MapManager.instance.unitList[pos];
                if (target.isEnemy != isEnemy)
                {
                    MapManager.instance.targetEnemies.Add(target);
                    return true;
                }
            }
        }
        return false;
    }
    public bool CanAttack(Vector3Int goalPos)
    {
        List<Vector3Int> list = GetAttackablePositions(goalPos, attackDis, attackDis);

        foreach (Vector3Int pos in list)
        {
            if (MapManager.instance.unitList.ContainsKey(pos))
            {
                Unit target = MapManager.instance.unitList[pos];
                if (target != null && target.isEnemy != isEnemy)
                {
                    MapManager.instance.targetEnemies.Add(target);
                    return true;
                }
            }
        }
        return false;
    }

    public List<Vector3Int> GetAttackablePositions(Vector3Int start, int min, int max)
    {
        List<Vector3Int> list = new List<Vector3Int>();
        Queue<Vector3Int> queue = new Queue<Vector3Int>();
        Dictionary<Vector3Int, int> cost = new Dictionary<Vector3Int, int>();

        Vector3Int[] directions = {
            Vector3Int.forward, // (0, 0, 1)
            Vector3Int.back,
            Vector3Int.left,    // (-1, 0, 0)
            Vector3Int.right
        };

        queue.Enqueue(start);
        cost[start] = 0;

        while (queue.Count > 0)
        {
            Vector3Int current = queue.Dequeue();
            foreach (Vector3Int dir in directions)
            {
                Vector3Int neighbor = current + dir;
                int newCost = cost[current] + 1;

                if (newCost <= max)
                {
                    if (!cost.ContainsKey(neighbor) || newCost < cost[neighbor])
                    {
                        cost[neighbor] = newCost;
                        queue.Enqueue(neighbor);

                        if (newCost >= min && !list.Contains(neighbor))
                            if (MapManager.instance.IsValidMap(neighbor))
                                list.Add(neighbor);
                    }
                }
            }
        }

        return list;
    }

    protected IEnumerator AttackAnimationRoutine()
    {
        animator.SetTrigger("isAttacking");
        yield return null;

        AnimatorStateInfo stateInfo = animator.GetCurrentAnimatorStateInfo(0);
        float length = stateInfo.length;
        yield return new WaitForSeconds(length + 1.0f);
    }

    public void TakeDamage(int dmg)
    {
        currentHP -= dmg;
        if (currentHP <= 0)
        {
            Die();
        }
    }

    protected virtual void Die()
    {
        Vector3Int pos = Vector3Int.FloorToInt(transform.position);
        MapManager.instance.RemovePosition(pos);

        Destroy(gameObject);

    }
}
