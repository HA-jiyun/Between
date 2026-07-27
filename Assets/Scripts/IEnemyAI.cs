using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IEnemyAI
{
    IEnumerator Attack(Enemy me);
}

public class EasyAI : IEnemyAI
{
    private PathFinder pathFinder = new();

    public IEnumerator Attack(Enemy me)
    {
        List<Character> targets = GameManager.instance.allCharacters;
        Character target = EasyFinder(me, targets);

        if (target != null && !me.CanAttack())
        {
            Vector3 goal = pathFinder.GetLimitedGrid(me, target, me.moveRange);
            yield return me.StartCoroutine(me.Move(goal));
        }

        if (me.CanAttack())
            yield return me.StartCoroutine(me.AttackRoutine(target));

        me.isActed = true;
    }

    private Character EasyFinder(Enemy me, List<Character> targets)
    {
        Character closest = null;
        float minDis = float.MaxValue;
        Vector3 pos = me.transform.position;

        foreach(Character target in targets)
        {
            Vector3 dir = target.transform.position - pos;
            float dis = dir.sqrMagnitude;

            if(dis < minDis)
            {
                minDis = dis;
                closest = target;
            }
        }

        return closest;
    }
}

public class NormalAI : IEnemyAI
{
    public IEnumerator Attack(Enemy me)
    {
        yield return null;
    }
}

public class HardAI : IEnemyAI
{
    public IEnumerator Attack(Enemy me)
    {
        yield return null;
    }
}
