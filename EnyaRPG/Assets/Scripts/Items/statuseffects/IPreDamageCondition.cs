using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class IPreDamageCondition : ScriptableObject
{
    public StatusEffect statusEffect;
    public virtual float AdjustDamage(CharacterBase caster, Act act, float damage){
        return damage;
    }
}