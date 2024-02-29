using UnityEngine;
using System.Collections.Generic;
using System.Collections;
using System.Linq;

[CreateAssetMenu(fileName = "ZephyrLinkPostDamageCondition", menuName = "SpellConditions/ZephyrLinkPostDamageCondition")]
public class ZephyrLinkPostDamageCondition : IPostDamageCondition
{
    private BattleController battleController;

    public override IEnumerator ApplyPostDamageEffect(CharacterBase caster, Act act)
    {
        battleController = FindObjectOfType<BattleController>();
        var casterBuffs = caster.characterStats.activeStatusEffects.OfType<Buff>().ToList();
        //refresh buffs
        foreach (var buff in casterBuffs)
        {
            buff.currentDuration = buff.duration;
        }
        foreach (var ally in battleController.playerParty)
        {

            var allyStats = ally.GetComponent<CharacterBase>().characterStats;

            if(ally != caster){
                //refresh  ally buffs
                foreach (StatusEffect allyBuff in allyStats.activeStatusEffects)
                {
                    allyBuff.currentDuration = allyBuff.duration;
                }
                foreach (StatusEffect buff in casterBuffs)
                {

                    allyStats.activeStatusEffects.Add(buff.Clone());
                }
            }

        }
        yield break;
    }
}
