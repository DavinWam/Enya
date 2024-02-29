using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(menuName = "Manager/EncounterManager")]
public class EncounterManager : ScriptableObject
{
    public List<Encounter> encounterList;
    private Dictionary<Encounter, BattleTrigger> encounterTriggerMap = new Dictionary<Encounter, BattleTrigger>();

    public Dictionary<int, float> defeatedEncounters = new Dictionary<int, float>();
    public float respawnTime = 300f;  // in seconds
    // Attribute to keep track of the current encounter
    public Encounter currentEncounter { get; private set; }
    public void EncounterDefeated(Encounter encounter)
    {
        if (!defeatedEncounters.ContainsKey(encounter.encounterID))
        {
            defeatedEncounters.Add(encounter.encounterID, Time.time);
        }
        else
        {
            defeatedEncounters[encounter.encounterID] = Time.time;
        }
    }
    public void RegisterTrigger(BattleTrigger trigger, Encounter associatedEncounter)
    {
        if (!encounterTriggerMap.ContainsKey(associatedEncounter))
        {
            encounterTriggerMap.Add(associatedEncounter, trigger);
        }
    }

    public bool CheckRespawn(Encounter encounter)
    {
        if (defeatedEncounters.ContainsKey(encounter.encounterID))
        {
            float timeDefeated = defeatedEncounters[encounter.encounterID];
            if (Time.time - timeDefeated >= respawnTime)
            {
                return true;
            }
        }
        return false;
    }

    public void RespawnEncounter(Encounter encounter, bool force)
    {
        if (CheckRespawn(encounter) || force)
        {
            BattleTrigger associatedTrigger = null;
            if (encounterTriggerMap.TryGetValue(encounter, out associatedTrigger))
            {
                // Check if the trigger is already active
                if (associatedTrigger.isActive)
                {
                    Debug.Log("The trigger is already active. Skipping respawn.");
                    return;
                }

                Vector3 spawnPosition = new Vector3(associatedTrigger.transform.position.x, associatedTrigger.transform.localScale.y, associatedTrigger.transform.position.z);
                encounter.Spawn(spawnPosition, associatedTrigger.transform.localScale);
                // Set the trigger to active
                associatedTrigger.isActive = true;;
            }
            else
            {
                Debug.LogError("No associated BattleTrigger found for the encounter.");
            }
       }
    }


        // Method to set the current encounter
    public void SetCurrentEncounter(Encounter encounter)
    {
        currentEncounter = encounter;
    }

    // Method to clear the current encounter (e.g., after it ends)
    public void ClearCurrentEncounter()
    {
        currentEncounter = null;
    }
}
