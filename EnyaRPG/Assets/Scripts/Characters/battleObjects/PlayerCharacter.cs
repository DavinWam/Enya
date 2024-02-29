using System;
using System.Collections;
using System.Collections.Generic;
using JetBrains.Annotations;
using Unity.VisualScripting;
using Unity.VisualScripting.Antlr3.Runtime;
using UnityEngine;
using UnityEngine.Experimental.GlobalIllumination;

public class PlayerCharacter : CharacterBase
{
    private Act currentAction;
    public event Action<Act> OnActionDecided = delegate { };
    private enum DecisionState
    {
        None,
        ChoosingAction,
        ChoosingTargetForAttack,
        ChoosingSpell,
        ChoosingTargetForSpell
    }
    private DecisionState currentState = DecisionState.None;
    private Spell selectedSpell;
    private bool ctrlHeld = false;
    private bool inputLocked = false;
    public GameObject levelUpTextPrefab; // Assign this in the Inspector
    public GameData gameData;
    private void Start(){
        PlayerStats ps = characterStats as PlayerStats;
        ps.CalculateExpToNextLevel();
        if(ps.isClone == false)
        {
            gameData.respawnLocation = transform.position;
            characterStats = ps.Clone();// if the chracter isn't a clone character aka our main characterwe need to make sure their stats are a copy
            gameData.partyManager.playerStats = characterStats as PlayerStats;
        }
        
    }
    private void Update()
    {
        if (inputLocked) return;
        if (Input.GetKey(KeyCode.LeftControl) && ctrlHeld == false)
        {
            ctrlHeld = true;
            Debug.Log("back");
            HandleBackStep();
        }
        if(Input.GetKeyUp(KeyCode.LeftControl)){
            ctrlHeld = false;
        }
        if(characterStats.currentHealth > 0){
            IsAlive = true;
        } 
        else
        {
            IsAlive = false;
        }
    }

    private void HandleBackStep()
    {
        BattleController battleController = FindObjectOfType<BattleController>();

        switch (currentState)
        {
            case DecisionState.ChoosingTargetForAttack:
                battleController.battleUI.ToggleCharacterSelectionUI(false, BattleUI.TargetSelectionContext.Enemies);
                battleController.battleUI.ShowActionMenu(this);
                FindObjectOfType<CameraController>().SwitchToBattleMode();
                battleController.battleUI.UnhighlightAllTurnWheelIcons();
                currentState = DecisionState.ChoosingAction;
                // Lock input for half a second when toggling
                LockInputForDuration(0.1f);
                break;

            case DecisionState.ChoosingTargetForSpell:
                battleController.battleUI.ToggleCharacterSelectionUI(false,BattleUI.TargetSelectionContext.Enemies);
                battleController.battleUI.UnhighlightAllTurnWheelIcons();
                ToggleWings();
                CastSpell();
                // Lock input for half a second when toggling
                LockInputForDuration(0.1f);
                break;

            case DecisionState.ChoosingSpell:
                battleController.battleUI.SpellMenu.SetActive(false);

                ToggleWings();
                FindObjectOfType<CameraController>().SwitchToBattleMode();
                battleController.battleUI.ShowActionMenu(this);
                currentState = DecisionState.ChoosingAction;
                // Lock input for half a second when toggling
                LockInputForDuration(0.1f);
                break;

            default:
                break;
        }
    }

    
    private IEnumerator LockInputCoroutine(float duration)
    {
        BattleUI bu = FindObjectOfType<BattleUI>();
        inputLocked = true;
        bu.inputLocked = true;
        yield return new WaitForSeconds(duration);
        inputLocked = false;
        bu.inputLocked = false;
    }

    public void LockInputForDuration(float duration)
    {
        StartCoroutine(LockInputCoroutine(duration));
    }

    public override Act Act()
    {
        currentState = DecisionState.ChoosingAction;
        selectedSpell = null;
        StartCoroutine(WaitForActionDecision());


        return currentAction;
    }

    private IEnumerator WaitForActionDecision()
    {
        bool actionDecided = false;
        OnActionDecided += action => { actionDecided = true; };
        yield return new WaitUntil(() => actionDecided);
    }

    public override void Attack()
    {
        BattleController battleController = FindObjectOfType<BattleController>();
        if (battleController.battleUI.TabIsHeld) return;
        currentState = DecisionState.ChoosingTargetForAttack;
        
        
        // Ensure the event is empty
        battleController.battleUI.ClearOnTargetSelectedSubscribers();

        battleController.battleUI.ToggleCharacterSelectionUI(true,BattleUI.TargetSelectionContext.Enemies) ;
        battleController.battleUI.HideActionMenu();
        battleController.battleUI.OnTargetSelected += HandleTargetSelection;
    }

