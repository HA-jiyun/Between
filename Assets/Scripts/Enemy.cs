using System.Collections;
using UnityEngine;

public class Enemy : Unit
{
    public IEnemyAI myAI;

    protected override void Start()
    {
        isEnemy = true;
        GameManager.instance.allEnemies.Add(this);
        base.Start();
    }

    public IEnumerator AttackRoutine(Character target)
    {
        int damageAmount = basicDamage;
        Look(target.transform);
        yield return StartCoroutine(AttackAnimationRoutine());
        target.TakeDamage(damageAmount);
    }

    protected override void Die()
    {
        GameManager.instance.allEnemies.Remove(this);
        base.Die();
    }
}
