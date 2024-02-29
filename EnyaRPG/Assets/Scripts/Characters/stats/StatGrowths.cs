using UnityEngine;

[CreateAssetMenu(fileName = "New StatGrowths", menuName = "Stats/StatGrowths")]
public class StatGrowths : ScriptableObject
{
    public enum GrowthCategory
    {
        Low,
        Moderate,
        High,
        VeryHigh,
        Special,
    }

    public GrowthCategory healthGrowthCategory;
    public GrowthCategory manaGrowthCategory;
    public GrowthCategory attackGrowthCategory;
    public GrowthCategory defenseGrowthCategory;
    public GrowthCategory speedGrowthCategory;
    public GrowthCategory critGrowthCategory;
    public GrowthCategory blockGrowthCategory;

    private float healthMultiplier = 1.5f; // Special case for health
    private float defenseMultiplier = 0.25f; // Special case for defense, quartered
    private float manaMultiplier = 0.25f; // Special case for mana, quartered
    private float critMultiplier = 0.25f; // Special case for crit rate, quartered
    private float blockMultiplier = 0.25f; // Special case for block rate, quartered
                                          // Calculate the growth amount for a specific stat type
    public float CalculateStatGrowth(StatType statType)
    {
        switch (statType)
        {
            case StatType.HEALTH:
                return CalculateHealthGrowth();
            case StatType.MANA:
                return CalculateManaGrowth();
            case StatType.ATTACK:
                return CalculateAttackGrowth();
            case StatType.DEFENSE:
                return CalculateDefenseGrowth();
            case StatType.SPEED:
                return CalculateSpeedGrowth();
            case StatType.CRIT_RATE:
                return CalculateCritRateGrowth()/100f;
            case StatType.BLOCK_RATE:
                return CalculateBlockRateGrowth()/100f;
            default:
                return 0.0f; // Default is no growth
        }
    }

    // Calculate health growth with special case
    private float CalculateHealthGrowth()
    {
        switch (healthGrowthCategory)
        {
            case GrowthCategory.Low:
                return Random.Range(0, 1) * healthMultiplier;
            case GrowthCategory.Moderate:
                return Random.Range(1, 2) * healthMultiplier;
            case GrowthCategory.High:
                return Random.Range(2, 3) * healthMultiplier;
            case GrowthCategory.VeryHigh:
                return Random.Range(3, 5) * healthMultiplier;
            default:
                return 0.0f; // Default is no growth
        }
    }
    // Calculate mana growth with special case
    private float CalculateManaGrowth()
    {
        switch (manaGrowthCategory)
        {
            case GrowthCategory.Low:
                return Random.Range(0, 1) * manaMultiplier;
            case GrowthCategory.Moderate:
                return Random.Range(1, 2) * manaMultiplier;
            case GrowthCategory.High:
                return Random.Range(2, 3) * manaMultiplier;
            case GrowthCategory.VeryHigh:
                return Random.Range(3, 5) * manaMultiplier;
            default:
                return 0.0f; // Default is no growth
        }
    }

    // Implement similar functions for other stats (attack, defense, speed, crit, block)
    private float CalculateAttackGrowth()
    {
        switch (attackGrowthCategory)
        {
            case GrowthCategory.Low:
                return Random.Range(0, 2);
            case GrowthCategory.Moderate:
                return Random.Range(0, 3);
            case GrowthCategory.High:
                return Random.Range(1, 3);
            case GrowthCategory.VeryHigh:
                return Random.Range(1, 4);
            default:
                return 0.0f; // Default is no growth
        }
    }

    private float CalculateDefenseGrowth()
    {
        switch (defenseGrowthCategory)
        {
            case GrowthCategory.Low:
                return Random.Range(0, 1) * defenseMultiplier;
            case GrowthCategory.Moderate:
                return Random.Range(1, 2) * defenseMultiplier;
            case GrowthCategory.High:
                return Random.Range(2, 3) * defenseMultiplier;
            case GrowthCategory.VeryHigh:
                return Random.Range(3, 5) * defenseMultiplier;
            default:
                return 0.0f; // Default is no growth
        }
    }

    private float CalculateSpeedGrowth()
    {
        switch (speedGrowthCategory)
        {
            case GrowthCategory.Low:
                return Random.Range(0, 2);
            case GrowthCategory.Moderate:
                return Random.Range(0, 3);
            case GrowthCategory.High:
                return Random.Range(1, 3);
            case GrowthCategory.VeryHigh:
                return Random.Range(1, 4);
            default:
                return 0.0f; // Default is no growth
        }
    }

    private float CalculateCritRateGrowth()
    {
        switch (critGrowthCategory)
        {
            case GrowthCategory.Low:
                return Random.Range(0, 1) * critMultiplier;
            case GrowthCategory.Moderate:
                return Random.Range(1, 2) * critMultiplier;
            case GrowthCategory.High:
                return Random.Range(2, 3) * critMultiplier;
            case GrowthCategory.VeryHigh:
                return Random.Range(3, 5) * critMultiplier;
            default:
                return 0.0f; // Default is no growth
        }
    }

    private float CalculateBlockRateGrowth()
    {
        switch (blockGrowthCategory)
        {
            case GrowthCategory.Low:
                return Random.Range(0, 1) * blockMultiplier;
            case GrowthCategory.Moderate:
                return Random.Range(1, 2) * blockMultiplier;
            case GrowthCategory.High:
                return Random.Range(2, 3) * blockMultiplier;
            case GrowthCategory.VeryHigh:
                return Random.Range(3, 5) * blockMultiplier;
            default:
                return 0.0f; // Default is no growth
        }
    }
}
