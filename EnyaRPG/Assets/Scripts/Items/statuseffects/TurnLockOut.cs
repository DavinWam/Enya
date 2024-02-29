using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "TurnLockout", menuName = "StatusEffects/TurnLockout", order = 2)]
public class TurnLockout : Debuff
{


    public override string GetDescription()
    {
        return $"Locked from doing anything for {currentDuration} actions.";
    }
}