using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class RPGPanel : MonoBehaviour
{
    [Header("UI Elements")]
    public List<Button> activeMemberUIButtons; // UI buttons for active party members
    public TextMeshProUGUI healCountText; // Text UI for displaying the heal count
    public GameData gameData; // Reference to the game data
    public PartyPanel partyPanel;
    public GearPanel gearPanel;
    public SpellPanel spellPanel;
    public OptionsPanel optionsPanel;
    public GameObject backgroundSlash;
    public TextMeshProUGUI  screenLabel;
    private void OnEnable()
    {
        InitializePanel();
        UpdateHealCount(); // Make sure the heal count is updated when the panel is enabled
    }

    private void Update()
    {
        // Continuously update the bars and the heal count while the panel is active
        UpdatePanel();
        UpdateHealCount();
    }

    public void InitializePanel()
    {
        partyPanel.isPanelOpen = false;
        gearPanel.isPanelOpen = false;
        spellPanel.isPanelOpen = false;
        optionsPanel.isPanelOpen = false;

        backgroundSlash.SetActive(true);
        screenLabel.text = "Main";
        // Update the player character's UI (always active)
        UpdateCharacterUI(activeMemberUIButtons[0], FindObjectOfType<PlayerController>().GetComponent<PlayerCharacter>());
        activeMemberUIButtons[0].interactable = true; // Player character's button is always interactable

        // Populate and set buttons based on active party members
        for (int i = 1; i < activeMemberUIButtons.Count; i++)
        {
            if (i - 1 < gameData.partyManager.activePartyMembersPrefabs.Count)
            {
                GameObject cloneCharacter = gameData.partyManager.activePartyMembersPrefabs[i - 1];
                UpdateCharacterConditionUI(activeMemberUIButtons[i], cloneCharacter);
                activeMemberUIButtons[i].interactable = true;
            }
            else
            {
                // Disable the button if no party member is present
                activeMemberUIButtons[i].interactable = false;
                activeMemberUIButtons[i].transform.Find("Icon").GetComponent<Image>().color = Color.clear;
            }
        }

        backgroundSlash.GetComponent<Animator>().Play("idle");
        foreach(Button characterPortrait in activeMemberUIButtons){
            characterPortrait.gameObject.GetComponent<Animator>().Play("idle");
        }
    }

    private void UpdatePanel()
    {
        int index = 0;
        foreach (Button characterButton in activeMemberUIButtons)
        {
            if (characterButton.gameObject.activeSelf && characterButton.interactable)
            {
                // Assuming the character data is somehow linked to the button (e.g., via name, ID, or data component)
                PlayerCharacter character = characterButton.GetComponentInParent<PlayerCharacter>();
                if (character != null)
                {
                    
                    if(index == 0){
                        UpdateCharacterUI(characterButton, character);
                    }else{
                        
                    }
                }
            }
        }
    }

    private void UpdateCharacterUI(Button characterButton, PlayerCharacter character)
    {
        Image iconImage = characterButton.transform.Find("Icon").GetComponent<Image>();
        iconImage.sprite = character.characterSprite;
        iconImage.color = Color.white;
        iconImage.preserveAspect = true; // Preserve the aspect ratio of the sprite
        float currentHealth = character.characterStats.GetEffectiveStat(StatType.CURRENT_HEALTH);
        float maxHealth = character.characterStats.GetEffectiveStat(StatType.HEALTH);
        float currentMana = character.characterStats.GetEffectiveStat(StatType.CURRENT_MANA);
        float maxMana = character.characterStats.GetEffectiveStat(StatType.MANA);

        // Update health and mana bars as before
        characterButton.transform.Find("HPBar").GetComponent<Slider>().value = currentHealth / maxHealth;
        characterButton.transform.Find("MPBar").GetComponent<Slider>().value = currentMana / maxMana;
    
    }
    private void UpdateCharacterConditionUI(Button characterButton, GameObject clone)
    {
        Image iconImage = characterButton.transform.Find("Icon").GetComponent<Image>();
        iconImage.sprite = clone.GetComponent<PlayerCharacter>().characterSprite;
        iconImage.color = Color.white;
        iconImage.preserveAspect = true; // Preserve the aspect ratio of the sprite

        PlayerStats stats = gameData.partyManager.GetStat(clone);
        // Assuming the health and mana are floats and within 0 to 1 range for slider values
        float currentHealth = stats.GetEffectiveStat(StatType.CURRENT_HEALTH);
        float maxHealth = stats.GetEffectiveStat(StatType.HEALTH);
        float currentMana = stats.GetEffectiveStat(StatType.CURRENT_MANA);
        float maxMana = stats.GetEffectiveStat(StatType.MANA);
        
        // Update health and mana bars as before
        characterButton.transform.Find("HPBar").GetComponent<Slider>().value = currentHealth / maxHealth;
        characterButton.transform.Find("MPBar").GetComponent<Slider>().value = currentMana / maxMana;

    }
    public void heal(){
        gameData.partyManager.UseHealingItem();
        InitializePanel();
    }
    // Call this function to update the heal count text
    public void UpdateHealCount()
    {
        int healCount = gameData.partyManager.currentHealingItemCount;
        healCountText.text = healCount.ToString();
    }

    public void Close()
    {
        if(partyPanel.isPanelOpen)
        {
            partyPanel.Toggle();

        }else if(gearPanel.isPanelOpen)
        {
            gearPanel.Toggle(0);
        }
        else if(optionsPanel.isPanelOpen)
        {
            optionsPanel.Toggle();
        }
        else if(spellPanel.isPanelOpen)
        {
            spellPanel.Toggle();
        }
    }
}
