using System.Collections.Generic;
using JetBrains.Annotations;
using Unity.VisualScripting;
using UnityEngine;

[CreateAssetMenu(fileName = "PartyData", menuName = "RPG/PartyManager", order = 1)]
public class PartyManager : ScriptableObject
{
    [Header("Party Configuration")]
    public List<GameObject> generalPartyMembersPrefabs; // General list to store all player prefabs
    public List<GameObject> activePartyMembersPrefabs;
    public List<GameObject> inactivePartyMembersPrefabs;

    public PlayerStats playerStats;
    public List<PlayerStats> cloneStats; // Sorted clone prefabs

    // List to store instantiated active party members
    private List<GameObject> activePartyInstances = new List<GameObject>();

    [Header("Resources")]
    private const int MAX_HEALING_ITEMS = 5;
    public int currentHealingItemCount = 5;

    public List<Gear> inventory;
    private OverworldUI overworldUI;

    // Method to get PlayerStats based on a GameObject
    // Function to get the character's name

    public void Start(){
        //the gear in inventory can be instanced so we have to make sure that their is no garbage when we restart
        foreach(Gear gear in inventory){
            if(gear == null){
                inventory.Remove(gear);
            }
        }
        List<PlayerStats> removeNullGear = new List<PlayerStats>();
        removeNullGear.Add(playerStats);
        removeNullGear.AddRange(cloneStats);
        foreach(PlayerStats ps in removeNullGear){
            foreach(Gear gear in ps.equippedGear){
                if(gear == null){
                    ps.equippedGear.Remove(gear);
                }
            }
        }
    }
    
    public PlayerStats GetStat(GameObject clonePrefab)
    {
        // Assuming playerPrefab has a component or a property that gives its FireType
        FireType cloneFireType = clonePrefab.GetComponent<PlayerCharacter>().GetPlayerStats().fireType; // Adjust this line as needed

        foreach (PlayerStats stats in cloneStats)
        {
            if (stats.fireType == cloneFireType)
            {
                return stats;
            }
        }

        return null; // Return null if no matching FireType is found
    }

    public PlayerStats GetStatsByIndex(int index){
        if(index == 0){
            return playerStats;
        }else{
            return cloneStats[index-1];
        }
    }
    public string GetCharacterName(PlayerStats stats)
    {
        if (stats == null)
        {
            return string.Empty;
        }

        if (stats == playerStats)
        {
            PlayerCharacter playerCharacter = FindObjectOfType<PlayerCharacter>();
            return playerCharacter != null ? playerCharacter.characterName : string.Empty;
        }

        for (int i = 0; i < cloneStats.Count; i++)
        {
            if (cloneStats[i] == stats)
            {
                GameObject tempPrefab = Instantiate(generalPartyMembersPrefabs[i]);
                PlayerCharacter memberCharacter = tempPrefab.GetComponent<PlayerCharacter>();
                string name = memberCharacter != null ? memberCharacter.characterName : string.Empty;
                Destroy(tempPrefab);
                return name;
            }
        }

        return string.Empty;
    }
    public Sprite GetCharacterSprite(PlayerStats stats)
    {
        if (stats == null)
        {
            return null;
        }
//        Debug.Log(playerStats.isClone);

        if (stats == playerStats)
        {
            PlayerCharacter playerCharacter = FindObjectOfType<PlayerCharacter>();
            Debug.Log(playerCharacter.characterName);
            return playerCharacter != null ? playerCharacter.characterSprite : null;
        }

        for (int i = 0; i < cloneStats.Count; i++)
        {
            if (cloneStats[i] == stats)
            {
                GameObject tempPrefab = Instantiate(generalPartyMembersPrefabs[i]);
                PlayerCharacter memberCharacter = tempPrefab.GetComponent<PlayerCharacter>();
                Sprite sprite = memberCharacter != null ? memberCharacter.characterSprite : null;
                Destroy(tempPrefab);
                return sprite;
            }
        }

        return null;
    }

    public void AddToInventory(List<Gear> newGear){
        foreach( Gear item in newGear){
            inventory.Add(Instantiate(item));
        }
        
        overworldUI = FindObjectOfType<OverworldUI>();
        if(overworldUI){
            Debug.Log("displaying gear notifs");
            overworldUI.DisplayItemInfo(newGear);
        }
    }
    // This method will spawn the active party members except the player
    public void SpawnActivePartyMembers(Vector3 spawnPosition)
    {
        // Clear previous instances
        foreach (var member in activePartyInstances)
        {
            Destroy(member);
        }
        activePartyInstances.Clear();

        foreach (var prefab in activePartyMembersPrefabs)
        {
            // Don't spawn if it's the player (assuming only player has the tag "Player")
            if (prefab.CompareTag("Player")) continue;

            GameObject instance = Instantiate(prefab, spawnPosition, Quaternion.identity);
            activePartyInstances.Add(instance);
        }
    }


    // PartyManager.cs
    public void AddToActiveParty(GameObject memberPrefab)
    {
        if (!activePartyMembersPrefabs.Contains(memberPrefab) && activePartyMembersPrefabs.Count < 4)
        {
            activePartyMembersPrefabs.Add(memberPrefab);
            inactivePartyMembersPrefabs.Remove(memberPrefab);
        }
    }

    public void RemoveFromActiveParty(GameObject memberPrefab)
    {
        if (activePartyMembersPrefabs.Contains(memberPrefab))
        {
            inactivePartyMembersPrefabs.Add(memberPrefab);
            activePartyMembersPrefabs.Remove(memberPrefab);
        }
    }
    public bool CanUseHealingItem()
    {
        // Check if there are any healing items left
        if (currentHealingItemCount <= 0)
        {
            return false;
        }

        // Check if the player character is not at full health
        if (playerStats.currentHealth < playerStats.GetEffectiveStat(StatType.HEALTH))
        {
            currentHealingItemCount--;
            return true;
        }

        // Check each clone character to see if they are not at full health
        foreach (var clone in activePartyInstances)
        {
            if (clone.GetComponent<PlayerCharacter>().GetPlayerStats().currentHealth < clone.GetComponent<PlayerCharacter>().GetPlayerStats().GetEffectiveStat(StatType.HEALTH))
            {
                currentHealingItemCount--;
                return true;
            }
        }

        // All characters are at full health, so return false
        return false;
    }


    public void UseHealingItem()
    {
        if (CanUseHealingItem())
        {
            float healAmount = playerStats.GetEffectiveStat(StatType.HEALTH) * 0.3f; 
            playerStats.Heal(healAmount);
            foreach (var member in cloneStats)
            {

                    healAmount = member.GetEffectiveStat(StatType.HEALTH) * 0.3f; // 30% of max health
                    member.Heal(healAmount);
            }
        
            
        }

    }

    public void ReplenishHealingItems()
    {
        currentHealingItemCount = MAX_HEALING_ITEMS;
    }
}
