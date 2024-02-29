using System.Linq;
using UnityEngine;

[CreateAssetMenu(fileName = "PowerChargeBuff", menuName = "StatusEffects/PowerChargeBuff")]
public class PowerChargeBuff : Buff
{
    public override void ApplyEffect(CharacterStats stats)
    {
        if (stats.activeStatusEffects.OfType<PowerChargeBuff>().Any())
        {
            return; // Avoid adding duplicate buffs
        }

        var clonedBuff = this.Clone();
        stats.activeStatusEffects.Add(clonedBuff);
        currentDuration = duration;

        BattleController.OnDamageDealt += ((PowerChargeBuff)clonedBuff).HandleDamageDealt;
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
            // Double the damage
            FindObjectOfType<BattleController>().SetDamage(damage * 2);
            DecreaseNoDuration(attacker.characterStats);
        }
    }
    
    // ... other methods ...
}
