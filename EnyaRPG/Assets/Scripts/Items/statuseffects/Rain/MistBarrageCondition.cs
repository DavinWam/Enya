using UnityEngine;

[CreateAssetMenu(fileName = "MistBarrageCondition", menuName = "SpellConditions/MistBarrageCondition")]
public class MistBarrageCondition : IPreDamageCondition
{
    public override float AdjustDamage(CharacterBase caster, Act act, float damage)
    {
        BattleController battleController = FindObjectOfType<BattleController>();
        var randomAlly = battleController.alivePlayers[UnityEngine.Random.Range(0, battleController.alivePlayers.Count)];
        CharacterBase allyCharacter = randomAlly.GetComponent<CharacterBase>();
        
        if (allyCharacter != null)
        {
            // Heal for a small amount, e.g., 5% of max health
            float healAmount = allyCharacter.characterStats.GetEffectiveStat(StatType.HEALTH) * 0.05f;
            ((PlayerCharacter)allyCharacter).Heal((int)healAmount, act.isCritical);
        }

        return damage; // Does not modify the original damage
    }
}
