using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Character : Unit
{
    protected override void Start()
    {
        GameManager.instance.allCharacters.Add(this);
        base.Start();
    }

    public void Clicked()
    {
        MapManager.instance.ShowMoveRange(this, moveRange, attackDis, attackDis);
        UIManager.instance.actPopup.Setup(CanAttack());
    }

    public override IEnumerator Move(Vector3 targetPos)
    {
        if(isMoving) yield break;

        InputManager.instance.currentTargetIndex = -1;
        yield return StartCoroutine(base.Move(targetPos));

        MapManager.instance.UpdateAggroLines(targetPos);
    }

    public List<Unit> GetEnemiesInRange(Vector3Int myPos)
    {
        List<Unit> enemies = new List<Unit>();
        List<Vector3Int> list = GetAttackablePositions(myPos, attackDis, attackDis);

        foreach(Vector3Int pos in list)
        {
            if (MapManager.instance.unitList.ContainsKey(pos))
            {
                Unit target = MapManager.instance.unitList[pos];
                if (target != null && target.isEnemy != isEnemy)
                    enemies.Add(target);
            }
        }
        return enemies;
    }

    private void Attack(int typeDamage)
    {
        StartCoroutine(AttackRoutine(typeDamage));
    }
    private IEnumerator AttackRoutine(int typeDamage)
    {
        Enemy target = InputManager.instance.selectedEnemy;
        if (target != null)
        {
            int damageAmount = typeDamage;
            yield return StartCoroutine(AttackAnimationRoutine());
            target.TakeDamage(damageAmount);
            EndAct();
        }
    }
    public void BasicAttack() => Attack(basicDamage);
    public void SpecialAttack() => Attack(specialDamage);

    public void EndAct()
    {
        MapManager.instance.HideMoveRange();

        if(GameManager.instance.allEnemies.Count == 0)
        {
            StartCoroutine(TurnManager.instance.EndGameRoutine());
            return;
        }

        isActed = true;
        MapManager.instance.targetEnemies.Clear();
        InputManager.instance.ClearEverything();
    }

    protected override void Die()
    {
        GameManager.instance.allCharacters.Remove(this);
        base.Die();
    }
}
