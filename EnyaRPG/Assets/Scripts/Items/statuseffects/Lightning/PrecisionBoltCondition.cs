using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[CreateAssetMenu(fileName = "PrecisionBoltCondition", menuName = "SpellConditions/PrecisionBoltCondition", order = 2)]
public class PrecisionBoltCondition : IPreDamageCondition
{
    public override float AdjustDamage(CharacterBase caster, Act act, float damage)
    {
        StatusEffect staticCharge = act.target.characterStats.activeStatusEffects.FirstOrDefault(effect => effect.label == "Static Charged");
        
        if (staticCharge != null)
        {
            act.isCritical = true;
            staticCharge.RemoveEffect(act.target.characterStats);  // Remove the Static Charged state.
        }
        return 0;
    }
}

