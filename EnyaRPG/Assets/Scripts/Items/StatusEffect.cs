using System.Linq;
using UnityEngine;

public abstract class StatusEffect : ScriptableObject
{
    public string label;
    public string description;
    public StatType affectedStatType;
    public bool isPercentageBoost = false; // If true, the boost amount is treated as a percentage
    public bool refresh = true;
    public bool noDuration;
    public int duration;
    public int currentDuration;
    private CharacterBase source;
    private CharacterBase target;
    
    public float boostAmount;
    public virtual float GetTotalBoostAmount()
    {
        return boostAmount;
    }
    // Abstract method to apply the status effect
    public abstract void ApplyEffect(CharacterStats target);

    // Abstract method to remove the status effect
    public abstract void RemoveEffect(CharacterStats target);

    public virtual void DecreaseDuration(CharacterStats target)
    {
        if(noDuration){
            return;
        }
        currentDuration--;
        if (currentDuration <= 0)
        {
            RemoveEffect(target);
        }
    }
    //for removing spells that are tagged noDuration
    public virtual void DecreaseNoDuration(CharacterStats target)
    {

        currentDuration--;
        if (currentDuration <= 0)
        {
            RemoveEffect(target);
        }
    }
    public virtual void actionEffect(CharacterBase active,Act act){
        //override this to make a spell do something after an action
    }
    public virtual string GetDescription()
    {
        return description;
    }
    public StatusEffect Clone()
    {
        return Instantiate(this);
    }

}
public class Buff : StatusEffect
{
    public override void ApplyEffect(CharacterStats target)
    {
        if(!target) return;
        
        var existingBuff = target.activeStatusEffects.OfType<Buff>().FirstOrDefault(buff => buff.GetType() == this.GetType());
        if (existingBuff != null)
        {
            if(refresh) existingBuff.currentDuration = duration;  // Reset duration
            
        }
        else
        {
            currentDuration = duration;
            target.activeStatusEffects.Add(Clone());
        }
    }

    public override void RemoveEffect(CharacterStats target)
    {
        if(!target) return;

        target.activeStatusEffects.Remove(this);
    }
}

public class Debuff : StatusEffect
{
    public override void ApplyEffect(CharacterStats target)
    {
        var existingDebuff = target.activeStatusEffects.OfType<Debuff>().FirstOrDefault(debuff => debuff.GetType() == this.GetType());
        if (existingDebuff != null)
        {
            if(refresh) existingDebuff.currentDuration = duration;  // Reset duration
        }
        else
        {
            currentDuration = duration;
            target.activeStatusEffects.Add(Clone());
        }
    }

    public override void RemoveEffect(CharacterStats target)
    {
        target.activeStatusEffects.Remove(this);
    }
}

