using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "StaticChargedDebuff", menuName = "StatusEffects/StaticChargedDebuff", order = 1)]
public class StaticChargedDebuff : Debuff
{
    //20% more susceptible

    // Adjust the effective stat of the target to reflect increased crit vulnerability.
    public override float GetTotalBoostAmount()
    {
        return -boostAmount;
    }
}
