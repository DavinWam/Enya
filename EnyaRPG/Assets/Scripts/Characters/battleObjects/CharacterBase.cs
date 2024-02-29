using UnityEngine;
using System.Collections.Generic;
using System;

public enum ActionType { ATTACK, IGNITE, USE_ITEM, SPELL,EMPTY }

public class Act
{
    public ActionType actionType;
    public CharacterBase target; // Stores the target character of the action
    public Spell spell;
    public bool isCritical = false;
    public bool isBlock = false;
    public bool isWeak = false;
    public bool IsFinished { get; private set; } = false;

    // Use this method to mark an action as finished when it's done.
    public void MarkAsFinished()
    {
        IsFinished = true;
    }
    public Act Copy(Act other)
    {
        Act newAct = new Act();
        newAct.actionType = other.actionType;
        newAct.target = other.target;
        newAct.spell = other.spell;
        newAct.isCritical = other.isCritical;
        newAct.isBlock = other.isBlock;
        newAct.isWeak = other.isWeak;
        return newAct;
        // Copy any other relevant properties
    }
    // This implicit operator allows you to directly assign an ActionType to an Act.
    // However, be careful with this as it could introduce bugs if used incorrectly.
    // Especially, this doesn't assign a target, which may or may not be problematic.
    public static implicit operator Act(ActionType v)
    {
        return new Act { actionType = v };  // This will set the actionType, but leave the target as null
    }

    // Additional methods or properties can be added as needed
}


public class CharacterBase : MonoBehaviour
{

    [Header("Character Attributes")]
    public string characterName;
    public Sprite characterSprite;
    private bool isAlive = true;
    public float distanceCovered;

    [Header("Character Stats & Status Effects")]
    public CharacterStats characterStats;


    // Property to get and set the alive status
    public bool IsAlive
    {
        get
        {
            // Ensure that the character is considered alive only if their health is >= 0
            return characterStats.currentHealth > 0;
        }
        set
        {
            // Set the internal alive status
            isAlive = value;
        }
    }
    public virtual CharacterStats GetStats()
    {
        return this.characterStats; // This assumes that all characters have a field or property named characterStats.
    }


    public Sprite GetSprite(){
        return characterSprite;
    }
    // Damage-related functionalities
    public virtual void TakeDamage(float damage,bool isCritical,bool isWeak,bool isBlock)
    {
        characterStats.currentHealth -= damage;

        if (characterStats.currentHealth <= 0)
        {
            IsAlive = false;
            Die();
        }
    }

    public void Heal(float healAmount, bool isCritical){
        characterStats.Heal(healAmount);
        FindObjectOfType<BattleController>().HealDamage(isCritical, this.transform.position,(int)healAmount);
    }

    public virtual void Attack()
    {
        Debug.Log("attacked from character base");
        //just a blueprint
    }
    public virtual void CastSpell()
    {
        Debug.Log("casted spell from character base");
        //just a blueprint
    }
    public virtual void CastSpell(Spell spell, CharacterBase target)
    {
        // Apply the spell effects on the target
        // This might involve adding more methods in Spell or using delegate/callback mechanisms.
        //spell.ApplyEffect(target);

    }

    public virtual Act Act()
    {
        // Default action for the base character; likely overridden by subclasses.
        return new Act { actionType = ActionType.EMPTY};
    }

    public virtual void Die()
    {
        Debug.Log(characterName + "has died");
        IsAlive = false;
        // Handle death logic here
        // This could include animations, gameplay mechanics, etc.
    }

}

// Note: You would need to provide implementations for `CharacterStats` and `StatusEffect` classes.
// The above code assumes these classes have certain methods or properties, but you can adjust as necessary.
