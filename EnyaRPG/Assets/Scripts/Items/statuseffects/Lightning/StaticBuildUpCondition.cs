

using UnityEngine;

[CreateAssetMenu(fileName = "StaticBuildUp", menuName = "SpellConditions/StaticBuildUpCondition", order = 1)]

public class StaticBuildupCondition : IPreDamageCondition
{
    public override float AdjustDamage(CharacterBase caster, Act act, float damage)
    {
        //when crit applies static charge stored in spell

        if(act.isCritical){
            act.spell.applyTarget.ApplyEffect(act.target.characterStats);
        }
        return damage;
    }
}