using System.Collections;
using System.Linq;
using UnityEngine;

[CreateAssetMenu(fileName = "SolarSalvationCondition", menuName = "SpellConditions/SolarSalvationCondition")]
public class SolarSalvationCondition : IPostDamageCondition
{
    public override IEnumerator ApplyPostDamageEffect(CharacterBase caster, Act act)
    {   
        BattleController battleController = FindObjectOfType<BattleController>();

        // Revive and heal allies
        foreach (var player in battleController.playerParty)
        {
            CharacterBase playerCharacter = player.GetComponent<CharacterBase>();
            if (playerCharacter != null && !playerCharacter.IsAlive)
            {
                // Revive the player
                playerCharacter.IsAlive = true;
                battleController.alivePlayers.Add(player); // Add them back to the list of alive players
            }
        }

        // Damage enemies with Sun Mark
        foreach (var enemy in battleController.aliveEnemies)
        {
            CharacterBase enemyCharacter = enemy.GetComponent<CharacterBase>();
            if (enemyCharacter != null)
            {
                // Check if the enemy has the Sun Mark debuff
                SunMarkDebuff sunMarkDebuff = enemyCharacter.characterStats.activeStatusEffects
                    .OfType<SunMarkDebuff>()
                    .FirstOrDefault();

                if (sunMarkDebuff != null)
                {
                    // Damage the enemy for 10% of their max health
                    float damageAmount = enemyCharacter.characterStats.GetEffectiveStat(StatType.HEALTH) * 0.1f;
                    enemyCharacter.TakeDamage(damageAmount,false,false,false);
                }
            }
        }

        yield break;
    }
}
