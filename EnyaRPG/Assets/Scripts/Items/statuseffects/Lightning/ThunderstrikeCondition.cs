using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[CreateAssetMenu(fileName = "ThunderstrikeCondition", menuName = "SpellConditions/ThunderstrikeCondition", order = 3)]
public class ThunderstrikeCondition : IPreDamageCondition
{
    public override float AdjustDamage(CharacterBase caster, Act act, float damage)
    {
        float hpPercentage = act.target.characterStats.currentHealth / act.target.characterStats.baseHealth * 100;
        StatusEffect staticCharge = act.target.characterStats.activeStatusEffects.FirstOrDefault(effect => effect.label == "Static Charged");
        
        if (hpPercentage < 20 || staticCharge != null)
        {
            act.isCritical = true;
        }
        return 0;
    }
}
