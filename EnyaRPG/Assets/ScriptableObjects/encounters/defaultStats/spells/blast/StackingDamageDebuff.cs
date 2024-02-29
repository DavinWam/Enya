using UnityEngine;
using System.Linq;

[CreateAssetMenu(fileName = "StackingDamageDebuff", menuName = "StatusEffects/StackingDamageDebuff")]
public class StackingDamageDebuff : Debuff
{

    public const int MaxStacks = 5;
    private int stacks = 0;

    public override string GetDescription()
    {
        return $"The target suffers, taking periodic damage. Stacks: {stacks}";
    }

    public override void ApplyEffect(CharacterStats target)
    {
        if (!target)
            return;

        StackingDamageDebuff existingDebuff = target.activeStatusEffects.OfType<StackingDamageDebuff>().FirstOrDefault();

        if (existingDebuff)
        {
            existingDebuff.stacks = Mathf.Min(existingDebuff.stacks + 1, MaxStacks);
            existingDebuff.currentDuration = duration;  // Reset duration
        }
        else
        {
            stacks = 1; // Initial stack
            currentDuration = duration; // Initial duration
            target.activeStatusEffects.Add(this.Clone());
        }
    }

    public override void actionEffect(CharacterBase active, Act act)
    {
        if (active.characterStats.activeStatusEffects.Contains(this))
        {
            // Apply damage for each stack
            float totalDamage = boostAmount * stacks;
            active.TakeDamage(totalDamage, false, false, false);

            // Decrease duration or remove the effect if duration ends
            currentDuration--;
            if (currentDuration <= 0)
            {
                RemoveEffect(active.characterStats);
            }
        }
    }


}
