using System.Collections;
using UnityEngine;

[CreateAssetMenu(fileName = "BreezyBlessingPostDamageCondition", menuName = "SpellConditions/BreezyBlessingPostDamageCondition")]
public class BreezyBlessingPostDamageCondition : IPostDamageCondition
{
    public GameData gameData;

    public override IEnumerator ApplyPostDamageEffect(CharacterBase caster, Act act)
    {
        StatusEffect randomBuff = gameData.GetRandomBuff();
        Debug.Log(caster.characterName);
        randomBuff.ApplyEffect(caster.characterStats);
        //caster.characterStats.activeStatusEffects.Add(randomBuff.Clone());
        yield break;
    }
}
