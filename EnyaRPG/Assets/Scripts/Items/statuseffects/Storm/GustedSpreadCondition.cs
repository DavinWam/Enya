using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "GustedCondition", menuName = "SpellConditions/GustedCondition")]
public class GustedSpreadCondition : IPreDamageCondition
{

    public override float AdjustDamage(CharacterBase caster, Act act, float damage)
    {
        // Check if target has Gusted debuff
        if (act.target.characterStats.activeStatusEffects.Exists(se => se is GustedDebuff))
        {
            var adjacentCharacters = FindObjectOfType<BattleController>().enemyAdjacencyList[act.target];  // Assuming BattleController is a singleton.

            foreach (var adjacentChar in adjacentCharacters)
            {
                // ApplyEffect or damage logic for adjacent characters.
                statusEffect.ApplyEffect(adjacentChar.characterStats);
            }
            statusEffect.RemoveEffect(act.target.characterStats);
        }else{
            statusEffect.ApplyEffect(act.target.characterStats);
        }
        return 0;
    }
}

