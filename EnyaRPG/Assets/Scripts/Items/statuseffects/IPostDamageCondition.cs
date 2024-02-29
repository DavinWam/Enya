using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class IPostDamageCondition : ScriptableObject
{
    public StatusEffect statusEffect;
    public virtual IEnumerator ApplyPostDamageEffect(CharacterBase caster, Act act)
    {   
        yield break;
    }
}