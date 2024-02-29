using UnityEngine;
using System.Collections.Generic;

public class BattleTrigger : MonoBehaviour 
{
    public Encounter encounter;
    public EncounterManager encounterManager;
    public BattleController battleController;
    public bool isActive = true;

    private PlayerController playerController;
    public List<Transform> playerPositions; // Transforms for player positions
    public List<Transform> enemyPositions; // Transforms for enemy positions

    private void Awake()
    {
        // Register with the EncounterManager
        encounterManager.RegisterTrigger(this, encounter);
    }
    void OnDrawGizmos()
    {
        // Set the Gizmo color to red with half alpha (transparency)
        Gizmos.color = new Color(1, 0, 0, 0.5f);

        // Draw a wireframe cube with the custom color
        // Replace 'transform.position' with the center of the cube
        // Replace 'Vector3.one' with the desired size of the cube
        Gizmos.DrawWireCube(transform.position, Vector3.one);
    }


    void Start()
    {
        if (!battleController || !encounterManager)
        {
            Debug.LogError("Trigger is missing Battle Controller or Encounter Manager");
        }
        
        // Pass the transforms to the Encounter to set up positions
        encounter.SetBattlePositions(playerPositions, enemyPositions);
        Vector3 spawnPosition = new Vector3(transform.position.x, transform.position.y, transform.position.z);
        // Obtain the collider's size for accurate roaming bounds
        BoxCollider collider = GetComponent<BoxCollider>();
        Vector3 boundsSize = collider != null ? collider.size : Vector3.one;  // Fallback to (1,1,1) if no collider is attached
        encounter.ifFirstLoad = true;
        encounter.Spawn(spawnPosition, boundsSize);
    }
    void OnTriggerEnter(Collider other) 
    {
        if (other.CompareTag("Player") && isActive) 
        {
            playerController = other.GetComponent<PlayerController>();
            isActive = false;
            encounterManager.SetCurrentEncounter(encounter);
            StartCoroutine(battleController.StartBattle(encounter, encounter.GetSpawnedEnemies(), playerController));

        }
    }

}