    private void HandleTargetSelection(CharacterBase selectedTarget)
    {
        
        if (selectedTarget != null)
        {
            currentState = DecisionState.None;
            currentAction = new Act { actionType = ActionType.ATTACK, target = selectedTarget };
            OnActionDecided(currentAction);
            FindObjectOfType<BattleController>().battleUI.ToggleCharacterSelectionUI(false, BattleUI.TargetSelectionContext.Enemies);
        }
        FindObjectOfType<BattleController>().battleUI.OnTargetSelected -= HandleTargetSelection;
    }

    public void UseMana(float cost)
    {
        // Ensure characterStats is of type PlayerStats and then deduct mana.
        if(characterStats is PlayerStats playerStatsInstance)
        {
            playerStatsInstance.currentMana -= cost;
            if (playerStatsInstance.currentMana < 0)
            {
                playerStatsInstance.currentMana = 0;
            }
        }
    }
    public override void CastSpell()
    {
        BattleController battleController = FindObjectOfType<BattleController>();
        if (battleController.battleUI.TabIsHeld) return;
        PlayerStats ps = characterStats as PlayerStats;
        if(ps.spellList.Count == 0)
        {
            return;
        }
        currentState = DecisionState.ChoosingSpell;
        
        
        // Show the spell menu
        battleController.battleUI.EnableSpellTab();
        battleController.battleUI.HideActionMenu();
        FindObjectOfType<CameraController>().FocusOnActiveCharacter(this);
        //bring up wings of fire
        ToggleWings();
        
        // Ensure the event is empty
        battleController.battleUI.ClearOnTargetSelectedSubscribers();

        // Subscribe to the event when a spell is selected
        battleController.battleUI.OnSpellSelected += HandleSpellSelection;
    }

    public void ToggleWings(){
        GameObject wings = this.GameObject().GetComponent<Transform>().GetChild(2).gameObject;
        if(wings.activeSelf){
            wings.SetActive(false);
        }else{
            wings.SetActive(true);
            wings.GetComponentInChildren<MeshRenderer>().material.SetColor("_Color",GetFireTypeColor(4f));
            wings.GetComponentInChildren<MeshRenderer>().material.SetColor("_FresnelColor", GetComplementaryColor(GetFireTypeColor(12f)));
            //sets light to a paler version of the flame color
            float saturation = 0f;
            float hue = 0f;
            float value = 0f;
            Color.RGBToHSV(GetFireTypeColor(),out hue,out saturation,out value);
            saturation /= 2f;

            wings.GetComponentInChildren<Light>().color = Color.HSVToRGB(hue, saturation, value);
        }
    }
    private void HandleSpellSelection(int selectedIndex)
    {
        Debug.Log($"picked spell: {selectedIndex}");
        PlayerStats playerStatsInstance = characterStats as PlayerStats;
        if(playerStatsInstance == null){
            Debug.LogError($"cant link to stats on{characterName}");
        }
        selectedSpell = playerStatsInstance.spellList[selectedIndex];

        if (selectedSpell.manaCost > playerStatsInstance.currentMana)
        {
            Debug.Log("not enough mana");
            return;
        }
        
        FindObjectOfType<CameraController>().SwitchToBattleMode();

        BattleController battleController = FindObjectOfType<BattleController>();
        // If the selected spell doesn't need you to select a target
        EffectType effect = selectedSpell.effectType;
        if(effect == EffectType.AOE_DAMAGE|| effect == EffectType.AOE_HEAL || effect == EffectType.AOE_BUFF){
            currentState = DecisionState.None;
            currentAction = new Act { actionType = ActionType.SPELL, target = null, spell = selectedSpell }; // Set target to null for AOE
            
            UseMana(selectedSpell.manaCost);
            battleController.battleUI.SpellMenu.SetActive(false);
            battleController.battleUI.OnSpellSelected -= HandleSpellSelection;
            OnActionDecided(currentAction);

            // Return early as we don't need to go to target selection
            return;
        }

        //spells that require selection 

        currentState = DecisionState.ChoosingTargetForSpell;

        // Unsubscribe from the target selection to ensure it's not double-subscribed
        battleController.battleUI.OnTargetSelected -= HandleTargetSelection;
        battleController.battleUI.SpellMenu.SetActive(false);
        // Check if the spell targets allies or enemies
        if (effect == EffectType.HEAL || effect == EffectType.BUFF)
        {
            Debug.Log("choosing heal");
            // For healing spells, select from allies
            battleController.battleUI.ToggleCharacterSelectionUI(true, BattleUI.TargetSelectionContext.Allies);

        }
        else
        {
            // For other spells, select from enemies
            battleController.battleUI.ToggleCharacterSelectionUI(true, BattleUI.TargetSelectionContext.Enemies);
        }
        battleController.battleUI.HideActionMenu();

        // Lambda for spell target selection
        battleController.battleUI.OnTargetSelected += (target) =>
        {
            currentState = DecisionState.None;
            if (target != null)
            {
                currentAction = new Act { actionType = ActionType.SPELL, target = target, spell = selectedSpell };
                OnActionDecided(currentAction);
                battleController.battleUI.ToggleCharacterSelectionUI(false,BattleUI.TargetSelectionContext.Enemies);
                UseMana(selectedSpell.manaCost);
            }
            // Unsubscribe the lambda after using it to ensure we don't have lingering subscriptions
            battleController.battleUI.OnTargetSelected -= HandleTargetSelection;
        };

        // Unsubscribe from the spell selection now that we've handled it
        battleController.battleUI.OnSpellSelected -= HandleSpellSelection;
    }

