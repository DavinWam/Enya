using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class GearPanel : MonoBehaviour
{
    public Image memberPortraitImage; // Assign this via the inspector
    public TextMeshProUGUI healthText;
    public TextMeshProUGUI manaText;
    public TextMeshProUGUI attackText;
    public TextMeshProUGUI defenseText;
    public TextMeshProUGUI speedText;
    public TextMeshProUGUI critRateText;
    public TextMeshProUGUI blockRateText;
    // Attributes
    public GameData gameData;
    public List<GameObject> gearSlots;
    public List<GameObject> inventoryButtons; // The list of buttons in the UI
    public Slider scrollSlider; // The slider component
    public int selectedGearIndex { get; private set; }//index of selected gear in inventory
    public TextMeshProUGUI equippedText;
    public TextMeshProUGUI InventoryText;
    public TextMeshProUGUI indexDisplayText;
    public GameObject inventoryGearList { get; private set; }
    public GameObject returnButton;
    public GameObject partyButton;
    public PlayerStats currentCharacter;
    public int currentCharIndex = 0;
    public int equippedIndex = -1;//index of equipped buttons
    public int inventoryIndex = -1; //index of inventory button
    private int currentInventoryOffset = -1;
    public Image equippedIcon;
    public TextMeshProUGUI otherEquippedText;
    public bool isPanelOpen = false;
    private Animator rpgPanelAnimator;
    private List<Gear> consolidatedInventory = new List<Gear>();

    private void Start()
    {
        //UpdateInventoryDisplay(0); // Initial update
        scrollSlider.onValueChanged.AddListener(UpdateInventoryDisplay); // Add listener for the slider
    }
    public void ConsolidateInventory()
    {
        for (int i = 0; i < gameData.partyManager.inventory.Count; i++) 
        { 
            if(gameData.partyManager.inventory[i] == null)
            {
                gameData.partyManager.inventory.RemoveAt(i);
                i -= 1;
            }
        }
        consolidatedInventory.Clear();
        consolidatedInventory.AddRange(gameData.partyManager.inventory);

        for (int i = 0; i < gameData.partyManager.cloneStats.Count+1; i++) 
        { 
            PlayerStats stats = gameData.partyManager.GetStatsByIndex(i);
            if( stats != currentCharacter)
            {
                Debug.Log("char:"+i);
                if(stats.equippedGear.Count !=0){

                    consolidatedInventory.AddRange(stats.equippedGear);
                }
                
            }
        }
        scrollSlider.maxValue = Mathf.Max(0,consolidatedInventory.Count-6);
        Debug.Log(consolidatedInventory.Count);
        // Now adjust the handle width

    }

    public void Equip()
    {
        // Ensure there is a selected gear in the inventory
        if (selectedGearIndex < 0 || selectedGearIndex >= consolidatedInventory.Count)
        {
            Debug.Log("No gear selected or invalid index");
            return;
        }
        Debug.Log(selectedGearIndex);
        // Get the selected gear
        Gear gearToEquip = consolidatedInventory[selectedGearIndex];


        //if a character has already equipped the item do nothing 
        if(gearToEquip.equippedStats != null) return;

        // Check if there's an empty slot in the character's equipped gear
        if (currentCharacter.EquipGear(gearToEquip)) // Assuming there's a limit (MAX_EQUIPPED_GEAR)
        {
            equippedIndex = currentCharacter.equippedGear.Count-1;
            // Remove the gear from the party inventory
            gameData.partyManager.inventory.Remove(gearToEquip);

            // Update the UI
            
            UpdateGearPanel();
            selectedGearIndex = Mathf.Max(selectedGearIndex-1,0);
            InventoryText.text = consolidatedInventory[selectedGearIndex].gearName+"\n"+consolidatedInventory[selectedGearIndex].gearDescription;

            ConsolidateInventory();
            UpdateInventoryDisplay(scrollSlider.value);

            Debug.Log("Gear equipped: " + gearToEquip.gearName);
        }
        else
        {
            Debug.Log("No empty slot available for equipping gear");
        }
    }
    public void Swap()
    {
        // Ensure there are selected gears in both the inventory and equipped gear
        if (selectedGearIndex < 0 || selectedGearIndex >= consolidatedInventory.Count ||
            equippedIndex < 0 || equippedIndex >= currentCharacter.equippedGear.Count)
        {
            Debug.Log("Invalid selection for swap");
            return;
        }

        // Get the selected gears
        Gear gearFromInventory = consolidatedInventory[selectedGearIndex];
        Gear gearFromEquipped = currentCharacter.equippedGear[equippedIndex];




        //equip other characters item to current character
        currentCharacter.equippedGear[equippedIndex] = gearFromInventory;
        equippedText.text = gearFromInventory.gearName+"/n"+gearFromInventory.gearDescription;
    

        // Check if the gear from the inventory is equipped to another character
        if (gearFromInventory.equippedStats != null)
        {
            // Unequip the gear from the other character
            PlayerStats otherStats = gearFromInventory.equippedStats;

            // equip item from current character to other character
            otherStats.equippedGear[otherStats.equippedGear.IndexOf(gearFromInventory)] = gearFromEquipped;
            //set current holder
            gearFromEquipped.equippedStats = otherStats;
        }else{
            // Update the inventory
            if (gearFromInventory.equippedStats == currentCharacter)
            {
                gameData.partyManager.inventory[selectedGearIndex] = gearFromEquipped;
            }
            else
            {
                gameData.partyManager.inventory.Add(gearFromEquipped);
                gameData.partyManager.inventory.RemoveAt(selectedGearIndex);
            }

            
            gearFromEquipped.equippedStats = null; // Reset equippedStats for the equipped gear
        }
        InventoryText.text = gearFromEquipped.gearName+"/n"+gearFromEquipped.gearDescription;
        
        //set current holder
        gearFromInventory.equippedStats = currentCharacter; // Update equippedStats for the inventory gear

        // Update the UI
        UpdateGearPanel();
        ConsolidateInventory();
        UpdateInventoryDisplay(scrollSlider.value);

        Debug.Log("Gear swapped");
    }

    public void Remove()
    {
        Debug.Log(gameData.partyManager.inventory.Count);
        // Ensure there is a selected gear in the equipped gear
        if (equippedIndex < 0 || equippedIndex >= currentCharacter.equippedGear.Count)
        {
            Debug.Log("No gear selected or invalid index");
            return;
        }

        // Get the selected gear
        Gear gearToUnequip = currentCharacter.equippedGear[equippedIndex];

        // Remove the gear from the character's equipped gear
        currentCharacter.equippedGear.RemoveAt(equippedIndex);
        gearToUnequip.equippedStats = null; // Reset the equippedStats

        // Add the gear back to the party inventory
        gameData.partyManager.inventory.Add(gearToUnequip);

        // Update the UI
        equippedIndex = currentCharacter.equippedGear.Count-1;
        UpdateGearPanel();

        InventoryText.text = gearToUnequip.gearName+"\n"+gearToUnequip.gearDescription;
        selectedGearIndex = gameData.partyManager.inventory.Count-1;
        ConsolidateInventory();
        UpdateInventoryDisplay(scrollSlider.value);

        Debug.Log("Gear unequipped: " + gearToUnequip.gearName);
        Debug.Log(selectedGearIndex);
    }



    public void UpdateInventoryDisplay(float sliderValue)
    {

        int inventorySize = consolidatedInventory.Count;

        // Calculate the starting index based on the slider value
        currentInventoryOffset = Mathf.FloorToInt(sliderValue);

        // Update each button
        for (int i = 0; i < inventoryButtons.Count; i++)
        {
            int inventoryIndex = currentInventoryOffset + i;
            GameObject gearSlot = inventoryButtons[i];
            TextMeshProUGUI nameText = gearSlot.transform.Find("Name").GetComponent<TextMeshProUGUI>();
            TextMeshProUGUI classText = gearSlot.transform.Find("Class").GetComponent<TextMeshProUGUI>();

            if (inventoryIndex < inventorySize)
            {
                Gear gear = consolidatedInventory[inventoryIndex];
                nameText.text = gear.gearName;
                classText.text = GetClassText(gear);
                gearSlot.GetComponent<Button>().interactable = true;
            }
            else
            {
                nameText.text = "";
                classText.text = "";
                gearSlot.GetComponent<Button>().interactable = false;
            }
        }
        
        UpdateInventoryDisplay();
    }


    public Gear GetCurrentInventoryItem()
    {
        if (selectedGearIndex >= 0 && selectedGearIndex < gameData.partyManager.inventory.Count)
        {
            return gameData.partyManager.inventory[selectedGearIndex];
        }
        return null; // Or handle this case as needed
    }

    public void UpdateInventoryDisplay(){
        if(selectedGearIndex < 0 && consolidatedInventory.Count >0){
            indexDisplayText.text = $"{0}/{consolidatedInventory.Count}";
        }else if(selectedGearIndex >= 0 && consolidatedInventory.Count == 0 ){
         indexDisplayText.text = $"0/{consolidatedInventory.Count}";
        }else{
            indexDisplayText.text = $"{selectedGearIndex+1}/{consolidatedInventory.Count}";
        }
        
    }
    public void OnSelectionInventory(int buttonIndex)
    {
        inventoryIndex = currentInventoryOffset + buttonIndex;
        if (inventoryIndex < consolidatedInventory.Count)
        {
            Gear selectedGear = consolidatedInventory[inventoryIndex];
            InventoryText.text = selectedGear.gearName+"\n"+selectedGear.gearDescription;
            selectedGearIndex = inventoryIndex; // Track the selected inventory index
            UpdateInventoryDisplay();

            // Check if the gear is equipped to a character
            if (selectedGear.equippedStats != null)
            {
                // Fetch the character's sprite and name
                Sprite characterSprite = gameData.partyManager.GetCharacterSprite(selectedGear.equippedStats);
                string characterName = gameData.partyManager.GetCharacterName(selectedGear.equippedStats);

                equippedIcon.sprite = characterSprite;
                equippedIcon.color = Color.white; // Set sprite color to normal
                otherEquippedText.text = characterName;
            }
            else
            {
                // Gear is not equipped, clear the icon and set text to 'None'
                equippedIcon.sprite = null;
                equippedIcon.color = Color.clear; // Make sprite invisible
                otherEquippedText.text = "None";
            }
        }
    }

    public void OnSelectionEquipped(int gearSlot)
    {  
        if(gearSlot > gameData.partyManager.GetStatsByIndex(currentCharIndex).equippedGear.Count-1)
        {
            equippedText.text = "";
            return;
        }
        Debug.Log("selection");
        Gear gear = gameData.partyManager.GetStatsByIndex(currentCharIndex).equippedGear[gearSlot];// Assuming each gear slot has a GearSlot component with a reference to the Gear
        if (gear != null)
        {
            Debug.Log($"selected {gearSlot}");
            equippedIndex = gearSlot;
            equippedText.text = gear.gearName+"\n"+gear.gearDescription; // Update the description text
        }
    }

    public void UpdateGearPanel()
    {
        currentCharacter = gameData.partyManager.GetStatsByIndex(currentCharIndex);
        memberPortraitImage.sprite = gameData.partyManager.GetCharacterSprite(currentCharacter);
        UpdateStatsDisplay(currentCharacter);
        
        for (int i = 0; i < gearSlots.Count; i++)
        {
            GameObject gearSlot = gearSlots[i];

            TextMeshProUGUI nameText = gearSlot.transform.Find("Name").GetComponent<TextMeshProUGUI>();
            TextMeshProUGUI classText = gearSlot.transform.Find("Class").GetComponent<TextMeshProUGUI>();
            if (i < currentCharacter.equippedGear.Count)
            {
                if(equippedIndex == -1){
                    equippedIndex = i;
                }
                Gear gear = currentCharacter.equippedGear[i];
                gearSlot.GetComponent<Button>().interactable = true;
                nameText.text = gear.gearName;
                classText.text = GetClassText(gear);
                
            }else{
                gearSlot.GetComponent<Button>().interactable = false;
                nameText.text = "";
                classText.text = "";
            }

        }

    }
    public void UpdateStatsDisplay(PlayerStats stats)
    {
        if (stats != null)
        {
            healthText.text = $"{stats.GetEffectiveStat(StatType.HEALTH)}";
            manaText.text = $"{stats.GetEffectiveStat(StatType.MANA)}";
            attackText.text = $"{stats.GetEffectiveStat(StatType.ATTACK)}";
            defenseText.text = $"{stats.GetEffectiveStat(StatType.DEFENSE)}";
            speedText.text = $"{stats.GetEffectiveStat(StatType.SPEED)}";
            critRateText.text = $"{(stats.GetEffectiveStat(StatType.CRIT_RATE) * 100).ToString("0.0")}%";
            blockRateText.text = $"{(stats.GetEffectiveStat(StatType.BLOCK_RATE) * 100).ToString("0.0")}%";
        }
    }

    private string GetStatAbbreviation(StatType statType)
    {
        switch (statType)
        {
            case StatType.ATTACK:
                return "ATK";
            case StatType.DEFENSE:
                return "DEF";
            case StatType.HEALTH:
                return "HP";
            case StatType.MANA:
                return "MP";
            case StatType.SPEED:
                return "SPD";
            case StatType.CRIT_RATE:
                return "CRIT";
            case StatType.BLOCK_RATE:
                return "BLCK";
            // Add other cases as needed
            default:
                return "";
        }
    }

    // Helper Method to Generate Class Text
    private string GetClassText(Gear gear)
    {
        string classText = GetStatAbbreviation(gear.statType);
        if (gear.isPercentage)
        {
            classText += "%";
        }

        return classText;
    }
    void SetPartyStatusForMenuChars(string animParameter, bool isInParty)
    {
        // Find all game objects with the tag "MenuChar"
        GameObject[] menuChars = GameObject.FindGameObjectsWithTag("MenuChar");

        foreach (GameObject menuChar in menuChars)
        {
            // Get the Animator component of each game object
            Animator animator = menuChar.GetComponent<Animator>();
            if (animator != null) // Check if Animator component exists
            {
                // Set the inParty parameter
                animator.SetBool(animParameter, isInParty);
            }
        }
    }
    public void Toggle(int newIndex)
    {
        Animator bgSlashAnimator = GameObject.Find("backgroundSlash").GetComponent<Animator>();
        rpgPanelAnimator = GameObject.Find("RPG_Panel").GetComponent<Animator>();
        // Assuming that all MenuChars have similar animation lengths, 
        // we take the length of the first found MenuChar's animation
        GameObject firstMenuChar = GameObject.FindGameObjectWithTag("MenuChar");
        float animationLength = firstMenuChar.GetComponent<Animator>().GetCurrentAnimatorStateInfo(0).length;

        PartyPanel partyPanel = FindObjectOfType<PartyPanel>();
        SpellPanel spellPanel = FindObjectOfType<SpellPanel>();
        if(partyPanel.isPanelOpen){
            partyPanel.Toggle();
        }
        if(spellPanel.isPanelOpen){
            spellPanel.Toggle();
        }
        if (isPanelOpen)
        {
            isPanelOpen = false;

            //shears sidebar
            bgSlashAnimator.SetBool("shear", true);
            bgSlashAnimator.SetBool("inGear", false);
            // Start a coroutine to delay the closing by the animation length

            // //reset label text
            GameObject.Find("panelLabelText").GetComponent<TextMeshProUGUI>().text = "Main";
            // //grows party icons
            SetPartyStatusForMenuChars("inGear",false);

            // //things that should happen at the end of the close elements by animation length
            rpgPanelAnimator.SetBool("inGear",false);
            UnityEngine.EventSystems.EventSystem.current.SetSelectedGameObject(partyButton);

        }
        else
        {
            isPanelOpen = true;
            //unshears side bar
            bgSlashAnimator.SetBool("shear", false);
            bgSlashAnimator.SetBool("inGear", true);


            // //close elements of main menu that don't carry over

            // //shrinks party icons
            SetPartyStatusForMenuChars("inGear",true);

            rpgPanelAnimator.SetBool("inGear",true);
            GameObject.Find("panelLabelText").GetComponent<TextMeshProUGUI>().text = "Gear";
            
            currentCharIndex = newIndex;
            equippedIndex = -1;
            UpdateGearPanel(); // Update the gear panel with the current character's gear

            ConsolidateInventory();
            
            UpdateInventoryDisplay(0);
        }
    }



}
