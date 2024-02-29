// PostDamageCondition Implementation
using System.Collections;
using UnityEngine;

[CreateAssetMenu(fileName = "CascadeClarityCondition", menuName = "SpellConditions/CascadeClarityCondition")]
public class CascadeClarityCondition : IPostDamageCondition
{

    public override IEnumerator ApplyPostDamageEffect(CharacterBase caster, Act act)
    {
        if (act.target.characterStats is PlayerStats playerStats)
        {
            float originalMana = playerStats.GetEffectiveStat(StatType.MANA);
            statusEffect.ApplyEffect(act.target.characterStats);
            float newMana = playerStats.GetEffectiveStat(StatType.MANA);
            playerStats.RegenerateMana(newMana - originalMana);
        }

        yield break;
    }
}
