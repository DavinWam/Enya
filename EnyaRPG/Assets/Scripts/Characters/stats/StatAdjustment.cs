using UnityEngine;

[CreateAssetMenu(fileName = "New StatAdjustment", menuName = "Stats/StatAdjustment")]
public class StatAdjustment : ScriptableObject
{
    public FireType fireType;
    public float healthMultiplier ;
    public float manaMultiplier ;
    public float attackMultiplier ;
    public float defenseMultiplier ;
    public float speedMultiplier ;
    public float critMultiplier ;
    public float blockMultiplier ;

    public float GetAdjustedValue(StatType type, float rawValue)
    {
        switch (type)
        {
            case StatType.ATTACK:
                return rawValue * attackMultiplier;
            case StatType.DEFENSE:
                return rawValue * defenseMultiplier;
            case StatType.HEALTH:
                return rawValue * healthMultiplier;
            case StatType.MANA:
                return rawValue * manaMultiplier;
            case StatType.SPEED:
                return rawValue * speedMultiplier;
            case StatType.CRIT_RATE:
                return rawValue * critMultiplier;
            case StatType.BLOCK_RATE:
                return rawValue * blockMultiplier;
            default:
                return rawValue; // Default is the raw value without any adjustments.
        }
    }
    

    // Additional methods for modifying or reading the multipliers can be added if required.
}
