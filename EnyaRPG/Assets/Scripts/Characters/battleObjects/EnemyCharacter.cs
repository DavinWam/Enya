using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class EnemyCharacter : CharacterBase
{
    public enum AIType { Aggressive, Defensive, StatusEffectPriority } // Add more AI types as required

    [Header("Enemy Specific Attributes")]
    public AIType aiType;
    [Header("UI Components")]
    public Slider healthBar;
    public Slider damageBar;
    private System.Random random = new System.Random(); // Initialize it here
    private BattleController battleController;
    [Header("Weakness UI Components")]
    public Image weaknessSpriteUI; // Assign this in the inspector
    [Header("UI Components for Queued Attack")]
    public GameObject attackQueueUI; // The UI element that indicates a queued attack
    public Image attackIcon; // The UI element to display the spell's effect icon
    public TextMeshProUGUI timerText; // The UI element to display the wind-up counter

    [Header("Enemy AI")]
    private EnemySpell queuedSpell; // The spell that is currently queued
    private int windUpCounter; // Counter for the wind-up turns
    public bool isBoss;
    private bool hasActivatedBossMode = false;
    public float enrageChance = 0f;

    void Awake()
    {
        battleController = FindObjectOfType<BattleController>();

    }
    public EnemyStats GetEnemyStats(){
        return characterStats as EnemyStats;
    }
    public void SetEnemyStats(EnemyStats instanceStats)
    {
        // Assuming statsPrefab has an EnemyStats component
        this.characterStats = instanceStats;
    }
    private void UpdateHealthBar()
    {
        if (healthBar)
        {
            healthBar.value = characterStats.currentHealth;
        }
        if (damageBar)
        {
            StartCoroutine(UpdateSubBar(damageBar, characterStats.currentHealth));
        }
    }
    private IEnumerator UpdateSubBar(Slider subBar, float targetValue)
    {
        yield return new WaitForSeconds(1f);
        float duration = 2f; // Duration over which the bar will decrease
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float newValue = Mathf.Lerp(subBar.value, targetValue, elapsed / duration);
            subBar.value = newValue;
            yield return null;
        }

        subBar.value = targetValue; // Ensure the final value is set
    }
    public override void TakeDamage(float damage, bool isCritical, bool isWeak, bool isBlock)
    {
        if (IsAlive == false) return;

        if (isBoss && !hasActivatedBossMode && characterStats.currentHealth <= characterStats.GetEffectiveStat(StatType.HEALTH) / 2)
        {
            ActivateBossMode();
            hasActivatedBossMode = true;
        }

        healthBar.maxValue = characterStats.GetEffectiveStat(StatType.HEALTH); // assuming maxHealth exists in your stats
        healthBar.value = characterStats.currentHealth;
        damageBar.maxValue = healthBar.maxValue;
        damageBar.value = healthBar.value;
        characterStats.currentHealth -= damage;


        FindObjectOfType<BattleController>().DealDamage(isCritical, isWeak, false, this.transform.position, (int)damage);
        // Update health bar UI
        UpdateHealthBar();
        if (characterStats.currentHealth <= 0)
        {
           
            Die();
        }
    }
    private void ActivateBossMode()
    {
        // Increase spell casting chance and attack stat

        enrageChance = .2f; // example: increase chance by 50%
        characterStats.baseAttack *= 1.2f;
        // Change the color of the health bar's fill area
        Image fillArea = healthBar.fillRect.GetComponent<Image>();
        if (fillArea != null)
        {
            fillArea.color *= 5f;
        }
    }
    public override void Die()
    {
        base.Die();
        Rigidbody rb = GetComponent<Rigidbody>();
        // Unlock rotation constraints
        if (rb != null)
        {
            rb.constraints = RigidbodyConstraints.None; // Or any specific constraints you want to unlock

            // Apply a small force to the right
            Vector3 forceDirection = Vector3.right * 50f; // Adjust the force magnitude as needed
            rb.AddForce(forceDirection, ForceMode.Impulse);
        }
    }

    public override Act Act()
    {
        if (windUpCounter > 0)
        {
            // Wind-up countdown
            windUpCounter--;
            if (timerText) timerText.text = windUpCounter.ToString();
            Debug.Log($"wind-Up{windUpCounter}");
            if (windUpCounter == 0)
            {
                // Cast the queued spell
                return CastQueuedSpell();
            }
        }

        // Check for new action
        foreach (var enemySpell in GetEnemyStats().spells)
        {
            if (enemySpell.currentCooldown == 0)
            {
                float roll = random.Next(0, 100) / 100f;
                if (roll <= enemySpell.chance + enrageChance)
                {
                    if(enemySpell.windUpLength != 0){
                        queuedSpell = enemySpell;
                        // Queue the spell and return a wait action
                        Debug.Log($"queueing {queuedSpell.spellName}");
                        BattleUI battleUI = FindObjectOfType<BattleUI>();
                        battleUI.SpellLabel.SetActive(true);
                        battleUI.SpellLabel.GetComponentInChildren<TextMeshProUGUI>().text = $"{characterName} is readying {queuedSpell.spellName}";
                        
                        windUpCounter = enemySpell.windUpLength;
                        enemySpell.currentCooldown = enemySpell.cooldown; // Set the cooldown

                        // Update the UI elements
                        attackQueueUI.SetActive(true);
                        if (attackIcon) attackIcon.sprite = enemySpell.EffectIcon; // Assuming the Spell class has an EffectIcon property
                        if (timerText) timerText.text = windUpCounter.ToString();

                        return ChooseRegularAttack(); // Return a regular attack or wait action
                    }else
                    {
                        Act spellAction = new Act
                        {
                            actionType = ActionType.SPELL,
                            spell = enemySpell,
                            target = ChooseSpellTarget(enemySpell)
                        };
                        return spellAction;
                    }

                }
            }else{
                enemySpell.currentCooldown -=1;
            }
        }

        // Default to regular attack if no spell is chosen
        return ChooseRegularAttack();
    }


    private Act ChooseRegularAttack()
    {
        Act chosenAction = new Act
        {
            actionType = ActionType.ATTACK,
            target = battleController.alivePlayers[random.Next(0, battleController.alivePlayers.Count)].GetComponent<CharacterBase>()
        };

        return chosenAction;
    }
    private Act CastQueuedSpell()
    {

        Act spellAction = new Act
        {
            actionType = ActionType.SPELL,
            spell = queuedSpell,
            target = ChooseSpellTarget(queuedSpell)
        };
        attackQueueUI.SetActive(false);
        queuedSpell.currentCooldown = queuedSpell.cooldown; // Set the cooldown after casting
        spellAction.spell = queuedSpell;//store the spell in the act        
        Debug.Log($"cast {spellAction.spell.spellName}");
        queuedSpell = null; // Clear the queued spell
        Debug.Log($"cast {spellAction.spell.spellName}");
        return spellAction;
    }

    private CharacterBase FindLowestHealthPlayer(List<GameObject> playerCharacters)
    {
        CharacterBase lowestHealthPlayer = null;
        float lowestHealth = float.MaxValue;

        foreach (var playerObj in playerCharacters)
        {
            var player = playerObj.GetComponent<CharacterBase>();
            if (player != null && player.characterStats.currentHealth < lowestHealth)
            {
                lowestHealthPlayer = player;
                lowestHealth = player.characterStats.currentHealth;
            }
        }

        return lowestHealthPlayer;
    }

    public CharacterBase ChooseSpellTarget(EnemySpell spell)
    {
        switch (spell.targetType)
        {
            case EnemySpell.TargetType.Self:
                return this; // The enemy targets itself

            case EnemySpell.TargetType.Enemy:
                // Choose a random enemy from the aliveEnemies list
                return battleController.aliveEnemies[random.Next(0, battleController.aliveEnemies.Count)].GetComponent<CharacterBase>();
            case EnemySpell.TargetType.Party:
                // Choose a random player from the alivePlayers list
                return battleController.alivePlayers[random.Next(0, battleController.alivePlayers.Count)].GetComponent<CharacterBase>();
            case EnemySpell.TargetType.LowestHealth:
                //chooses the lowest health player
                return FindLowestHealthPlayer(battleController.alivePlayers);              
            case EnemySpell.TargetType.Other:
                // Choose a random enemy that is not the caster
                List<CharacterBase> otherEnemies = battleController.aliveEnemies
                    .Where(e => e.GetComponent<CharacterBase>() != this)
                    .Select(e => e.GetComponent<CharacterBase>())
                    .ToList();
                return otherEnemies.Count > 0 ? otherEnemies[random.Next(0, otherEnemies.Count)] : null;
            case EnemySpell.TargetType.None:
                return null;
            default:
                throw new InvalidOperationException("Unknown spell target type");
        }
    }


    public CharacterBase ChooseTarget()
    {
        // Example AI logic to choose a target from the provided list of player characters
        // For this example, we'll just pick a random target
        return  battleController.alivePlayers[random.Next(0, battleController.alivePlayers.Count)].GetComponent<CharacterBase>();
    }
    public void UpdateWeaknessUI(UIManager uiManager)
    {
        if (uiManager == null || weaknessSpriteUI == null) return;

        Sprite weaknessSprite = uiManager.GetSpriteForFireType(characterStats.weakness);
        if (weaknessSprite != null)
        {
            weaknessSpriteUI.sprite = weaknessSprite;
            weaknessSpriteUI.gameObject.SetActive(true);
        }
        else
        {
            weaknessSpriteUI.gameObject.SetActive(false);
        }
    }
}
