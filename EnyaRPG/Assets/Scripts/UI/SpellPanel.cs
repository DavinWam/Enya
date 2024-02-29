using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System.Collections.Generic;

public class SpellPanel : MonoBehaviour
{
    public Image memberPortraitImage; // Assign this via the inspector
    public TextMeshProUGUI nameText;
    public TextMeshProUGUI levelText;
    public TextMeshProUGUI healthText;
    public TextMeshProUGUI manaText;
    public TextMeshProUGUI attackText;
    public TextMeshProUGUI defenseText;
    public TextMeshProUGUI speedText;
    public TextMeshProUGUI critRateText;
    public TextMeshProUGUI blockRateText;
    public List<Button> SpellRPGMenuButtons;

    
    public TextMeshProUGUI effectTypeText;
    public TextMeshProUGUI basePowerText;
    public TextMeshProUGUI manaCostText;
    public TextMeshProUGUI numHitsText;
    public TextMeshProUGUI descriptionText;
    public TextMeshProUGUI statusEffectDescriptionText;

    PlayerStats currentCharacter;
    public GameData gameData;
    public bool isPanelOpen = false;
    private Animator rpgPanelAnimator;
    private Animator bgSlashAnimator;

    [Header("UI References")]
    public TextMeshProUGUI panelLabelText; // Assign in Inspector
    // Add other relevant UI references and methods as needed

    // Start method (if needed)
    void Start()
    {
        bgSlashAnimator = GameObject.Find("backgroundSlash").GetComponent<Animator>();
        rpgPanelAnimator = FindObjectOfType<RPGPanel>().GetComponent<Animator>();
        // Initialize the panel state if necessary
    }

    public void Toggle()
    {

        PartyPanel partyPanel = FindObjectOfType<PartyPanel>();
        if(partyPanel.isPanelOpen){
            partyPanel.Toggle();
        }
        if (isPanelOpen)
        {
            ClosePanel();
        }
        else
        {
            OpenPanel();
        }
    }
    public void UpdateStatsDisplay(PlayerStats stats)
    {
        if (stats != null)
        {
            nameText.text = gameData.partyManager.GetCharacterName(stats);
            levelText.text = $"{stats.level}";
            healthText.text = $"{stats.GetEffectiveStat(StatType.HEALTH)}";
            manaText.text = $"{stats.GetEffectiveStat(StatType.MANA)}";
            attackText.text = $"{stats.GetEffectiveStat(StatType.ATTACK)}";
            defenseText.text = $"{stats.GetEffectiveStat(StatType.DEFENSE)}";
            speedText.text = $"{stats.GetEffectiveStat(StatType.SPEED)}";
            critRateText.text = $"{(stats.GetEffectiveStat(StatType.CRIT_RATE) * 100).ToString("0.0")}%";
            blockRateText.text = $"{(stats.GetEffectiveStat(StatType.BLOCK_RATE) * 100).ToString("0.0")}%";
        }
    }
    private void OpenPanel()
    {
        RPGPanel rPGPanel = FindObjectOfType<RPGPanel>();
        PartyPanel partyPanel = FindObjectOfType<PartyPanel>();
        isPanelOpen = true;

        // Adjust sidebar and other UI elements for opening
        bgSlashAnimator.SetBool("shear", false);
        bgSlashAnimator.SetBool("inGear", false);
        //shears sidebar


        
        rpgPanelAnimator.SetBool("inSpell", true);
        rpgPanelAnimator.SetBool("inParty", false);

        //shrinks party icons
        partyPanel.SetPartyStatusForMenuChars("inParty", true);
        // Update label text
        panelLabelText.text = "Spells";

        // Additional logic to initialize or update spell panel contents
        InitializePanel(0); // If you have an initialization method
    }

    private void ClosePanel()
    {
        RPGPanel rPGPanel = FindObjectOfType<RPGPanel>();
        PartyPanel partyPanel = FindObjectOfType<PartyPanel>();

        isPanelOpen = false;

        // Adjust sidebar and other UI elements for closing
        bgSlashAnimator.SetBool("shear", true);

        //shrinks party icons
        partyPanel.SetPartyStatusForMenuChars("inParty", false);
        rpgPanelAnimator.SetBool("inSpell", false);

        // Reset label text
        panelLabelText.text = "Main";
        partyPanel.ResetPartyMemberPositions();
        rPGPanel.InitializePanel();
        // Additional logic to handle closing the panel
    }

    // Additional methods and logic for the spell panel
    public void InitializePanel(int index)
    {
        FindObjectOfType<PartyPanel>().InitializePanel();
                    // Update the UI with the member's information
            PlayerCharacter memberCharacter = null;

            // Check if the index is for the player character
            if (index == 0)
            {
                memberCharacter = FindObjectOfType<PlayerCharacter>();
                currentCharacter = memberCharacter.GetPlayerStats();
            }
            else
            {
                // Adjust the index to account for the player character at index 0
                int adjustedIndex = index - 1;

                // Combine active and inactive member lists
                List<GameObject> allPartyMembers = new List<GameObject>(gameData.partyManager.activePartyMembersPrefabs);
                allPartyMembers.AddRange(gameData.partyManager.inactivePartyMembersPrefabs);

                currentCharacter = gameData.partyManager.GetStat(allPartyMembers[adjustedIndex]);
            }


        PopulateSpellButtons(currentCharacter);
        PopulateSpellDetails(0);
        memberPortraitImage.sprite = gameData.partyManager.GetCharacterSprite(currentCharacter);
        UpdateStatsDisplay(currentCharacter);
    }

