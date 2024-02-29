using System.Linq;
using UnityEngine;

[CreateAssetMenu(fileName = "DownpourDanceBuff", menuName = "StatusEffects/DownpourDanceBuff")]
public class DownpourDanceBuff : Buff
{
    public FireType additionalFireType;
    private BattleController bc;

    public override void ApplyEffect(CharacterStats stats)
    {
        if (stats.activeStatusEffects.OfType<DownpourDanceBuff>().Any())
        {
            return; // Avoid adding duplicate buffs
        }

        var clonedBuff = this.Clone();
        stats.activeStatusEffects.Add(clonedBuff);
        currentDuration = duration;
        ((DownpourDanceBuff)clonedBuff).additionalFireType = SelectRandomWeaknessType(FindObjectOfType<BattleController>());
        BattleController.OnPlayerSpellUsed += ((DownpourDanceBuff)clonedBuff).HandlePlayerSpellUsage;
    }

    public override void RemoveEffect(CharacterStats stats)
    {
        stats.activeStatusEffects.Remove(this);
        BattleController.OnPlayerSpellUsed -= HandlePlayerSpellUsage;
    }

    private FireType SelectRandomWeaknessType(BattleController battleController)
    {
        var uniqueWeaknesses = battleController.aliveEnemies
            .Select(enemy => enemy.GetComponent<CharacterBase>().characterStats.weakness)
            .Distinct()
            .Where(weakness => weakness != FireType.RAIN)
            .ToList();

        if (uniqueWeaknesses.Count > 0)
        {
            int randomIndex = Random.Range(0, uniqueWeaknesses.Count);
            return uniqueWeaknesses[randomIndex];
        }
        else
        {
            return FireType.RAIN; // Default type if no other weaknesses are found
        }
    }

    private void HandlePlayerSpellUsage(Act act, CharacterBase caster)
    {
        Debug.Log("in buff");
        if (caster.characterStats.activeStatusEffects.Any(buff => buff is DownpourDanceBuff))
        {
            bool isRainWeakness = act.target.characterStats.weakness == FireType.RAIN && act.spell.fireType != FireType.RAIN;
            bool isAdditionalWeakness = act.target.characterStats.weakness == additionalFireType && act.spell.fireType != additionalFireType;
              Debug.Log("is weak: "+isAdditionalWeakness);
            if (isRainWeakness || isAdditionalWeakness)
            {
                bc = FindObjectOfType<BattleController>();
                Debug.Log("weak");
                bc.SetWeaknessMultiplier(1.3f);
                act.isWeak = true;
                bc.SetIsWeakOverride(true);
                currentDuration--;
            }

            if (currentDuration <= 0)
            {
                RemoveEffect(caster.characterStats);
            }
        }
    }
}
