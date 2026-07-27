using System.Collections.Generic;
using UnityEngine;

public class PathFinder
{
    public static List<Vector3> GetPath(Grid grid, Vector3 start, Vector3 end)
    {
        List<Vector3> path = new List<Vector3>();

        Vector3Int startCell = grid.WorldToCell(start);
        Vector3Int endCell = grid.WorldToCell(end);

        List<Vector3Int> cellPath = Calculate(startCell, endCell);

        foreach (Vector3Int cell in cellPath)
        {
            path.Add(grid.GetCellCenterWorld(cell));
        }

        return path;
    }

    private static List<Vector3Int> Calculate(Vector3Int start, Vector3Int end)
    {
        List<Vector3Int> path = new List<Vector3Int>();
        Vector3Int current = start;

        while (current.x != end.x)
        {
            current.x += (end.x > current.x) ? 1 : -1;
            path.Add(current);
        }
        while (current.z != end.z)
        {
            current.z += (end.z > current.z) ? 1 : -1;
            path.Add(current);
        }

        return path;
    }

    public Vector3 GetClosestGrid(Enemy me, Character target)
    {
        Vector3 targetPos = target.transform.position;
        Vector3[] gridList = new Vector3[]
        { targetPos + Vector3.forward,
          targetPos + Vector3.back,
          targetPos + Vector3.left,
          targetPos + Vector3.right};

        Vector3 bestGrid = me.transform.position;
        float minDis = float.MaxValue;

        foreach(var grid in gridList)
        {
            if (MapManager.instance.IsMovable(grid, me.transform.position))
            {
                float dis = (me.transform.position - grid).sqrMagnitude;
                if(dis < minDis)
                {
                    minDis = dis;
                    bestGrid = grid;
                }
            }
        }

        return bestGrid;
    }

    public Vector3 GetLimitedGrid(Enemy me, Character target, int moveCount)
    {
        Vector3 goal = GetClosestGrid(me, target);
        List<Vector3> fullPath = GetPath(InputManager.instance.grid, me.transform.position, goal);

        int steps = Mathf.Min(fullPath.Count, moveCount);
        Vector3 limitedGoal = fullPath[steps - 1];

        return limitedGoal;
    }
}
