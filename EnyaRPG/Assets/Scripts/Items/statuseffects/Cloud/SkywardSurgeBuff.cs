using System.Linq;
using UnityEngine;

[CreateAssetMenu(fileName = "SkywardSurgeBuff", menuName = "StatusEffects/SkywardSurgeBuff")]
public class SkywardSurgeBuff : Buff
{
    public override void ApplyEffect(CharacterStats target)
    {
        if (target.activeStatusEffects.OfType<GenericBuff>().Any(buff => buff.affectedStatType == this.affectedStatType))
        {
            return; // Prevent adding duplicate buffs of the same type
        }

        var clonedBuff = this.Clone();
        clonedBuff.currentDuration = duration;
        target.activeStatusEffects.Add(clonedBuff);

        // Subscribe to any events if necessary
    }

    public override void RemoveEffect(CharacterStats target)
    {
        target.activeStatusEffects.Remove(this);
        // Unsubscribe from any events if necessary
    }


}
