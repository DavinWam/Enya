using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class EncounterRespawner : MonoBehaviour
{
    public EncounterManager encounterManager;

    private void Start()
    {
        StartCoroutine(RespawnCheck());
    }

    private IEnumerator RespawnCheck()
    {
        while (true)
        {
            yield return new WaitForSeconds(10f);  // Check every 10 seconds for demonstration. Adjust as needed.

            List<int> encounterIds = new List<int>(encounterManager.defeatedEncounters.Keys);
            for (int i = 0; i < encounterIds.Count; i++)
            {
                Encounter encounter = encounterManager.encounterList.Find(e => e.encounterID == encounterIds[i]);
                if (encounterManager.CheckRespawn(encounter))
                {
                    encounterManager.RespawnEncounter(encounter, false);
                    encounterManager.defeatedEncounters.Remove(encounterIds[i]);
                    i--;  // Decrement the index as the dictionary size has changed
                }
            }
        }
    }

}
