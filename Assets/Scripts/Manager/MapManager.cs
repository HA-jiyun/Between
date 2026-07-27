using System.Collections.Generic;
using UnityEngine;

public class MapManager : MonoBehaviour
{
    public static MapManager instance;

    [Header("Plane Info")]
    public GameObject plane;
    private float minX, minZ, maxX, maxZ;

    [Header("Map Info")]
    private List<GameObject> tiles = new();
    private List<Vector3Int> moveRangePositions = new();
    public Dictionary<Vector3Int, Unit> unitList = new();
    private HashSet<Vector3Int> blockList = new();
    public List<Unit> targetEnemies = new();

    [Header("Tile Settings")]
    [SerializeField] private GameObject tilePrefab;
    [SerializeField] private int poolSize = 100;
    private Queue<GameObject> tilePool = new();

    [SerializeField] private Material moveMaterial;
    [SerializeField] private Material playerAttackMaterial;
    [SerializeField] private Material enemyAttackMaterial;

    [Header("Line Settings")]
    [SerializeField] private GameObject linePrefab;
    private Queue<GameObject> linePool = new();
    private List<GameObject> activeLines = new();

    private Vector3[] linePoints = new Vector3[20];
    [SerializeField] private AnimationCurve heightCurve;
    [SerializeField] private float heightFactor = 0.3f;
    [SerializeField] private float minHeight = 1.5f;
    [SerializeField] private float maxHeight = 5.0f;

    public enum TileType { Move, PlayerAttack, EnemyAttack }

    private void Awake()
    {
        instance = this;

        Renderer renderer = plane.GetComponent<Renderer>();
        Bounds bounds = renderer.bounds;

        minX = Mathf.RoundToInt(bounds.min.x + 0.1f);
        maxX = Mathf.RoundToInt(bounds.max.x - 0.1f);
        minZ = Mathf.RoundToInt(bounds.min.z + 0.1f);
        maxZ = Mathf.RoundToInt(bounds.max.z - 0.1f);

        SetTilePool(poolSize);
        Debug.Log($"MinX: {minX}, MaxX: {maxX} | MinZ: {minZ}, MaxZ: {maxZ}");
    }

    public bool IsMovable(Vector3 pos, Vector3 start)
    {
        Vector3Int gridPos = ToGridPos(pos);
        Vector3Int gridStart = ToGridPos(start);
        return IsMovable(gridPos, gridStart);
    }
    public bool IsMovable(Vector3Int pos, Vector3Int start)
    {
        if (!IsValidMap(pos)) return false;
        if (blockList.Contains(pos)) return false;
        if (pos == start) return true;
        if (unitList.ContainsKey(pos)) return false;

        return true;
    }

    public bool IsAttackable(Vector3Int pos)
    {
        if (!IsValidMap(pos)) return false;
        if (blockList.Contains(pos)) return false;

        return true;
    }

    public bool IsValidMap(Vector3Int pos)
    {
        return pos.x >= minX && pos.x <= maxX
            && pos.z >= minZ && pos.z <= maxZ;
    }

    public void AddPosition(Unit unit, Vector3Int newPos)
    {
        unitList[newPos] = unit;
    }
    public void RemovePosition(Vector3Int oldPos)
    {
        if (unitList.ContainsKey(oldPos))
            unitList.Remove(oldPos);
    }

    public Vector3Int ToGridPos(Vector3 pos)
    {
        return new Vector3Int(Mathf.FloorToInt(pos.x), 0, Mathf.FloorToInt(pos.z));
    }
    public Vector3Int ToGridPos(Vector3 pos, bool isMouse)
    {
        return new Vector3Int(Mathf.FloorToInt(pos.x + 0.5f), 0, Mathf.FloorToInt(pos.z + 0.5f));
    }

    public List<Vector3Int> SetMoveRange(Vector3Int start, int moveRange)
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

        list.Add(start);
        while (queue.Count > 0)
        {
            Vector3Int current = queue.Dequeue();

            foreach (Vector3Int dir in directions)
            {
                Vector3Int neighbor = current + dir;
                int newCost = cost[current] + 1;

                if (newCost <= moveRange && IsMovable(neighbor, start))
                {
                    if (!cost.ContainsKey(neighbor) || newCost < cost[neighbor])
                    {
                        cost[neighbor] = newCost;
                        queue.Enqueue(neighbor);

                        if (!list.Contains(neighbor))
                            list.Add(neighbor);
                    }
                }
            }
        }

