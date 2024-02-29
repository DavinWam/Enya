using System.Linq;
using UnityEngine;

[CreateAssetMenu(fileName = "MistMantlePreDamageCondition", menuName = "SpellConditions/MistMantlePreDamageCondition")]
public class MistMantlePreDamageCondition : IPreDamageCondition
{
    public override float AdjustDamage(CharacterBase caster, Act act, float damage)
    {
        int buffCount = caster.characterStats.activeStatusEffects.OfType<Buff>().Count();
       BattleController bc = FindObjectOfType<BattleController>();
       bc.AddAttackPower(buffCount*.4f);
        return damage;
    }
}
