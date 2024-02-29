using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[CreateAssetMenu(fileName = "GustingSpeedBuff", menuName = "StatusEffects/GustingSpeedBuff")]
public class GustingSpeedBuff : Buff
{
    public int numHit;
   public override void ApplyEffect(CharacterStats target)
    {
        if (!target)
            return;

        GustingSpeedBuff existingBuff = target.activeStatusEffects.OfType<GustingSpeedBuff>().FirstOrDefault();

        if (existingBuff)//refresh if already has buff
        {
            existingBuff.numHit = numHit;
            existingBuff.currentDuration = duration;  // Reset duration
        }
        else//otherwise apply buff
        {
            currentDuration = duration; // Initial duration
            target.activeStatusEffects.Add(this.Clone());
        }
    }


    public override float GetTotalBoostAmount()
    {
        return boostAmount * numHit;
    }
    public override string GetDescription()
    {
        string baseDescription = base.GetDescription();
        return $"{baseDescription} Stacks: {numHit} * {boostAmount}%";
    }
}
