using System.Collections.Generic;
using UnityEngine;

public class AttackEffectProcessor
{
    private List<IAttackEffect> effects = new List<IAttackEffect>();

    public void AddEffect(IAttackEffect effect)
    {
        effects.Add(effect);
    }

    public void ClearEffects()
    {
        effects.Clear();
    }

    public void ApplyEffects(Transform target)
    {
        foreach (var ef in effects)
            ef.ApplyEffect(target);
    }
}
