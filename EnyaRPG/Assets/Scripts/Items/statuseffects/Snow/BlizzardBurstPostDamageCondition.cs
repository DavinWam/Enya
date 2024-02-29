using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "BlizzardBurstPostDamageCondition", menuName = "SpellConditions/BlizzardBurstPostDamageCondition")]
public class BlizzardBurstPostDamageCondition : IPostDamageCondition
{
    public GameData gameData; // Reference to GameData
    public BlizzardBurstDebuff blizzardBurstDebuff; // Reference to the specific slow debuff

    public override IEnumerator ApplyPostDamageEffect(CharacterBase caster, Act act)
    {   
        BattleController battleController = FindObjectOfType<BattleController>();
        float casterAttackStat = caster.characterStats.GetEffectiveStat(StatType.ATTACK);

        foreach (var enemyGameObject in battleController.aliveEnemies)
        {
            CharacterStats enemyStats = enemyGameObject.GetComponent<CharacterBase>().characterStats;

            // Apply the specific BlizzardBurstDebuff (Slow Debuff)
            enemyStats.activeStatusEffects.Add(Instantiate((BlizzardBurstDebuff)statusEffect));

            // Apply three random debuffs, including setting FrostburnDebuff attack value
            ApplyRandomDebuffs(enemyStats, 3, casterAttackStat);
        }

        yield break;
    }


    private void ApplyRandomDebuffs(CharacterStats target, int count, float casterAttackStat)
    {
        for (int i = 0; i < count; i++)
        {
            int randomIndex = Random.Range(0, gameData.debuffsList.Count);
            Debuff randomDebuff = gameData.debuffsList[randomIndex];

            // Clone the randomly selected debuff
            Debuff clonedDebuff = Instantiate(randomDebuff);

            // If it's a FrostburnDebuff, set the attack value
            if (clonedDebuff is FrostburnDebuff frostburnDebuff)
            {
                frostburnDebuff.attack = casterAttackStat;
            }

            // Apply the debuff
            clonedDebuff.currentDuration = clonedDebuff.duration;
            target.activeStatusEffects.Add(clonedDebuff);
        }
    }

}