    public void PopulateSpellButtons(PlayerStats currentCharacterStats)
    {
        for (int i = 0; i < SpellRPGMenuButtons.Count; i++)
        {
            if (i < currentCharacterStats.spellList.Count)
            {
                Spell spell = currentCharacterStats.spellList[i];

                // Update the name text
                TextMeshProUGUI nameText = SpellRPGMenuButtons[i].transform.Find("Name").GetComponent<TextMeshProUGUI>();
                nameText.text = spell.spellName;

                // Update the fire icon
                Image fireIcon = SpellRPGMenuButtons[i].transform.Find("FireIcon").GetComponent<Image>();
                fireIcon.sprite = FindObjectOfType<UIManager>().GetSpriteForFireType(spell.fireType);
                fireIcon.color = Color.white;

                // Enable the button
                SpellRPGMenuButtons[i].gameObject.SetActive(true);
            }
            else
            {
                // Update the name text
                TextMeshProUGUI nameText = SpellRPGMenuButtons[i].transform.Find("Name").GetComponent<TextMeshProUGUI>();
                nameText.text = "";

                // Update the fire icon
                Image fireIcon = SpellRPGMenuButtons[i].transform.Find("FireIcon").GetComponent<Image>();
                fireIcon.color = Color.clear;

                // Disable the button if no spell
                SpellRPGMenuButtons[i].gameObject.SetActive(false);
            }
        }
    }
    public void PopulateSpellDetails(int index)
    {
        if (index < 0 || index >= currentCharacter.spellList.Count || currentCharacter.spellList.Count == 0)
        {
            // Invalid index, clear text or show default message
            ClearSpellDetails();
            return;
        }

        Spell selectedSpell = currentCharacter.spellList[index];

        // Update UI elements with spell details
        effectTypeText.text = EffectTypeToString(selectedSpell.effectType);
        descriptionText.text = selectedSpell.description;
        manaCostText.text = $"{selectedSpell.manaCost}";
        basePowerText.text = $"{selectedSpell.basePower*100}%";
        numHitsText.text = $"{selectedSpell.numHits}";

        // Construct status effect description
        string statusEffectDesc = "";

        if (selectedSpell.applySelf != null)
        {
            statusEffectDesc += $"Buff: {selectedSpell.applySelf.label} - {selectedSpell.applySelf.GetDescription()}\n";
        }
        if (selectedSpell.applyTarget != null)
        {
            statusEffectDesc += $"Debuff: {selectedSpell.applyTarget.label} - {selectedSpell.applyTarget.GetDescription()}\n";
        }

        // Include preDamageConditions' StatusEffect
        if (selectedSpell.preDamageConditions?.statusEffect != null)
        {
            string type = selectedSpell.preDamageConditions.statusEffect is Buff ? "Buff" : "Debuff";
            statusEffectDesc += $"{type}: {selectedSpell.preDamageConditions.statusEffect.label} - {selectedSpell.preDamageConditions.statusEffect.GetDescription()}\n";
        }

        // Include postDamageConditions' StatusEffect
        if (selectedSpell.postDamageConditions?.statusEffect != null)
        {
            string type = selectedSpell.postDamageConditions.statusEffect is Buff ? "Buff" : "Debuff";
            statusEffectDesc += $"{type}: {selectedSpell.postDamageConditions.statusEffect.label} - {selectedSpell.postDamageConditions.statusEffect.GetDescription()}\n";
        }

        statusEffectDescriptionText.text = statusEffectDesc;
    }
    public string EffectTypeToString(EffectType effectType)
    {
        switch (effectType)
        {
            case EffectType.HEAL:
                return "Heal";
            case EffectType.AOE_HEAL:
                return "Aoe Heal";
            case EffectType.ADJACENT_AOE_DAMAGE:
                return "Adjacent Hit";
            case EffectType.SINGLE_TARGET_DAMAGE:
                return "Single Hit";
            case EffectType.MULTIHIT_TARGET_DAMAGE:
                return "Multihit";
            case EffectType.AOE_DAMAGE:
                return "Aoe Hit";
            case EffectType.BUFF:
                return "Buff";
            case EffectType.DEBUFF:
                return "Debuff";
            case EffectType.AOE_BUFF:
                return "Aoe Buff";
            default:
                return "Unknown";
        }
    }

    private void ClearSpellDetails()
    {
        descriptionText.text = "";
        manaCostText.text = "";
        basePowerText.text = "";
        numHitsText.text = "";
        statusEffectDescriptionText.text = "";
    }
    // Other methods as needed...
}
