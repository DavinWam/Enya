using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;

[CreateAssetMenu(fileName = "New PlayerStats", menuName = "Stats/PlayerStats")]
public class PlayerStats : CharacterStats
{


    [Header("Base Stats")]
    public int exp; // Current exp points.
    public int totalExp; // Current exp points.
    public int expToNextLevel; // Experience needed to reach the next level.
    public int level; // Current level.
    public float baseMana; // Base mana or magic points without modifiers.
    public float currentMana; // Represents the character's current mana or magic points.
    public FireType fireType;
    public float critRate; // Base chance of landing a critical hit without modifiers.
    public float blockRate; // Base chance of blocking an incoming attack without modifiers.
    public List<Spell> spellList; // The list of spells currently available to the character.
    public List<Gear> equippedGear; // Gear currently equipped by the player character, organized by equipment slots.
    public const int MAX_LEVEL = 50;
    public const int MAX_AMOUNT_GEAR = 3;
    public bool isClone; // True if is a clone, used for picking stat growths.
    public StatGrowths growths;
    public List<StatAdjustment> adjustments; // Instance of the StatModifiers class which will hold the correction multipliers.
    public GameData gameData;


    public void GainExp(int amount)
    {
        totalExp += amount;
        exp += amount;

        while (exp >= expToNextLevel)
        {

            // Subtract the required exp for level up
            exp -= expToNextLevel;
            LevelUp();
            CalculateExpToNextLevel();
        }
    }
    public GameObject levelUpTextPrefab; // Assign this in the Inspector

    public void LevelUp()
    {
        if(level == PlayerStats.MAX_LEVEL) return;
        level++;
        if(isClone == false)
        {
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            levelUpTextPrefab = player.GetComponent<PlayerCharacter>().levelUpTextPrefab;
            // Instantiate the level-up text prefab
            GameObject levelUpText = Instantiate(levelUpTextPrefab, player.transform.position, Quaternion.identity, player.GetComponentInChildren<Canvas>().gameObject.transform);
            
            levelUpText.GetComponent<RectTransform>().anchoredPosition = new Vector3(0.5f, 1.5f, 0f);

            // Set the text (if needed)
            TextMeshProUGUI textComponent = levelUpText.GetComponent<TextMeshProUGUI>();
            if (textComponent != null)
            {
                textComponent.text = "Level Up!";
            }
        }

        // List of stats that level up
        StatType[] levelingStats =
        {
            StatType.ATTACK,
            StatType.DEFENSE,
            StatType.HEALTH,
            StatType.MANA,
            StatType.SPEED,
            StatType.CRIT_RATE,
            StatType.BLOCK_RATE
        };

        // Loop through stats that level up
        foreach (StatType stat in levelingStats)
        {
            // Calculate growth for the current stat
            float growthValue = growths.CalculateStatGrowth(stat);

            // Add growth to the current stat
            switch (stat)
            {
                case StatType.ATTACK:
                    baseAttack += growthValue;
                    break;

                case StatType.DEFENSE:
                    baseDefense += growthValue;
                    break;

                case StatType.HEALTH:
                    baseHealth += growthValue;
                    break;

                case StatType.MANA:
                    baseMana += growthValue;
                    break;

                case StatType.SPEED:
                    baseSpeed += growthValue;
                    break;

                case StatType.CRIT_RATE:
                    critRate += growthValue;
                    break;

                case StatType.BLOCK_RATE:
                    blockRate += growthValue;
                    break;
            }
        }
        List<Spell> newSpells = gameData.GetSpellsForLevelUp(level, fireType);
        foreach (Spell newSpell in newSpells)
        {
            if (!spellList.Contains(newSpell))
            {
                spellList.Add(newSpell);
            }
        }
        // Debug log
        Debug.Log($"{this} leveled up! They're now level {level}.");
    }



