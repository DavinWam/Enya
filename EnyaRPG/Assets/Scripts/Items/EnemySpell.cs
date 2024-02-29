using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[CreateAssetMenu(menuName = "Spells/EnemySpell")]

public class EnemySpell : Spell
{
    public enum TargetType { Self, Enemy, Party, Other,None, LowestHealth, }        
    public TargetType targetType;
    [Header("Enemy Specific Properties")]
    public Sprite EffectIcon; // Icon representing the type of attack
    public float chance; // Chance to choose this spell each turn
    public int windUpLength; // Number of turns for wind-up before the spell is cast
    public int cooldown; // Cooldown in turns
    public int currentCooldown; // Current cooldown counter
    public GameObject spawn;
    public EnemyStats spawnStats;

    public GameObject Spawn(Vector3 location, int i)
    {
        if (spawn)
        {
            // Instantiate the enemy at the specified location
            GameObject spawnedEnemy = Instantiate(spawn, location, Quaternion.identity);

            // Clone the spawnStats and assign to the spawned enemy
            EnemyStats clonedStats = Instantiate(spawnStats);
            EnemyCharacter enemyCharacter = spawnedEnemy.GetComponent<EnemyCharacter>();
            if (enemyCharacter)
            {
                // Clone the spells for this enemy
                for (int j = 0; j < clonedStats.spells.Count; j++)
                {
                    EnemySpell originalSpell = clonedStats.spells[j];
                    EnemySpell clonedSpell = Instantiate(originalSpell);
                    clonedSpell.currentCooldown = 0;  // Reset cooldown for the cloned spell
                    clonedStats.spells[j] = clonedSpell;  // Replace the spell in the list
                }

                enemyCharacter.SetEnemyStats(clonedStats);
                enemyCharacter.characterName += $"{spawnedEnemy.name} {i}";


                spawnedEnemy.GetComponent<EnemyRoam>().enabled = false;
                // Update the weakness UI
                UIManager uiManager = FindObjectOfType<UIManager>();
                enemyCharacter.UpdateWeaknessUI(uiManager);
                return spawnedEnemy;
            }
        }
        return null;
    }
}
