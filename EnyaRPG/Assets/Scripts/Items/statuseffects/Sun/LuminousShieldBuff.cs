using System.Linq;
using UnityEngine;

[CreateAssetMenu(fileName = "LuminousShieldBuff", menuName = "StatusEffects/LuminousShieldBuff")]
public class LuminousShieldBuff : Buff
{

    public override void ApplyEffect(CharacterStats stats)
    {
        if (stats.activeStatusEffects.OfType<LuminousShieldBuff>().Any())
        {
            return; // Avoid adding duplicate buffs
        }

        var clonedBuff = this.Clone();
        stats.activeStatusEffects.Add(clonedBuff);
        currentDuration = duration;

        BattleController.OnDamageDealt += ((LuminousShieldBuff)clonedBuff).HandleDamageDealt;
    }

    public override void RemoveEffect(CharacterStats stats)
    {
        stats.activeStatusEffects.Remove(this);
        BattleController.OnDamageDealt -= HandleDamageDealt;
    }

    private void HandleDamageDealt(CharacterBase attacker, Act act, float damage)
    {
        if (act.target.characterStats.activeStatusEffects.Contains(this))
        {
            float reflectedDamage = damage * GetTotalBoostAmount()/100f;
            attacker.TakeDamage(reflectedDamage,false,false,false);
            //set damage to 0
            FindObjectOfType<BattleController>().SetDamage(0);
            DecreaseNoDuration(act.target.characterStats);
        }
    }

}