        return list;
    }
    public HashSet<Vector3Int> SetAttackRange(List<Vector3Int> moveRange, int min, int max)
    {
        HashSet<Vector3Int> attackRange = new HashSet<Vector3Int>();

        foreach (Vector3Int pos in moveRange)
        {
            for (int x = -max; x <= max; x++)
            {
                for (int z = -max; z <= max; z++)
                {
                    int dis = Mathf.Abs(x) + Mathf.Abs(z);
                    if (dis >= min && dis <= max)
                    {
                        Vector3Int targetPos = new Vector3Int(pos.x + x, pos.y, pos.z + z);

                        if (IsAttackable(targetPos)) 
                            attackRange.Add(targetPos);
                    }
                }
            }
        }

        return attackRange;
    }

    public void ShowMoveRange(Unit unit, int moveRange, int minDis, int maxDis)
    {
        Vector3Int start = ToGridPos(unit.transform.position);
        moveRangePositions = SetMoveRange(start, moveRange);
        HashSet<Vector3Int> attackList = SetAttackRange(moveRangePositions, minDis, maxDis);
        TileType unitType = unit.isEnemy ? TileType.EnemyAttack : TileType.PlayerAttack;

        foreach (Vector3Int pos in moveRangePositions)
        {
            Vector3 worldPos = new(pos.x, 0.01f, pos.z);

            GameObject tile = GetTile(TileType.Move);
            tile.transform.SetPositionAndRotation(worldPos, Quaternion.identity);
            tile.SetActive(true);

            tiles.Add(tile);
        }
        foreach (Vector3Int pos in attackList)
        {
            if (moveRangePositions.Contains(pos)) continue;

            Vector3 worldPos = new(pos.x, 0.01f, pos.z);

            GameObject tile = GetTile(unitType);
            tile.transform.SetPositionAndRotation(worldPos, Quaternion.identity);
            tile.SetActive(true);

            tiles.Add(tile);
        }
    }
    public void HideMoveRange()
    {
        foreach (GameObject t in tiles)
        {
            t.SetActive(false);
            t.tag = "MoveRange";
            tilePool.Enqueue(t);
        }
        tiles.Clear();
    }

    private void SetTilePool(int size)
    {
        for (int i = 0; i < size; i++)
        {
            GameObject tile = Instantiate(tilePrefab, transform);
            tile.SetActive(false);
            tilePool.Enqueue(tile);
        }
        tiles.Clear();
    }
    private GameObject GetTile(TileType type)
    {
        if(tilePool.Count == 0)
            SetTilePool(10);

        GameObject tile = tilePool.Dequeue();
        MeshRenderer mr = tile.GetComponent<MeshRenderer>();
        mr.material = type switch
        {
            TileType.PlayerAttack => playerAttackMaterial,
            TileType.EnemyAttack => enemyAttackMaterial,
            _ => moveMaterial
        };

        if (type == TileType.Move)
            tile.tag = "MoveRange";
        else
            tile.tag = "AttackRange";

        return tile;
    }

    private GameObject GetLine()
    {
        GameObject line;

        if (linePool.Count > 0)
            line = linePool.Dequeue();
        else
            line = Instantiate(linePrefab, transform);

        if (!activeLines.Contains(line))
            activeLines.Add(line);

        line.gameObject.SetActive(true);
        return line;
    }

    public void UpdateAggroLines(Vector3 pos)
    {
        ClearAggroLines();
        Vector3Int gridPos = ToGridPos(pos);

        if (!moveRangePositions.Contains(gridPos)) return;
        foreach (Unit enemy in GameManager.instance.allEnemies)
        {
            List<Vector3Int> enemyMove = SetMoveRange(enemy.gridPos, enemy.moveRange);
            HashSet<Vector3Int> enemyThreat = SetAttackRange(enemyMove, enemy.attackDis, enemy.attackDis);

            if (enemyThreat.Contains(gridPos))
            {
                DrawLineBetween(enemy.transform.position, pos);
            }
        }
    }

    private void DrawLineBetween(Vector3 startPos, Vector3 endPos)
    {
        GameObject line = GetLine();
        LineRenderer mainLine = line.GetComponent<LineRenderer>();
        LineRenderer outline = line.transform.GetChild(0).GetComponent<LineRenderer>();

        Vector3 p0 = startPos + Vector3.up * 0.6f;
        Vector3 p1 = endPos + Vector3.up * 0.6f;

        float distance = Vector3.Distance(p0, p1);
        float calculatedHeight = Mathf.Clamp(distance * heightFactor, minHeight, maxHeight);

        int pointCount = linePoints.Length;
        for (int i = 0; i < pointCount; i++)
        {
            float t = i / (float)(pointCount - 1);
            Vector3 currentPos = Vector3.Lerp(p0, p1, t);
            currentPos.y += heightCurve.Evaluate(t) * calculatedHeight;

            linePoints[i] = currentPos;
        }
        mainLine.positionCount = pointCount;
        mainLine.SetPositions(linePoints);

        outline.positionCount = pointCount;
        outline.SetPositions(linePoints);
    }

    public void ClearAggroLines()
    {
        foreach (var line in activeLines)
        {
            line.gameObject.SetActive(false);
            linePool.Enqueue(line);
        }
        activeLines.Clear();
    }
}
