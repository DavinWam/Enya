using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "DeathTriggerDefenseReduction", menuName = "SpellConditions/DeathTriggerDefenseReduction")]
public class DeathTriggerDefenseReduction : IPostDamageCondition
{
    public Debuff defenseReductionDebuff;

    public override IEnumerator ApplyPostDamageEffect(CharacterBase caster, Act act)
    {
        // Check if the target of the act is dead
        if (!act.target.IsAlive)
        {
            // Apply the debuff to all alive players
            BattleController battleController = FindObjectOfType<BattleController>();
            foreach (var playerObj in battleController.alivePlayers)
            {
                CharacterBase player = playerObj.GetComponent<CharacterBase>();
                defenseReductionDebuff.ApplyEffect(player.characterStats);
            }
        }

        yield return null;
    }
}
