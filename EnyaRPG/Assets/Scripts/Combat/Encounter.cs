using UnityEngine;
using System.Collections.Generic;
using System.Collections;
using System;

[System.Serializable]
public class AdjacencyDefinition
{
    public int mainIndex;
    public List<int> adjacentIndices;
}
[CreateAssetMenu(menuName = "Game/Encounter")]

public class Encounter : ScriptableObject
{
    public int encounterID; // A unique identifier for each encounter.
    public AudioClip battleMusic;
    public bool ifFirstLoad = true;
    public Vector3 offset;
    public List<GameObject> enemyPrefabs; // The prefabs for the enemies in this encounter.
    public List<EnemyStats> enemyStats; // The prefabs for the enemies in this encounter.
    public List<GameObject> spawnedEnemies; // The prefabs for the enemies in this encounter.
    public List<Vector3> enemyBattlePositions; // Positions where enemies will stand during the battle.
    public List<Vector3> partyBattlePositions; // Positions where the party members will stand during the battle (up to 4).
    public int temp;
    [Header("Adjacency Info")]
    public List<AdjacencyDefinition> playerAdjacencyDefinitions;
    public List<AdjacencyDefinition> enemyAdjacencyDefinitions;
    
    public Vector3 savedLocation;
    private Vector3 savedBounds;
    // Method to set battle positions from Transform lists

    public void SetBattlePositions(List<Transform> playerTransforms, List<Transform> enemyTransforms)
    {
        partyBattlePositions.Clear();
        enemyBattlePositions.Clear();

        foreach (Transform playerTransform in playerTransforms)
        {
            partyBattlePositions.Add(playerTransform.position);
        }

        foreach (Transform enemyTransform in enemyTransforms)
        {
            enemyBattlePositions.Add(enemyTransform.position);
        }
    }

    public void Spawn(Vector3 location, Vector3 bounds)
    {
        spawnedEnemies = new List<GameObject>();
        // Logic to instantiate the encounter at its specific location.
        for (int i = 0; i < enemyPrefabs.Count; i++)
        {
            if (enemyPrefabs[i])
            {
                UIManager uiManager = FindObjectOfType<UIManager>();
                GameObject spawnedEnemy = Instantiate(enemyPrefabs[i], location, Quaternion.identity); // I noticed you used a placeholder 'enemyPrefab' and 'spawnPosition', 'spawnRotation', updated it to use the list and location.
                spawnedEnemy.transform.SetParent(GameObject.FindGameObjectWithTag("EnemyParentTag").transform);
                
                EnemyCharacter enemyCharacter = spawnedEnemy.GetComponent<EnemyCharacter>();
                if(spawnedEnemy){
                    
                    if(i != 0)
                    {
                        enemyCharacter.characterName += $"{i}";
                    }
                    

                    // Clone the stats and spells
                    EnemyStats clonedStats = Instantiate(enemyStats[i]);
                    enemyCharacter.SetEnemyStats(clonedStats);
                    clonedStats.currentHealth = clonedStats.GetEffectiveStat(StatType.HEALTH);
                    clonedStats.activeStatusEffects = new List<StatusEffect>();

                    // Clone the spells for this enemy
                    for (int j = 0; j < clonedStats.spells.Count; j++)
                    {
                        EnemySpell originalSpell = clonedStats.spells[j];
                        EnemySpell clonedSpell = Instantiate(originalSpell);
                        clonedSpell.currentCooldown = 0;  // Reset cooldown for the cloned spell
                        clonedStats.spells[j] = clonedSpell;  // Replace the spell in the list
                    }

                    // Set roaming area (assuming you have a method named 'SetBounds' in the EnemyRoam script)
                    if(ifFirstLoad == true)
                    {
                        savedBounds = bounds;
                        savedLocation = location;
                        spawnedEnemy.GetComponent<EnemyRoam>().SetBounds(location, bounds);
                    }else
                    {
                        spawnedEnemy.GetComponent<EnemyRoam>().SetBounds(savedLocation, savedBounds);
                    }
                    // Update the weakness UI
                    enemyCharacter.UpdateWeaknessUI(uiManager);
                    spawnedEnemies.Add(spawnedEnemy);
                    Debug.Log(spawnedEnemies[i].GetComponent<CharacterBase>().characterName);

                    spawnedEnemy.transform.position = location;
                }else{
                    Debug.LogError($"Encounter{encounterID} could not find character component for enemy {i}");
                }

                
            }
            else
            {
                Debug.LogError($"Prefab is missing in encounter {encounterID}");
            }
        }
        ifFirstLoad = false;
    }

    public List<GameObject> GetSpawnedEnemies()
    {
        return spawnedEnemies;
    }

    public List<Vector3> PrepareEntitiesBattlePositions(GameObject playerObject, List<GameObject> spawnedEnemies)
    {
        List<Vector3> positions = new List<Vector3>
        {
            partyBattlePositions[0]  // player's position
        };

        for (int i = 0; i < spawnedEnemies.Count; i++)
        {
            // Disable roaming behavior
            var roamingBehavior = spawnedEnemies[i].GetComponent<EnemyRoam>();
            if (roamingBehavior != null)
                roamingBehavior.enabled = false;
            positions.Add(enemyBattlePositions[i]);
        }

        return positions;
    }

    public void Defeat()
    {
        // Logic to mark the encounter as defeated and hide/disable it from the game world.
    }
}
