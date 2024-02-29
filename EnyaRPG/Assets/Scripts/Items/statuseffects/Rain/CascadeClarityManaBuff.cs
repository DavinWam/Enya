using UnityEngine;
using System.Linq;

[CreateAssetMenu(fileName = "CascadeClarityManaBuff", menuName = "StatusEffects/CascadeClarityManaBuff")]
public class CascadeClarityManaBuff : Buff
{
    public override void ApplyEffect(CharacterStats stats)
    {
        // Check if the target already has an instance of this buff
        CascadeClarityManaBuff existingBuff = stats.activeStatusEffects.OfType<CascadeClarityManaBuff>().FirstOrDefault();

        if (existingBuff == null) // If no existing buff, apply a new one
        {
            if (stats is PlayerStats playerStats)
            {
                // Store the original mana value
                float originalMana = playerStats.GetEffectiveStat(StatType.MANA);

                // Add the new buff instance
                var clonedBuff = this.Clone();
                clonedBuff.currentDuration = duration;
                stats.activeStatusEffects.Add(clonedBuff);

                // Recalculate mana after applying the buff
                float newMana = playerStats.GetEffectiveStat(StatType.MANA);

                // Regenerate the difference in mana
                playerStats.RegenerateMana(newMana - originalMana);
            }
        }
        else // If the buff is already applied, refresh its duration
        {
            existingBuff.currentDuration = duration;
        }
    }


}
