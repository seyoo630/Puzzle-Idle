using UnityEngine;

public class Archer : Character
{
    protected override void Awake()
    {
        base.Awake();

        selfBuffProcessor.AddEffect(new ArcherEffect_CritBoost());

        debuffProcessor.AddEffect(new ArcherEffect_Debuff());
    }
}