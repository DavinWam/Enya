using UnityEngine;
using System.Collections.Generic;
using System.Collections;

[CreateAssetMenu(fileName = "RandomBuffPostDamage", menuName = "PostDamageConditions/RandomBuffPostDamage")]
public class RandomBuffPostDamage : IPostDamageCondition
{
    [SerializeField] private List<Buff> potentialBuffs;

    public override IEnumerator ApplyPostDamageEffect(CharacterBase activeCharacter, Act act)
    {
        if (potentialBuffs.Count == 0)
            yield break;

        // Randomly pick two unique buffs from the list
        Buff firstBuff = potentialBuffs[Random.Range(0, potentialBuffs.Count)];
        Buff secondBuff;
        do
        {
            secondBuff = potentialBuffs[Random.Range(0, potentialBuffs.Count)];
        } while (secondBuff == firstBuff);  // Ensure the second buff is different

        // Apply the buffs
        firstBuff.ApplyEffect(act.target.characterStats);
        secondBuff.ApplyEffect(act.target.characterStats);

        // Optional: Logic to handle visual/audio feedback
        yield break;
    }
    
}
