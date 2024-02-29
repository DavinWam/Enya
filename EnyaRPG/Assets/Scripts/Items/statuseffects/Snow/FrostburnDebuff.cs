using UnityEngine;
using System.Linq;

[CreateAssetMenu(fileName = "FrostburnDebuff", menuName = "StatusEffects/FrostburnDebuff")]
public class FrostburnDebuff : Debuff
{
    public float attack;
    public override string GetDescription()
    {
        return "The target suffers from frostburn, taking periodic damage.";
    }

    public override float GetTotalBoostAmount()
    {
        return attack*(boostAmount/100f);
    }
    public override void actionEffect(CharacterBase active, Act act)
    {
        // This method is called whenever an action occurs in the game.
        // Check if the active character is the target of the debuff.
        if (active.characterStats.activeStatusEffects.Contains(this))
        {
            // Apply tick damage
            active.TakeDamage(GetTotalBoostAmount(),false,false,false);

            // Optionally, add visual or sound effects here
            // Decrease duration or remove the effect if duration ends
            currentDuration--;
            if (currentDuration <= 0)
            {
                RemoveEffect(active.characterStats);
            }
        }
    }
}
