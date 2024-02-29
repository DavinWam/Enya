using System.Collections;
using System.Linq;
using UnityEngine;

[CreateAssetMenu(fileName = "HealingConversionBuff", menuName = "StatusEffects/HealingConversionBuff")]
public class HealingConversionBuff : Buff
{
    public override void ApplyEffect(CharacterStats stats)
    {
        // Avoid adding duplicate buffs
        if (stats.activeStatusEffects.OfType<HealingConversionBuff>().Any())
        {
            return;
        }

        var clonedBuff = this.Clone();
        stats.activeStatusEffects.Add(clonedBuff);
        currentDuration = duration;

        // Subscribe to the damage dealt event
        BattleController.OnDamageDealt += ((HealingConversionBuff)clonedBuff).HandleDamageDealt;
    }

    public override void RemoveEffect(CharacterStats stats)
    {
        // Remove the buff and unsubscribe from the event
        stats.activeStatusEffects.Remove(this);
        BattleController.OnDamageDealt -= HandleDamageDealt;
    }

    private void HandleDamageDealt(CharacterBase attacker, Act act, float damage)
    {
        // Check if the target of the action has this buff
        if (act.target.characterStats.activeStatusEffects.Contains(this))
        {
            // Convert damage to healing
            act.target.characterStats.Heal(damage);

            // Optionally, you can zero out the damage if you want to negate it completely
            // SetDamage(0) method can be used here if you have implemented it in your BattleController
            FindObjectOfType<BattleController>().SetDamage(0);

            // Decrease the duration of the buff if it's not infinite
            DecreaseNoDuration(act.target.characterStats);
        }
    }


}
