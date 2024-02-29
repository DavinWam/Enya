using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "GameData", menuName = "GameData/GameDataDatabase", order = 0)]
public class GameData : ScriptableObject
{
    // [Header("Gear Data")]
    //public List<Gear> gearList = new List<Gear>();

    [Header("Spell Data")]
    public List<Spell> spellsList = new List<Spell>();
    [Header("Status Effect Data")]
    public List<Buff> buffsList = new List<Buff>(); // List to store all Buff objects
    public List<Debuff> debuffsList = new List<Debuff>(); // List to store all Debuff objects
    public int nextPartyMember = 0;
    public Vector3 respawnLocation;
    public PartyManager partyManager;
    public List<FireType> unlockedFireTypes; // Populate this list with unlocked types
    public List<GameObject> potentialPartyMembersList;
    public List<PlayerStats> potentialPartyMembersStats;

     public List<StatAdjustment> statAdjustments; // Populate this list with unlocked types

    // [Header("Status Effect Data")]
    // public List<StatusEffect> statusEffectList = new List<StatusEffectSO>();

    // [Header("Dialogue Data")]
    // public List<DialogueSO> dialoguesList = new List<DialogueSO>();

    // Methods
    // public Gear GetGearByID(int id)
    // {
    //     return gearList.Find(gear => gear.id == id);
    // }
    // Method to get a random buff
    public Buff GetRandomBuff()
    {
        if (buffsList.Count == 0)
        {
            Debug.LogError("No buffs available in GameData!");
            return null;
        }

        int randomIndex = Random.Range(0, buffsList.Count);
        return buffsList[randomIndex];
    }
    public Spell GetSpellByName(string name)
    {
        return spellsList.Find(spell => spell.spellName == name);
    }
    public List<Spell> GetSpellsForLevelUp(int level, FireType fireType)
    {
        return spellsList.FindAll(spell => spell.unlockLevel <= level && spell.fireType == fireType);
    }

    // public StatusEffect GetStatusEffectByName(string name)
    // {
    //     return statusEffectList.Find(effect => effect.name == name);
    // }

    // public DialogueSO GetDialogueByID(int id)
    // {
    //     return dialoguesList.Find(dialogue => dialogue.id == id);
    // }
}
