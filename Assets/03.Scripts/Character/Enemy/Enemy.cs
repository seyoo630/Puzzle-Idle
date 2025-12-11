using UnityEngine;
using System.Collections;

public class Enemy : Character
{
    [Header("Enemy Specific")]
    public float actionDelay = 1.0f; // 적 턴일 때 행동 딜레이

    protected override void Awake()
    {
        base.Awake();
        // 적군만의 초기화 로직
        // selfBuffProcessor.AddEffect(new EnemyRageEffect());
    }

    public void EnemyTurnAction()
    {
        if (currentHP <= 0) return;

        //StartCoroutine(EnemyAttackRoutine());
    }

    //IEnumerator EnemyAttackRoutine()
    //{
    //    // 턴 시작 딜레이
    //    yield return new WaitForSeconds(actionDelay);

    //    // 공격 대상 찾기 (예: 랜덤한 아군 영웅, 혹은 어그로 높은 영웅)
    //    // 현재는 Inspector에서 할당된 attackTarget을 사용한다고 가정
    //    if (attackTarget != null)
    //    {
    //        // 적군은 퍼즐을 맞추지 않으므로 고정 횟수(예: 1발) 혹은 랜덤 패턴으로 공격
    //        int attackCount = Random.Range(1, 4);

    //        yield return StartCoroutine(PerformAttack());
    //    }
    //    else
    //    {
    //        Debug.LogWarning("Enemy has no target!");
    //    }

    //    // 턴 종료 알림 (GameManager에게 턴 넘김)
    //    // GameManager.Instance.EndEnemyTurn(); 
    //}

    // 적군이 죽었을 때 추가 보상 드랍 등
    protected override void Die()
    {
        base.Die();
        // 보상 드랍 로직
        // LootManager.Instance.DropGold(100);

        // 애니메이션 재생 후 일정 시간 뒤 오브젝트 파괴
        Destroy(gameObject, 2.0f);
    }
}