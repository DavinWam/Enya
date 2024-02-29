using System.Linq;
using UnityEngine;

[CreateAssetMenu(fileName = "StratosphericInsightBuff", menuName = "StatusEffects/StratosphericInsightBuff")]
public class StratosphericInsightBuff : Buff
{
    public override void ApplyEffect(CharacterStats stats)
    {
        if (stats.activeStatusEffects.OfType<StratosphericInsightBuff>().Any())
        {
            return; // Avoid adding duplicate buffs
        }

        var clonedBuff = this.Clone();
        stats.activeStatusEffects.Add(clonedBuff);
        currentDuration = duration;

        BattleController.OnDamageDealt += ((StratosphericInsightBuff)clonedBuff).HandleDamageDealt;
    }

    public override void RemoveEffect(CharacterStats stats)
    {
        stats.activeStatusEffects.Remove(this);
        BattleController.OnDamageDealt -= HandleDamageDealt;
    }

    private void HandleDamageDealt(CharacterBase attacker, Act act, float damage)
    {
        if (attacker.characterStats.activeStatusEffects.Contains(this))
        {
            // Increase the mana regeneration rate
            FindObjectOfType<BattleController>().SetAttackManaRegen(boostAmount);
        }
    }
    
    // ... other methods ...
}
