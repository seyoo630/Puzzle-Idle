using UnityEngine;
using DG.Tweening;
using System.Collections;

public abstract class Character : MonoBehaviour
{
    public int maxHP = 100;

    public Transform attackTarget;

    public GameObject projectilePrefab;
    public Transform projectileSpawnPoint;

    public float projectileSpeed = 12f;
    public float projectileDelay = 0.1f;

    public int ultimateBuffTurns = 3;
    public int debuffTurns = 3;

    public GameObject ultimateBuffVFX;

    protected SPUM_Prefabs anim;

    protected bool isUltimateBuffActive = false;
    protected int remainUltTurns = 0;

    protected AttackEffectProcessor selfBuffProcessor;
    protected AttackEffectProcessor debuffProcessor;

    protected virtual void Awake()
    {
        anim = GetComponent<SPUM_Prefabs>();

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

        for (int i = 0; i < count; i++)
        {
            SpawnProjectile();
            yield return new WaitForSeconds(projectileDelay);
        }

        if (count >= 3)
            debuffProcessor.ApplyEffects(attackTarget);

        if (isUltimateBuffActive)
            selfBuffProcessor.ApplyEffects(transform);
    }

    protected virtual void SpawnProjectile()
    {
        Vector3 start = projectileSpawnPoint.position;
        Vector3 end = attackTarget.position;

        GameObject proj = Instantiate(projectilePrefab, start, Quaternion.identity);

        Vector3 dir = (end - start).normalized;
        float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
        proj.transform.rotation = Quaternion.Euler(0, 0, angle);

        float t = Vector3.Distance(start, end) / projectileSpeed;

        proj.transform.DOMove(end, t)
            .SetEase(Ease.Linear)
            .OnComplete(() => Destroy(proj));
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
