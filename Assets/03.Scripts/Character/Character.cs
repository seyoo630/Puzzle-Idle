using UnityEngine;
using DG.Tweening;
using System.Collections;

public abstract class Character : MonoBehaviour
{
    [Header("Status")]
    public int maxHP = 100;
    public int currentHP;

    [Header("Combat Settings")]
    public Transform attackTarget; // 공격 대상
    public float attackAnimDelay = 0.3f;

    public GameObject projectilePrefab;
    public Transform projectileSpawnPoint;

    public float projectileSpeed = 12f;
    public float projectileDelay = 0.1f;

    public int ultimateBuffTurns = 3;
    public int debuffTurns = 3;

    public GameObject ultimateBuffVFX;
    public GameObject hitEffectPrefab;

    protected SPUM_Prefabs anim;
    protected Animator _animator;

    protected bool isUltimateBuffActive = false;
    protected int remainUltTurns = 0;

    protected AttackEffectProcessor selfBuffProcessor;
    protected AttackEffectProcessor debuffProcessor;

    protected virtual void Awake()
    {
        anim = GetComponent<SPUM_Prefabs>();
        if (anim != null) _animator = anim._anim;

        currentHP = maxHP;

        selfBuffProcessor = new AttackEffectProcessor();
        debuffProcessor = new AttackEffectProcessor();

        if (ultimateBuffVFX != null)
            ultimateBuffVFX.SetActive(false);
    }

    public virtual IEnumerator PerformAttack(int count, BlockType type)
    {
        BubbleBlockSpawner.Instance.SpawnMinusText(type, count);

        if (attackTarget == null || projectilePrefab == null || projectileSpawnPoint == null)
            yield break;

        if (count >= 6)
            ActivateUltimateBuff();

        PlayAnimation("2_Attack");

        yield return new WaitForSeconds(attackAnimDelay);

        for (int i = 0; i < count; i++)
        {
            SpawnProjectile(count);
            yield return new WaitForSeconds(projectileDelay);
        }

        if (count >= 3)
            debuffProcessor.ApplyEffects(attackTarget);

        if (isUltimateBuffActive)
            selfBuffProcessor.ApplyEffects(transform);
    }

    protected virtual void SpawnProjectile(int comboCount)
    {
        if (attackTarget == null) return;

        Vector3 start = projectileSpawnPoint.position;
        Vector3 end = attackTarget.position;

        GameObject proj = Instantiate(projectilePrefab, start, Quaternion.identity);

        Vector3 dir = (end - start).normalized;
        float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
        proj.transform.rotation = Quaternion.Euler(0, 0, angle);

        float t = Vector3.Distance(start, end) / projectileSpeed;

        proj.transform.DOMove(end, t)
            .SetEase(Ease.Linear)
            .OnComplete(() => {

                // --- [여기서부터 타격감 코드 추가] ---

                // 1. 타격감 계수 (콤보가 높을수록 화면이 더 많이 흔들림)
                float impactPower = 1.0f + (comboCount * 0.2f);

                // 2. 히트 스탑 (시간 정지)
                // 콤보가 높으면 약간 더 길게 멈춤
                GameFeelManager.Instance.DoHitStop(1.0f + (comboCount * 0.1f));

                // 3. 카메라 쉐이크
                GameFeelManager.Instance.ShakeCamera(impactPower);

                // 4. 타겟 반응 (넉백 & 플래시)
                GameFeelManager.Instance.ApplyImpactToTarget(attackTarget, dir);

                // 5. 파티클(이펙트) 생성 (프리팹이 있다면)
                Instantiate(hitEffectPrefab, attackTarget.position, Quaternion.identity);

                // --- [타격감 코드 끝] ---

                Character targetChar = attackTarget.GetComponent<Character>();
                if (targetChar != null)
                {
                    // 데미지 공식은 기획에 따라 변경 (여기선 예시로 콤보 * 10)
                    int damageAmount = comboCount * 10;
                    targetChar.TakeDamage(damageAmount);

                }
                Destroy(proj);
            });
    }

    public void TakeDamage(int damage)
    {
        if (currentHP <= 0) return; 

        currentHP -= damage;
        Debug.Log($"{name} took {damage} damage! HP: {currentHP}");

        PlayAnimation("3_Damaged"); 

        if (currentHP <= 0)
        {
            Die();
        }
    }

    protected virtual void Die()
    {
        currentHP = 0;
        PlayAnimation("4_Death"); 
      
        Debug.Log($"{name} has died.");
    }

    protected void PlayAnimation(string triggerName)
    {
        if (_animator != null)
        {            
            _animator.SetTrigger(triggerName);
        }
    }
    protected void ActivateUltimateBuff()
    {
        isUltimateBuffActive = true;
        remainUltTurns = ultimateBuffTurns;

        if (ultimateBuffVFX != null)
            ultimateBuffVFX.SetActive(true);
    }

    public void ReduceBuffTurn()
    {
        if (!isUltimateBuffActive) return;

        remainUltTurns--;
        if (remainUltTurns <= 0)
        {
            isUltimateBuffActive = false;

            if (ultimateBuffVFX != null)
                ultimateBuffVFX.SetActive(false);
        }
    }
}
