using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Debuff", menuName = "StatusEffects/GustedDebuff")]
public class GustedDebuff : Debuff
{
    


    public override string GetDescription()
    {
        return "The target is gusted, susceptible to certain wind-based attacks.";
    }
    public override void actionEffect(CharacterBase active, Act act){
        float damage = active.characterStats.GetEffectiveStat(StatType.HEALTH)*(boostAmount/100f);
        //add gusted animation
        active.TakeDamage(damage,false,false,false);
    }
}
