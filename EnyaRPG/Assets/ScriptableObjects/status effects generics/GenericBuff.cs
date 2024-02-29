using System.Linq;
using UnityEngine;

[CreateAssetMenu(fileName = "GenericBuff", menuName = "StatusEffects/Buff")]
public class GenericBuff : Buff
{


    public override void ApplyEffect(CharacterStats target)
    {
        var existingBuff = target.activeStatusEffects.OfType<GenericBuff>()
            .FirstOrDefault(buff => buff.affectedStatType == this.affectedStatType && buff.GetType() == this.GetType());

        if (existingBuff != null)
        {
            existingBuff.currentDuration = duration; // Refresh the duration of existing buff
        }
        else
        {
            var clonedBuff = Clone() as GenericBuff;
            clonedBuff.currentDuration = duration;
            target.activeStatusEffects.Add(clonedBuff);
        }
    }

    public override void RemoveEffect(CharacterStats target)
    {
        target.activeStatusEffects.RemoveAll(buff => buff is GenericBuff genericBuff && genericBuff.affectedStatType == this.affectedStatType && buff.GetType() == this.GetType());
    }

    // ... other methods ...
}
