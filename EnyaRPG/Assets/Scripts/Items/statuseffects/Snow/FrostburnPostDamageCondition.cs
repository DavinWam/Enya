using UnityEngine;
using System.Collections;

[CreateAssetMenu(fileName = "FrostburnPostDamageCondition", menuName = "SpellConditions/FrostburnPostDamageCondition")]
public class FrostburnPostDamageCondition : IPostDamageCondition
{
 

    public override IEnumerator ApplyPostDamageEffect(CharacterBase caster, Act act)
    {   
        

        // Clone the debuff and set its damage
        FrostburnDebuff debuff = Instantiate(statusEffect) as FrostburnDebuff;
        //set its damage relative to attack
        debuff.attack = caster.characterStats.GetEffectiveStat(StatType.ATTACK);
        
        // Apply the debuff to the target
        debuff.ApplyEffect(act.target.characterStats);

        yield break;
    }
}
