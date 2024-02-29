using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;


[CreateAssetMenu(fileName = "GustingBlowCondition", menuName = "SpellConditions/GustingBlowCondition")]
public class GustingBlowSpeedCondition: IPreDamageCondition
{
    private GustingSpeedBuff gsBuff;
    // Start is called before the first frame update
    public override float AdjustDamage(CharacterBase caster, Act act, float damage)
    {
        Debug.Log("in gust speed condition");
        BattleController battleController = FindObjectOfType<BattleController>();
        int numGusted = 0;
        foreach(var enemy in battleController.aliveEnemies){
            StatusEffect gusted = null;
            gusted = enemy.GetComponent<CharacterBase>().characterStats.activeStatusEffects.FirstOrDefault(effect => effect.label == "Gusted");
            if(gusted){
                numGusted++;
            }
        }
        Debug.Log($"num hit:{numGusted}");
        gsBuff  = (GustingSpeedBuff ) statusEffect; 
        gsBuff.numHit = numGusted;
        Debug.Log($"num in buff:{gsBuff.numHit}");
        if(gsBuff.numHit > 0){
            gsBuff.ApplyEffect(caster.characterStats);    
        }
        return 0;   
    }
    
}
