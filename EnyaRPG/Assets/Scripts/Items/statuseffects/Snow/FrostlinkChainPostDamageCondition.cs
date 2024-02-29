using System.Collections;
using System.Linq;
using UnityEngine;

[CreateAssetMenu(fileName = "FrostlinkChainPostDamageCondition", menuName = "SpellConditions/FrostlinkChainPostDamageCondition")]
public class FrostlinkChainPostDamageCondition : IPostDamageCondition
{
    public override IEnumerator ApplyPostDamageEffect(CharacterBase caster, Act act)
    {   
        var primaryTarget = act.target;
        var debuffs = primaryTarget.characterStats.activeStatusEffects.OfType<Debuff>();

        // Check if primary target is chilled
        if (debuffs.Any(d => d.affectedStatType == StatType.SPEED))
        {
            //refresh debuff
            foreach (var debuff in debuffs)
            {
                debuff.currentDuration = debuff.duration;

            }
            CharacterBase secondaryTarget = FindRandomEnemy(act.target);
            if (secondaryTarget != null)
            {
                foreach (var debuff in debuffs)
                {
                    secondaryTarget.characterStats.activeStatusEffects.Add(debuff.Clone());
                }
            }
        }

        yield break;
    }

private CharacterBase FindRandomEnemy(CharacterBase currentTarget)
    {
        var battleController = FindObjectOfType<BattleController>();
        if (battleController == null)
        {
            Debug.LogError("BattleController not found in the scene.");
            return null;
        }

        // Create a new list excluding the current target
        var possibleTargets = battleController.aliveEnemies
            .Where(enemy => enemy.GetComponent<CharacterBase>() != currentTarget)
            .ToList();

        if (possibleTargets.Count == 0)
        {
            // No other targets available
            return null;
        }

        // Select a random enemy from the list
        int randomIndex = UnityEngine.Random.Range(0, possibleTargets.Count);
        return possibleTargets[randomIndex].GetComponent<CharacterBase>();
    }

}
