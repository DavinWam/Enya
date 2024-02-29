using UnityEngine;
using System.Collections.Generic;
using System.Collections;

[CreateAssetMenu(fileName = "TempestsGiftPostDamageCondition", menuName = "SpellConditions/TempestsGiftPostDamageCondition")]
public class TempestsGiftPostDamageCondition : IPostDamageCondition
{
    public GameData gameData;

    public override IEnumerator ApplyPostDamageEffect(CharacterBase caster, Act act)
    {
        List<GameObject> playerParty = FindObjectOfType<BattleController>().playerParty;
        StatusEffect randomBuff = null;
        CharacterStats memberStats = null; 
        for (int i = 0; i < 3; i++) // Grant 3 random buffs
        {
            Debug.Log(i);
            randomBuff = gameData.GetRandomBuff();
            foreach (GameObject member in playerParty)
            {
                Debug.Log(randomBuff.label);
                memberStats = member.GetComponent<CharacterBase>().characterStats;
                randomBuff.ApplyEffect(memberStats);

            }
            
        }

        yield break;
    }
}
