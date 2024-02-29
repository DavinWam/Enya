using System.Linq;
using UnityEngine;

[CreateAssetMenu(fileName = "CycloneSurgePreDamageCondition", menuName = "SpellConditions/CycloneSurgePreDamageCondition")]
public class CycloneSurgePreDamageCondition : IPreDamageCondition
{
    public override float AdjustDamage(CharacterBase caster, Act act, float damage)
    {
        int buffCount = caster.characterStats.activeStatusEffects.OfType<Buff>().Count();
       BattleController bc = FindObjectOfType<BattleController>();
       bc.AddAttackPower(buffCount*.2f);
        return damage;
    }
}
