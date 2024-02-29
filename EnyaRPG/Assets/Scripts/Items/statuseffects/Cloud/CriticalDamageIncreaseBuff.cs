using UnityEngine;
using System.Linq;

[CreateAssetMenu(fileName = "CriticalDamageIncreaseBuff", menuName = "StatusEffects/CriticalDamageIncreaseBuff")]
public class CriticalDamageIncreaseBuff : Buff
{
    public override void ApplyEffect(CharacterStats target)
    {
        // Check for existing instances of this buff
        if (target.activeStatusEffects.OfType<CriticalDamageIncreaseBuff>().Any())
        {
            return; // Avoid adding duplicate buffs
        }

        var clonedBuff = this.Clone();
        clonedBuff.currentDuration = duration;
        target.activeStatusEffects.Add(clonedBuff);

        // Subscribe to the damage dealt event
        BattleController.OnDamageDealt += ((CriticalDamageIncreaseBuff)clonedBuff).IncreaseCriticalDamage;
    }

    public override void RemoveEffect(CharacterStats target)
    {
        // Remove the buff and unsubscribe from the event
        target.activeStatusEffects.Remove(this);
        BattleController.OnDamageDealt -= IncreaseCriticalDamage;
    }

    private void IncreaseCriticalDamage(CharacterBase attacker, Act act, float damage)
    {
        if (attacker.characterStats is PlayerStats playerStats && act.isCritical)
        {
            float critsScalar = (1.25f + boostAmount) / 1.25f;
            // Modify the damage based on the increased critical damage
            FindObjectOfType<BattleController>().SetDamage(damage * critsScalar);
        }
    }
}
