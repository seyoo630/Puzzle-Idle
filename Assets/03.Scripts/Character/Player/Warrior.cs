using UnityEngine;

public class Warrior : Character
{
    protected override void Awake()
    {
        base.Awake();

        selfBuffProcessor.AddEffect(new WarriorEffect_ExtraDamage());

        debuffProcessor.AddEffect(new WarriorEffect_Debuff());
    }
}