    public Spell GetSelectedSpell(){
        return selectedSpell;
    }

public void UseItem()
{
    BattleController battleController = FindObjectOfType<BattleController>();
    if (battleController.battleUI.TabIsHeld) return;
    // Access the singleton instance of the PartyManager (adjust this line based on your implementation)
    PartyManager partyManager = battleController.partyManager;
    
    // Check if a healing item is available and use it
    if (partyManager.CanUseHealingItem())
    {
        // Iterate through each active party member and heal them
        foreach (var member in battleController.playerParty)
        {
            CharacterBase partyMember = member.GetComponent<CharacterBase>();
            if (partyMember != null && partyMember.IsAlive)
            {
                float healAmount = partyMember.characterStats.GetEffectiveStat(StatType.HEALTH) * 0.3f; // 30% of max health
                partyMember.characterStats.Heal(healAmount);
            }
        }

        // Update the UI to reflect the new item count
        battleController.battleUI.UpdateItemCount();

        // Notify the current Act that the action has been decided
        OnActionDecided(new Act { actionType = ActionType.USE_ITEM });

        // End the turn if necessary here
    }
    else
    {
        Debug.Log("No healing items left.");
        // If no items left, you might want to provide feedback to the player or allow them to choose a different action
    }
}



    public void Ignite()
    {
        BattleController bc = FindObjectOfType<BattleController>();
        if (bc.battleUI.TabIsHeld) return;
        StartCoroutine(bc.battleUI.LockInputCoroutine(.5f));
        bc.ConsumeFuel();
        Animator charAnimator = this.GameObject().GetComponentInChildren<Animator>();
        charAnimator.SetInteger("igniteType", (int)bc.usedFuel);
        charAnimator.SetTrigger("startIgnite");
    }
    public override void TakeDamage(float damage,bool isCritical,bool isWeak,bool isBlock)
    {
        characterStats.currentHealth -= damage;
        if(characterStats.currentHealth <= 0)
        {
            characterStats.currentHealth = 0;
            IsAlive = false;
        }
        Debug.Log("playey took "+ damage+" damage.");
        Debug.Log("player health: "+characterStats.currentHealth);
        FindObjectOfType<BattleController>().DealDamage(isCritical,isWeak,isBlock,this.transform.position, (int)(damage+.5f));
        if (characterStats.currentHealth <= 0)
        {
            base.Die();
        }
    }

    public Color GetComplementaryColor(Color original)
    {
        float hue, saturation, value;
        Color.RGBToHSV(original, out hue, out saturation, out value);

        // Adjust the hue to get the complementary color.
        hue += 0.35f;
        if (hue > 1f) hue -= 1f;

        return Color.HSVToRGB(hue, saturation, value);
    }

    public Color GetFireTypeColor(float desiredIntensity = 1f)
    {
        Color rawColor;

        switch(((PlayerStats)characterStats).fireType)
        {
            case FireType.STORM:
                rawColor = Color.red;
                break;
            case FireType.LIGHTNING:
                rawColor = new Color(148f/255f, 0f/255f, 211f/255f);  // purple
                break;
            case FireType.SUN:
                rawColor = Color.yellow;
                break;
            case FireType.RAIN:
                rawColor = Color.cyan;
                break;
            case FireType.SNOW:
                rawColor = Color.white;
                break;
            case FireType.CLOUD:
                rawColor = Color.green;
                break;
            case FireType.SKY:
                rawColor = new Color(255f/255f, 165f/255f, 0f/255f);  // orange       
                break;
            default:
                rawColor = Color.black;
                break;
        }

        // Calculate the average intensity of the raw color.
        float currentIntensity = (rawColor.r + rawColor.g + rawColor.b) / 3f;

        // Compute the factor to normalize the color's intensity.
        float factor = desiredIntensity / currentIntensity;

        // Apply the factor to adjust the intensity.
        return new Color(rawColor.r * factor, rawColor.g * factor, rawColor.b * factor, rawColor.a);
    }
    public PlayerStats GetPlayerStats(){
        return characterStats as PlayerStats;
    }
    public List<Gear> GetGear(){
        return GetPlayerStats().equippedGear;
    }
}


// You might also want to define the Item class, BattleManager, UIManager, and other dependencies or reference them from elsewhere in your codebase.
