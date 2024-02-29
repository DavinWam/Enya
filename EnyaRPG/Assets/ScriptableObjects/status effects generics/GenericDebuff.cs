using System.Linq;
using UnityEngine;

[CreateAssetMenu(fileName = "GenericDebuff", menuName = "StatusEffects/Debuff")]
public class GenericDebuff : Debuff
{
    public override void ApplyEffect(CharacterStats target)
    {
        var existingDebuff = target.activeStatusEffects.OfType<GenericDebuff>()
            .FirstOrDefault(debuff => debuff.affectedStatType == this.affectedStatType && debuff.GetType() == this.GetType());

        if (existingDebuff != null)
        {
            existingDebuff.currentDuration = duration; // Refresh the duration of existing buff
        }
        else
        {
            var clonedDebuff = Clone() as GenericDebuff;
            clonedDebuff.currentDuration = duration;
            target.activeStatusEffects.Add(clonedDebuff);
        }
    }


    public override void RemoveEffect(CharacterStats target)
    {
        target.activeStatusEffects.Remove(this);
        // Unsubscribe from any events if necessary
    }

    // ... other methods ...
}
