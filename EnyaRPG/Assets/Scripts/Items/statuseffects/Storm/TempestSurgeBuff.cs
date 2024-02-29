using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[CreateAssetMenu(fileName = "TempestSurgeBuff", menuName = "StatusEffects/TempestSurgeBuff")]
public class TempestSurgeBuff : Buff
{
    public override void ApplyEffect(CharacterStats stats)
    {
        BattleController bc = FindObjectOfType<BattleController>();
        TempestSurgeBuff existingBuff = bc.activeCharacter.characterStats.activeStatusEffects.OfType<TempestSurgeBuff>().FirstOrDefault();
        if(existingBuff != null){
            existingBuff.currentDuration = duration;
        }else
        {
            currentDuration = duration;
            stats.activeStatusEffects.Add(this.Clone());
            Debug.Log("tempest surge");
        }
        
        BattleController.OnDamageDealt += HandleDamageDealt; // Subscribe to the event
    }

    // This will be unsubscribed when the effect is removed.
    public override void RemoveEffect(CharacterStats target)
    {
        if (!target)
            return;

        target.activeStatusEffects.Remove(this);
        BattleController.OnDamageDealt -= HandleDamageDealt; // Unsubscribe from the event
    }

    private void HandleDamageDealt(CharacterBase attacker,Act act, float damage)
    {
        // Assuming there's a way to access the character's mana or MP
        float manaRestoreAmount = damage/5f; // Define how you get the boost amount and apply it
        
        if (attacker.characterStats.activeStatusEffects.OfType<TempestSurgeBuff>().Any())
        {
            if (attacker.characterStats is PlayerStats playerStats)
            {
                playerStats.RegenerateMana(manaRestoreAmount);
            }
        }else{
            return;
        }
        
    }

}
