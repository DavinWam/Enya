using UnityEngine;

[CreateAssetMenu(fileName = "New Gear", menuName = "Gear")]
public class Gear : ScriptableObject
{
    public string gearID;
    public string gearName;
    public string gearDescription;
    public StatType statType;
    public float boostAmount;
    public bool isPercentage;
    public Sprite gearIcon;
    public PlayerStats equippedStats = null;


    // Method to get the modified stat value

}
