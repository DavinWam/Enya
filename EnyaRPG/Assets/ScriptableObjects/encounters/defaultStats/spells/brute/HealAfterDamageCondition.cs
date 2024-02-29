using System.Collections;
using UnityEngine;

[CreateAssetMenu(fileName = "HealAfterDamageCondition", menuName = "SpellConditions/HealAfterDamageCondition", order = 2)]
public class HealAfterDamageCondition :  IPostDamageCondition
{
    public float healPercentage; // The percentage of damage dealt that will be converted to healing

    // Constructor to initialize the healing percentage
    public HealAfterDamageCondition(float healPercentage)
    {
        this.healPercentage = healPercentage;
    }

    // Implement the ApplyPostDamageEffect method from the IPostDamageCondition interface
    public override IEnumerator ApplyPostDamageEffect(CharacterBase user, Act act)
    {
        if (user == null || act == null)
        {
            yield break; // Exit if user or act is null
        }

        // Calculate the amount of healing based on the damage dealt and the healPercentage
        float healAmount = FindObjectOfType<BattleController>().GetDamage() * healPercentage / 100f;

        // Apply the healing to the user
        user.Heal(healAmount, false);

        // Add any additional logic or animations for the healing effect here

        yield return null; // Wait for the next frame (or longer if needed for animations)
    }
}