    public int CalculateExpToNextLevel()
    {
        return expToNextLevel = level * level * 10;
    }
    public override float GetEffectiveStat(StatType type)
    {
        float baseValue;

        switch (type)
        {
            case StatType.CRIT_RATE:
                baseValue = critRate;
                break;
            case StatType.BLOCK_RATE:
                baseValue = blockRate;
                break;
            case StatType.MANA:
                baseValue = baseMana;
                break;
            case StatType.CURRENT_MANA:
                baseValue = currentMana;
                break;
            default:
                baseValue = base.GetEffectiveStat(type);
                break;
        }

        // If the character is not a clone, apply adjustments.
        if (!isClone)
        {
            StatAdjustment selectedAdjustment = adjustments.FirstOrDefault(adj => adj.fireType == fireType);
            if (selectedAdjustment != null)
            {
                //Debug.Log($"adjusting by: {selectedAdjustment.attackMultiplier}");
                baseValue = selectedAdjustment.GetAdjustedValue(type, baseValue);
            }
        }
        // Apply gear modifiers

        float additiveAmount = 0f;
        float multiplicativeAmount = 1f; // Start with 1 for multiplicative boosts

        foreach (var gear in equippedGear)
        {
            if (gear.statType == type)
            {
                if (gear.isPercentage)
                {
                    multiplicativeAmount += gear.boostAmount / 100f;
                }
                else
                {
                    additiveAmount += gear.boostAmount;
                }
            }

        }
        
        baseValue = (baseValue + additiveAmount) * multiplicativeAmount;
        // Apply boosts from all applicable buffs
        additiveAmount = 0f;
        multiplicativeAmount = 1f; // Start with 1 for multiplicative boosts
        foreach (Buff buff in activeStatusEffects.OfType<Buff>().Where(b => b.affectedStatType == type || b.affectedStatType == StatType.ALL))
        {
            float boost = buff.GetTotalBoostAmount();
            if (buff.isPercentageBoost)
            {
                multiplicativeAmount += boost / 100f; // Convert percentage to multiplier
            }
            else
            {
                additiveAmount += boost;
            }
        }

        // Apply reductions from all applicable debuffs
        foreach (Debuff debuff in activeStatusEffects.OfType<Debuff>().Where(d => d.affectedStatType == type || d.affectedStatType == StatType.ALL))
        {
            float decrease = debuff.GetTotalBoostAmount(); // Assuming GetTotalBoostAmount returns negative values for debuffs
            if (debuff.isPercentageBoost)
            {
                multiplicativeAmount += decrease / 100f; // Convert percentage to multiplier
            }
            else
            {
                additiveAmount += decrease;
            }
        }


        baseValue = (baseValue + additiveAmount) * multiplicativeAmount;
        //make sure that when health is scaled you can't have more current hp than max hp
        if (type == StatType.CURRENT_HEALTH)
        {
            float health = GetEffectiveStat(StatType.HEALTH);
            if (health < baseValue)
            {
                baseValue = health;
                currentHealth = health;
            }
        }
        if (type == StatType.CURRENT_MANA)
        {
            float mana = GetEffectiveStat(StatType.MANA);
            if (mana < baseValue)
            {
                baseValue = mana;
                currentMana = mana;
            }
        }
        return baseValue;

    }

    public void RegenerateMana(float amount)
    {
        currentMana += amount;
        // Debug.Log($"mana: {currentMana}");
        if (currentMana > GetEffectiveStat(StatType.MANA))
        {
            currentMana = GetEffectiveStat(StatType.MANA);
        }
    }
    public bool EquipGear(Gear newGear){
        if(equippedGear.Count < MAX_AMOUNT_GEAR && newGear){
            newGear.equippedStats = this;
            equippedGear.Add(newGear);
            return true;
        }   
        return false;
    }
     public float GetAdjustedStat(StatType type)
    {
        float baseValue;

        switch (type)
        {
            case StatType.CRIT_RATE:
                baseValue = critRate;
                break;
            case StatType.BLOCK_RATE:
                baseValue = blockRate;
                break;
            case StatType.MANA:
                baseValue = baseMana;
                break;
            case StatType.CURRENT_MANA:
                baseValue = currentMana;
                break;
            default:
                baseValue = base.GetEffectiveStat(type);
                break;
        }

        // If the character is not a clone, apply adjustments.
        if (!isClone)
        {
            StatAdjustment selectedAdjustment = adjustments.FirstOrDefault(adj => adj.fireType == fireType);
            if (selectedAdjustment != null)
            {
                //Debug.Log($"adjusting by: {selectedAdjustment.attackMultiplier}");
                baseValue = selectedAdjustment.GetAdjustedValue(type, baseValue);
            }
        }
        if (type == StatType.CURRENT_HEALTH)
        {
            float health = GetEffectiveStat(StatType.HEALTH);
            if (health < baseValue)
            {
                baseValue = health;
                currentHealth = health;
            }
        }
        if (type == StatType.CURRENT_MANA)
        {
            float mana = GetEffectiveStat(StatType.MANA);
            if (mana < baseValue)
            {
                baseValue = mana;
                currentMana = mana;
            }
        }
        return baseValue;
    }
    // Method to get the raw stat value from the base class
    public float GetRawStatValue(StatType type)
    {   
        float baseValue;
        switch (type)
        {
            case StatType.CRIT_RATE:
                baseValue = critRate;
                break;
            case StatType.BLOCK_RATE:
                baseValue = blockRate;
                break;
            case StatType.MANA:
                baseValue = baseMana;
                break;
            case StatType.CURRENT_MANA:
                baseValue = currentMana;
                break;
            default:
                baseValue = base.GetEffectiveStat(type);
                break;
        }
        return baseValue;
    }

    public PlayerStats Clone()
    {
        // Start by cloning base class properties
        return Instantiate(this);
    }
}
