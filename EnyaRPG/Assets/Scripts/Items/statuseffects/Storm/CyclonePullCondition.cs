using System.Linq;
using UnityEngine;

[CreateAssetMenu(fileName = "CyclonePullCondition", menuName = "SpellConditions/CyclonePullCondition")]
public class CyclonePullCondition : IPreDamageCondition
{
    [Tooltip("Multiplier to apply when the target is under 'Gusted' status effect.")]
    public float gustedDamageMultiplier = 1.5f;  // For example, 1.5 means 50% increased damage.

    public override float AdjustDamage(CharacterBase caster, Act act, float damage)
    {
        // Check if the target has the "Gusted" status effect.
        bool hasGustedEffect = act.target.characterStats.activeStatusEffects.Any(effect => effect.label == "Gusted");

        // If the target has the effect, multiply the damage.
        if (hasGustedEffect)
        {
            return damage * gustedDamageMultiplier;
        }
        Debug.Log("boosting damage against gusted enemy");
        // If not, return the original damage.
        return damage;
    }
} 
