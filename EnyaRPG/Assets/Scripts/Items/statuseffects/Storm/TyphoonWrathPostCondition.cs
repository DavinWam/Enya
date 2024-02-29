using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "TyphoonWrathPostCondition", menuName = "SpellConditions/TyphoonWrathPostCondition")]
public class TyphoonWrathPostCondition : IPostDamageCondition
{
    public override IEnumerator ApplyPostDamageEffect(CharacterBase caster, Act act)
    {
        BattleController battleController = FindObjectOfType<BattleController>();
        List<GameObject> aliveEnemies = battleController.aliveEnemies;  // Assuming `aliveEnemies` is public or has a public getter.

        bool enemyKilled = false;

        for (int i = aliveEnemies.Count - 1; i >= 0; i--)
        {
            CharacterBase character = aliveEnemies[i].GetComponent<CharacterBase>();
            if (character.characterStats.activeStatusEffects.Exists(se => se is GustedDebuff) && !character.IsAlive)
            {
                enemyKilled = true;
                break;
            }
        }

        if (enemyKilled)
        {
            List<CharacterBase> tempList = new List<CharacterBase>();
            
            // Dequeue all items from the original queue and place them into the temporary list.
            while (battleController.turnQueue.Count > 0)
            {
                tempList.Add(battleController.turnQueue.Dequeue());
            }

            // Add the caster to the front of the list.
            tempList.Insert(0, caster);

            // Requeue all items from the list back to the original queue.
            foreach (var character in tempList)
            {
                battleController.turnQueue.Enqueue(character);
            }

            yield return battleController.StartCoroutine(battleController.battleUI.UpdateTurnWheel(battleController.turnQueue));
        }
    }
}
