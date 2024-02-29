// PostDamageCondition Implementation
using System.Collections;
using UnityEngine;

[CreateAssetMenu(fileName = "RainmakersFuryCondition", menuName = "SpellConditions/RainmakersFuryCondition")]
public class RainmakersFuryCondition : IPostDamageCondition
{
    public override IEnumerator ApplyPostDamageEffect(CharacterBase caster, Act act)
    {
        BattleController battleController = FindObjectOfType<BattleController>();

        foreach (var player in battleController.playerParty)
        {
            CharacterBase playerCharacter = player.GetComponent<CharacterBase>();
            if (playerCharacter != null && playerCharacter.IsAlive)
            {
                // Restore a certain amount of MP, e.g., 10% of max mana
                if (playerCharacter.characterStats is PlayerStats playerStats)
                {
                    float manaRestore = playerStats.GetEffectiveStat(StatType.MANA) * 0.1f;
                    playerStats.RegenerateMana(manaRestore);
                }
            }
        }

        yield break;
    }
}