using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class CharacterStats : ScriptableObject
{


    [Header("Base Stats")]
    public float baseHealth;
    public float currentHealth;
    public float baseAttack;
    public float baseDefense;
    public float baseSpeed;
    public FireType weakness;
    public List<StatusEffect> activeStatusEffects = new List<StatusEffect>();
    public virtual float GetEffectiveStat(StatType type)
    {
        switch (type)
        {
            case StatType.ATTACK:
                return GetAttack();
            case StatType.DEFENSE:
                return GetDefense();
            case StatType.HEALTH:
                return GetHealth();
            case StatType.CURRENT_HEALTH:
                return currentHealth;
            case StatType.SPEED:
                return GetSpeed();
            case StatType.CRIT_RATE:
                return GetCritRate();
            case StatType.BLOCK_RATE:
                return GetBlockRate();
            case StatType.CRIT_VULNERABILITY:
                return CritVulnerability();
            default:
                Debug.LogError("StatType not found!");
                return 0;
        }
    }
    public void Heal(float amount)
    {
        if(currentHealth < 0){
            currentHealth = 0;
        }
        currentHealth += amount;
        if (currentHealth > GetEffectiveStat(StatType.HEALTH))
            currentHealth =  GetEffectiveStat(StatType.HEALTH);
    }
    // Sample functions for fetching the  stats.
    // You can expand or adjust based on your game's mechanics.

    private float GetAttack()
    {
        // Example logic for calculating  attack
        // Factor in equipment, buffs, debuffs, etc.
        return baseAttack;
    }

    private float GetDefense()
    {
        // Example logic for calculating  defense
        return baseDefense;
    }

    private float GetHealth()
    {
        // Example logic for calculating  health
        return baseHealth;
    }


    private float GetSpeed()
    {
        // Example logic for calculating  speed
        return baseSpeed;
    }

    private float GetCritRate()
    {
        // Example logic for calculating  crit rate
        return 0; // Placeholder
    }

    private float GetBlockRate()
    {
        // Example logic for calculating  block rate
        return 0; // Placeholder
    }
    public float CritVulnerability()
    {
        float totalVulnerability = 0f;

        // Loop through each active status effect
        foreach (StatusEffect effect in activeStatusEffects)
        {
            // If the effect affects Crit Vulnerability, add its value
            if (effect.affectedStatType == StatType.CRIT_VULNERABILITY)
            {
                totalVulnerability += effect.boostAmount;
            }
        }

        // Return the base Crit Vulnerability combined with the total from the effects
        return totalVulnerability;
    }
    public void RemoveAllActiveStatusEffects()
    {
        while (activeStatusEffects.Count > 0)
        {
            activeStatusEffects[0].RemoveEffect(this);
        }
    }

    protected CharacterStats CloneBaseStats()
    {
        CharacterStats clone = ScriptableObject.CreateInstance<CharacterStats>();

        // Copy fields from CharacterStats
        clone.baseHealth = this.baseHealth;
        clone.currentHealth = this.currentHealth;
        clone.baseAttack = this.baseAttack;
        clone.baseDefense = this.baseDefense;
        clone.baseSpeed = this.baseSpeed;
        clone.weakness = this.weakness;
        clone.activeStatusEffects = new List<StatusEffect>(this.activeStatusEffects);

        return clone;
    }
}
