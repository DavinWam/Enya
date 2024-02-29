using System.Linq;
using Unity.VisualScripting;
using UnityEngine;

[CreateAssetMenu(fileName = "VoltageJabBuff", menuName = "StatusEffects/VoltageJabBuff", order = 1)]
public class VoltageJabBuff : Buff
{
    // Maximum stacks this buff can have
    public const int MaxStacks = 3;
    public int stacks = 0;

    public override void ApplyEffect(CharacterStats target)
    {
        if (!target)
            return;

        // Check if the player already has the buff, if so, increase stacks
        VoltageJabBuff existingBuff = target.activeStatusEffects.OfType<VoltageJabBuff>().FirstOrDefault();

        if (existingBuff)
        {
            existingBuff.stacks = Mathf.Min(existingBuff.stacks + 1, MaxStacks);
            existingBuff.currentDuration = duration;  // Reset duration
        }
        else
        {
            stacks = 1; // Initial stack
            currentDuration = duration; // Initial duration
            target.activeStatusEffects.Add(this.Clone());
        }
    }


    public override float GetTotalBoostAmount()
    {
        return boostAmount * (stacks);
    }
    public override string GetDescription()
    {
        string baseDescription = base.GetDescription();
        return $"{baseDescription} Stacks: {stacks}";
    }

    
}
