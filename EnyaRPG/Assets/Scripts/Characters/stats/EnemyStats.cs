using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[CreateAssetMenu(fileName = "EnemyStatsData", menuName = "Stats/EnemyStats")]
public class EnemyStats : CharacterStats
{

    
    public List<EnemySpell> spells; // List of spells with chances and wind-up lengths
    [Header("Experience Points")]
    public int expGiven; // Experience points provided upon defeat

    public int GetExpGiven()
    {
        return expGiven;
    }
    public override float GetEffectiveStat(StatType type)
    {
        float baseValue = base.GetEffectiveStat(type);

        float additiveAmount = 0f;
        float multiplicativeAmount = 1f; // Start with 1 for multiplicative boosts

        // Applying Buffs
        foreach (Buff buff in activeStatusEffects.OfType<Buff>().Where(b => b.affectedStatType == type))
        {
            float boost = buff.GetTotalBoostAmount();
            if (buff.isPercentageBoost)
            {
                multiplicativeAmount += boost / 100f; // Convert percentage to multiplier
            }
            else
            {
                additiveAmount += boost;
            }
        }

        // Applying Debuffs
        foreach (Debuff debuff in activeStatusEffects.OfType<Debuff>().Where(d => d.affectedStatType == type))
        {
            float decrease = debuff.GetTotalBoostAmount(); // Assuming GetTotalBoostAmount returns negative values for debuffs
            if (debuff.isPercentageBoost)
            {
                multiplicativeAmount += decrease / 100f; // Convert percentage to multiplier
            }
            else
            {
                additiveAmount += decrease;
            }
        }

        baseValue = (baseValue + additiveAmount) * multiplicativeAmount;
        return baseValue;
    }
}
