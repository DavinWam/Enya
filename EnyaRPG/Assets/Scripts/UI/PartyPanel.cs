using System.Collections.Generic;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Unity.VisualScripting;
public class PartyPanel : MonoBehaviour
{
    public bool isPanelOpen = false;
    private Animator rpgPanelAnimator;
    [Header("Active Member UI Elements")]
    public List<RectTransform> activeMemberUIElements; // Assign the RectTransforms of the active member UI in the inspector
    int currentMember = 0;

    [Header("Inactive Member UI Elements")]
    public List<GameObject> inactiveMemberUIElements; // Assign the RectTransforms of the inactive member UI in the inspector
    public GameData gameData;
    public PartyManager partyManager;
    // Add these two new lists to store the original positions
    private List<Vector2> originalPositionsActive;
    private List<Vector2> originalPositionsInactive;

    [Header("UI References")]
    public TextMeshProUGUI memberNameText; // Assign this via the inspector
    public Image memberPortraitImage; // Assign this via the inspector
    public TextMeshProUGUI levelText;
    public TextMeshProUGUI nextLevelText;
    public TextMeshProUGUI expText;
    public TextMeshProUGUI hpText;
    public TextMeshProUGUI mpText;
    public TextMeshProUGUI atkText;
    public TextMeshProUGUI defText;
    public TextMeshProUGUI spdText;
    public TextMeshProUGUI critRateText;
    public TextMeshProUGUI blockRateText;
    public Slider healthRawSlider;
    public Slider manaRawSlider;
    public Slider attackRawSlider;
    public Slider defenseRawSlider;
    public Slider speedRawSlider;
    public Slider critRateRawSlider;
    public Slider blockRateRawSlider;
    public Slider healthAdjustedSlider;
    public Slider manaAdjustedSlider;
    public Slider attackAdjustedSlider;
    public Slider defenseAdjustedSlider;
    public Slider speedAdjustedSlider;
    public Slider critRateAdjustedSlider;
    public Slider blockRateAdjustedSlider;

    [Header("Growth Rate Text References")]
    public TextMeshProUGUI healthGrowthText;
    public TextMeshProUGUI manaGrowthText;
    public TextMeshProUGUI attackGrowthText;
    public TextMeshProUGUI defenseGrowthText;
    public TextMeshProUGUI speedGrowthText;
    public TextMeshProUGUI critRateGrowthText;
    public TextMeshProUGUI blockRateGrowthText;
    public UIManager uiManager; // Assign this via the inspector
    public Image fireTypeIconImage; // Assign this via the inspector
    [Header("Fire Type Selection Menu")]
    public GameObject changeFireTypeButton;
    public GameObject fireTypeSelectionMenu; // Assign this via the inspector

    // Method to open the fire type selection menu// Method to open the fire type selection menu
    public void ShowFireTypeSelectionMenu()
    {
        if(fireTypeSelectionMenu.activeSelf){
            fireTypeSelectionMenu.SetActive(false);
        }else{
            if (currentMember == 0) // Only show for the player character
            {
                fireTypeSelectionMenu.SetActive(true);

                // Enable buttons based on unlocked fire types
                // Assuming you have an array or list of buttons in the same order as the FireType enum
                Button[] fireTypeButtons = fireTypeSelectionMenu.GetComponentsInChildren<Button>(true);
                for (int i = 0; i < fireTypeButtons.Length; i++)
                {
                    FireType type = (FireType)i;
                    fireTypeButtons[i].gameObject.SetActive(gameData.unlockedFireTypes.Contains(type));
                }
            }
        }

    }
    // This method is called by fire type buttons
    public void OnFireTypeSelected(int fireTypeIndex)
    {

        // Convert the integer index back to the FireType enum
        FireType selectedType = (FireType)fireTypeIndex;

        // Check if the selected type is unlocked before proceeding
        if (gameData.unlockedFireTypes.Contains(selectedType))
        {
            ChangeFireType(selectedType);
        }
    }

    // This method does the actual fire type change
    private void ChangeFireType(FireType newType)
    {
        PlayerStats playerStats = FindObjectOfType<PlayerCharacter>().characterStats as PlayerStats;
        if (playerStats != null)
        {
            playerStats.fireType = newType;
            // Update spells and any other relevant information
            playerStats.spellList = gameData.GetSpellsForLevelUp(playerStats.level, playerStats.fireType);


            //update UI
            UpdateMemberInfo(0);
            // Close the fire type selection menu
            fireTypeSelectionMenu.SetActive(false);
        }
    }



    void Start()
    {
        // Initialize the lists and store the original positions of the UI elements
        originalPositionsActive = new List<Vector2>();
        originalPositionsInactive = new List<Vector2>();

        foreach (var uiElement in activeMemberUIElements)
        {
            originalPositionsActive.Add(uiElement.anchoredPosition);
        }

        foreach (GameObject uiElement in inactiveMemberUIElements)
        {
            originalPositionsInactive.Add(uiElement.GetComponent<RectTransform>().anchoredPosition);
        }
    }
    // Method called by button OnClick event
    public void UpdateMemberInfo(int index)
    {
        // If the panel isn't open, open it.
        GearPanel gearPanel = FindObjectOfType<GearPanel>();
        SpellPanel spellPanel = FindObjectOfType<SpellPanel>();
        if(gearPanel.isPanelOpen == false && spellPanel.isPanelOpen == false){
            if (!isPanelOpen )
            {
                Toggle();
            }
            currentMember = index;
            // Update the UI with the member's information
            PlayerCharacter memberCharacter = null;
            PlayerStats memberStats = null; // Assuming PlayerStats is derived from PlayerCharacter
            

    // Check if the index is for the player character
            if (index == 0)
            {
                memberCharacter = FindObjectOfType<PlayerCharacter>();
                // Assuming memberCharacter has a PlayerStats component for stats
                memberStats = memberCharacter.characterStats as PlayerStats;
            }
            else
            {
                // Adjust the index to account for the player character at index 0
                int adjustedIndex = index - 1;

                // Combine active and inactive member lists
                List<GameObject> allPartyMembers = new List<GameObject>(partyManager.activePartyMembersPrefabs);
                allPartyMembers.AddRange(partyManager.inactivePartyMembersPrefabs);

                if (adjustedIndex >= 0 && adjustedIndex < allPartyMembers.Count)
                {
                    GameObject memberPrefab = allPartyMembers[adjustedIndex];
                    // Instantiate the prefab temporarily to access its PlayerCharacter component
                    GameObject tempPrefab = Instantiate(memberPrefab);
                    memberCharacter = tempPrefab.GetComponent<PlayerCharacter>();
                    // Destroy the temporary prefab immediately after fetching the data
                    Destroy(tempPrefab);
                    memberStats = partyManager.GetStat(memberPrefab);
                    Debug.Log("health2: "+memberStats.currentHealth);
                     Debug.Log("health3: "+memberStats.GetEffectiveStat(StatType.CURRENT_HEALTH));
                }else{
                    return;
                }
            }

            // Update the text field with the member's name
            memberNameText.text = memberCharacter != null ? memberCharacter.characterName : "Name not found";




            Sprite memberSprite = gameData.partyManager.GetCharacterSprite(memberStats);
            // Update the portrait image with the member's sprite
            if (memberSprite != null)
            {
//                Debug.Log("image");
                memberPortraitImage.sprite = memberSprite;
                memberPortraitImage.preserveAspect = true; // This will preserve the sprite's aspect ratio
            }
            else
            {
                // Handle the case where the sprite is not found, maybe set a default sprite
                memberPortraitImage.color = Color.clear;
            }

            if (memberStats != null)
            {
                float currentHealth = memberStats.GetEffectiveStat(StatType.CURRENT_HEALTH);
                float maxHealth = memberStats.GetEffectiveStat(StatType.HEALTH);
                float currentMana = memberStats.GetEffectiveStat(StatType.CURRENT_MANA);
                float maxMana = memberStats.GetEffectiveStat(StatType.MANA);
                
                Color fireTypeColor = memberCharacter.GetFireTypeColor(1.5f); // Get the color based on the fire type

                fireTypeIconImage.sprite = uiManager.GetSpriteForFireType(memberStats.fireType);
                levelText.text = $"{memberStats.level}";
                nextLevelText.text = $"{memberStats.CalculateExpToNextLevel()}";
                expText.text = $"{memberStats.exp}";


                hpText.text = $"{memberStats.GetEffectiveStat(StatType.CURRENT_HEALTH)}/{memberStats.GetEffectiveStat(StatType.HEALTH)}";

                mpText.text = $"{memberStats.GetEffectiveStat(StatType.CURRENT_MANA)}/{memberStats.GetEffectiveStat(StatType.MANA)}";
                atkText.text = $"{memberStats.GetEffectiveStat(StatType.ATTACK)}";
                defText.text = $"{memberStats.GetEffectiveStat(StatType.DEFENSE)}";
                spdText.text = $"{memberStats.GetEffectiveStat(StatType.SPEED)}";
                // Assuming GetEffectiveStat returns a value between 0 and 1 for rates
                critRateText.text = $"{(memberStats.GetEffectiveStat(StatType.CRIT_RATE) * 100).ToString("0.00")}%";
                blockRateText.text = $"{(memberStats.GetEffectiveStat(StatType.BLOCK_RATE) * 100).ToString("0.00")}%";


                Slider hpBar = healthRawSlider.transform.Find("HPBar").GetComponent<Slider>();
                Slider mpBar = manaRawSlider.transform.Find("MPBar").GetComponent<Slider>();

                
                hpBar.value = currentHealth / maxHealth;
                hpText.text = ((int)currentHealth).ToString()+"/"+maxHealth;
                mpBar.value = currentMana / maxMana;
                mpText.text = ((int)currentMana).ToString()+"/"+maxMana;
                // Update sliders for raw and scaled stats
                healthRawSlider.maxValue = memberStats.GetEffectiveStat(StatType.HEALTH);
                healthRawSlider.value = memberStats.GetRawStatValue(StatType.HEALTH);

                manaRawSlider.maxValue = memberStats.GetEffectiveStat(StatType.MANA);
                manaRawSlider.value = memberStats.GetRawStatValue(StatType.MANA);

                attackRawSlider.maxValue = memberStats.GetEffectiveStat(StatType.ATTACK);
                attackRawSlider.value = memberStats.GetRawStatValue(StatType.ATTACK);

                defenseRawSlider.maxValue = memberStats.GetEffectiveStat(StatType.DEFENSE);
                defenseRawSlider.value = memberStats.GetRawStatValue(StatType.DEFENSE);

                speedRawSlider.maxValue = memberStats.GetEffectiveStat(StatType.SPEED);
                speedRawSlider.value = memberStats.GetRawStatValue(StatType.SPEED);

                critRateRawSlider.maxValue = memberStats.GetEffectiveStat(StatType.CRIT_RATE);
                critRateRawSlider.value = memberStats.GetRawStatValue(StatType.CRIT_RATE);

                blockRateRawSlider.maxValue = memberStats.GetEffectiveStat(StatType.BLOCK_RATE);
                blockRateRawSlider.value = memberStats.GetRawStatValue(StatType.BLOCK_RATE);


                if (!memberStats.isClone)
                {
                    changeFireTypeButton.SetActive(true);

                    healthAdjustedSlider.fillRect.GetComponent<Image>().material.color = fireTypeColor;
                    healthAdjustedSlider.maxValue = memberStats.GetEffectiveStat(StatType.HEALTH);
                    healthAdjustedSlider.value = (memberStats).GetAdjustedStat(StatType.HEALTH);


                    manaAdjustedSlider.maxValue = memberStats.GetEffectiveStat(StatType.MANA);
                    manaAdjustedSlider.value = (memberStats).GetAdjustedStat(StatType.MANA);


                    attackAdjustedSlider.maxValue = memberStats.GetEffectiveStat(StatType.ATTACK);
                    attackAdjustedSlider.value = (memberStats).GetAdjustedStat(StatType.ATTACK);


                    defenseAdjustedSlider.maxValue = memberStats.GetEffectiveStat(StatType.DEFENSE);
                    defenseAdjustedSlider.value = (memberStats).GetAdjustedStat(StatType.DEFENSE);


                    speedAdjustedSlider.maxValue = memberStats.GetEffectiveStat(StatType.SPEED);
                    speedAdjustedSlider.value = (memberStats).GetAdjustedStat(StatType.SPEED);

                    critRateAdjustedSlider.maxValue = memberStats.GetEffectiveStat(StatType.CRIT_RATE);
                    critRateAdjustedSlider.value = (memberStats).GetAdjustedStat(StatType.CRIT_RATE);

                    blockRateAdjustedSlider.maxValue = memberStats.GetEffectiveStat(StatType.BLOCK_RATE);
                    blockRateAdjustedSlider.value = (memberStats).GetAdjustedStat(StatType.BLOCK_RATE);

                }
                else
                {
                    fireTypeSelectionMenu.SetActive(false);
                    changeFireTypeButton.SetActive(false);
                    // For clone characters, set the adjusted sliders' value to 0
                    healthAdjustedSlider.value = 0;
                    manaAdjustedSlider.value = 0;
                    attackAdjustedSlider.value = 0;
                    defenseAdjustedSlider.value = 0;
                    speedAdjustedSlider.value = 0;
                    critRateAdjustedSlider.value = 0;
                    blockRateAdjustedSlider.value = 0;
                }

                // Update growth rate texts
                healthGrowthText.text = memberStats.growths.healthGrowthCategory.ToString();
                manaGrowthText.text = memberStats.growths.manaGrowthCategory.ToString();
                attackGrowthText.text = memberStats.growths.attackGrowthCategory.ToString();
                defenseGrowthText.text = memberStats.growths.defenseGrowthCategory.ToString();
                speedGrowthText.text = memberStats.growths.speedGrowthCategory.ToString();
                critRateGrowthText.text = memberStats.growths.critGrowthCategory.ToString();
                blockRateGrowthText.text = memberStats.growths.blockGrowthCategory.ToString();
            }
            else
            {
                Debug.LogError("PlayerStats component not found on the member character prefab.");
            }
            if (index != 0)
            {
                Destroy(memberCharacter);
            }
        }else if(gearPanel.isPanelOpen == true){
            gearPanel.currentCharIndex = index;
            gearPanel.equippedText.text = "";
            gearPanel.UpdateGearPanel();
            gearPanel.ConsolidateInventory();
            gearPanel.UpdateInventoryDisplay(gearPanel.scrollSlider.value);


        }else{
            spellPanel.InitializePanel(index);
            spellPanel.PopulateSpellDetails(0);
        }


    }
    public void InitializePanel()
    {
        // Merge active and inactive member UI elements into a single list
        List<RectTransform> allMemberUIElements = new List<RectTransform>(activeMemberUIElements);
       
       //UI element that we're alligning the character portraits with
        transform.Find("ActiveTag").GameObject().SetActive(true);
        
        //set actives 
        foreach(GameObject inactiveMemberUIElement in inactiveMemberUIElements)
        {   
            inactiveMemberUIElement.SetActive(true);
            allMemberUIElements.Add(inactiveMemberUIElement.GetComponent<RectTransform>());
        }

        // Handle the player character separately
        PlayerCharacter playerCharacter = FindObjectOfType<PlayerCharacter>();
        if (playerCharacter != null)
        {
            Image playerIconImage = allMemberUIElements[0].Find("Icon").GetComponent<Image>();
            playerIconImage.sprite = playerCharacter.characterSprite;
            playerIconImage.color = Color.white;
            allMemberUIElements[0].GetComponent<Button>().interactable = true;
        }

        // Merge active and inactive party members into a single list, skipping the player character
        List<GameObject> allPartyMembers = new List<GameObject>(gameData.partyManager.activePartyMembersPrefabs);
        allPartyMembers.AddRange(gameData.partyManager.inactivePartyMembersPrefabs);


        Debug.Log("party count"+ allPartyMembers.Count);
        // Start filling UI elements from index 1, as index 0 is for the player character
        for (int i = 1; i < allMemberUIElements.Count; i++)
        {
            int partyMemberIndex = i - 1; // Adjust index for allPartyMembers
            if (partyMemberIndex < allPartyMembers.Count)
            {
                PlayerCharacter memberCharacter = allPartyMembers[partyMemberIndex].GetComponent<PlayerCharacter>();
                Image iconImage = allMemberUIElements[i].Find("Icon").GetComponent<Image>();

                iconImage.sprite = memberCharacter.characterSprite;
                iconImage.color = Color.white;
                allMemberUIElements[i].GetComponent<Button>().interactable = true;
            }
            else
            {
                // Clear and disable any remaining UI elements
                allMemberUIElements[i].GetComponent<Button>().interactable = false;
                allMemberUIElements[i].Find("Icon").GetComponent<Image>().color = Color.clear;
            }
        }
    }




    public void AddRemovePartyMember()
    {
        if (currentMember == 0) // Index 0 is reserved for the player and should return immediately
        {
            return;
        }

        if(currentMember > 3 &&gameData.partyManager.activePartyMembersPrefabs.Count == 3){//when you try to add but the party is already full
            return;
        } 
        // Adjust the index to account for the player character at index 0
        int adjustedIndex = currentMember - 1;

        // Combine active and inactive member lists
        List<GameObject> allPartyMembers = new List<GameObject>(gameData.partyManager.activePartyMembersPrefabs);
        allPartyMembers.AddRange(gameData.partyManager.inactivePartyMembersPrefabs);

   
        if (adjustedIndex >= 0 && adjustedIndex < allPartyMembers.Count)
        {
            GameObject memberPrefab = allPartyMembers[adjustedIndex];

            if (gameData.partyManager.activePartyMembersPrefabs.Contains(memberPrefab))
            {
                // Member is currently active; remove from active party
                gameData.partyManager.RemoveFromActiveParty(memberPrefab);
            }
            else if (gameData.partyManager.activePartyMembersPrefabs.Count < 4)
            {
                // Member is not active and there is space in the active party; add to active party
                gameData.partyManager.AddToActiveParty(memberPrefab);
            }

            // Align party members after any change to the party configuration
            AlignPartyMembers();
            InitializePanel(); // Update the UI to reflect the new party configuration
        }
        else
        {
            // Handle the case where the index is out of range of the combined list
            Debug.LogError("Selected member index is out of range.");
        }
    }


    // This method now starts a coroutine that will align party members after the animation
    public void SetPartyStatusForMenuChars(string animParameter, bool isInParty)
    {
        GameObject[] menuChars = GameObject.FindGameObjectsWithTag("MenuChar");
        float animationTime = 0f;

        foreach (GameObject menuChar in menuChars)
        {
            Animator animator = menuChar.GetComponent<Animator>();
            if (animator != null)
            {
                animator.SetBool(animParameter, isInParty);
                animationTime = animator.GetCurrentAnimatorStateInfo(0).length - .1f;

            }
        }
        if (isInParty == true)
        {
            StartCoroutine(WaitAndAlignPartyMembers(animationTime));
        }

    }

    private IEnumerator WaitAndAlignPartyMembers(float delay)
    {
        // Wait for the animation to complete
        yield return new WaitForSeconds(delay);

        // Now align the party members
        AlignPartyMembers();
    }
    // Call this method when opening the panel to align party members
    public void AlignPartyMembers()
    {
        int activeMembersCount = gameData.partyManager.activePartyMembersPrefabs.Count; // Number of active members
        int numInParty = 0;
        Debug.Log("active members:" + activeMembersCount);


        // Slide the active member UI elements to the left except the first one
        for (int i = 1; i < activeMembersCount + 1; i++)
        {
            // Assuming the first element stays in place, shift the others to the left
            activeMemberUIElements[i].anchoredPosition = new Vector2(
                activeMemberUIElements[0].anchoredPosition.x + (i * activeMemberUIElements[i].sizeDelta.x),
                activeMemberUIElements[i].anchoredPosition.y);
            numInParty++;
        }
        activeMembersCount += numInParty;
        // // Slide the inactive member UI elements to the right
        // Align the remaining 'not in party' active members to the right
        Debug.Log("not in party" + activeMembersCount);
        Debug.Log("end" + (activeMemberUIElements.Count - 1 - activeMembersCount));
        for (int i = activeMemberUIElements.Count - 1; i > numInParty; i--)
        {
            Debug.Log(i);
            // Calculate the new position for 'not in party' members 
            // This starts at the position of the first inactive member and moves each 'not in party' element to the left
            activeMemberUIElements[i].anchoredPosition = new Vector2(
                inactiveMemberUIElements[0].GetComponent<RectTransform>().anchoredPosition.x - ((activeMemberUIElements.Count - i) * activeMemberUIElements[0].sizeDelta.x),
                activeMemberUIElements[i].anchoredPosition.y);
        }
    }
    private void SetActiveMembersInteractable(bool isInteractable)
    {
        foreach (var uiElement in activeMemberUIElements)
        {
            Button button = uiElement.GetComponent<Button>();
            if (button != null)
            {
                button.interactable = isInteractable;
            }
        }
    }

    public void ResetPartyMemberPositions()
    {
        transform.Find("ActiveTag").GameObject().SetActive(false);
        // Reset the positions of active member UI elements to their original positions
        for (int i = 0; i < activeMemberUIElements.Count; i++)
        {
            activeMemberUIElements[i].anchoredPosition = originalPositionsActive[i];
        }

        // Set all active member UI elements to not be interactable initially when exiting the panel
        SetActiveMembersInteractable(false);

        // Make only 'in party' member UI elements interactable when exiting the panel
        for (int i = 0; i < gameData.partyManager.activePartyMembersPrefabs.Count + 1; i++)
        {
            Button button = activeMemberUIElements[i].GetComponent<Button>();
            if (button != null)
            {
                button.interactable = true;
            }
        }
        // Reset the positions of inactive member UI elements to their original positions
        for (int i = 0; i < inactiveMemberUIElements.Count; i++)
        {
            
            inactiveMemberUIElements[i].GetComponent<RectTransform>().anchoredPosition = originalPositionsInactive[i];
            inactiveMemberUIElements[i].GameObject().SetActive(false);
        }
    }
    public void Toggle()
    {
        Animator bgSlashAnimator = GameObject.Find("backgroundSlash").GetComponent<Animator>();
        RPGPanel rPGPanel = FindObjectOfType<RPGPanel>();
        rpgPanelAnimator = rPGPanel.GetComponent<Animator>();
        // Assuming that all MenuChars have similar animation lengths, 
        // we take the length of the first found MenuChar's animation
        GameObject firstMenuChar = GameObject.FindGameObjectWithTag("MenuChar");
        float animationLength = firstMenuChar.GetComponent<Animator>().GetCurrentAnimatorStateInfo(0).length;



        SpellPanel spellPanel = FindObjectOfType<SpellPanel>();
        if(spellPanel.isPanelOpen){
            spellPanel.Toggle();
        }

        if (isPanelOpen)
        {
            isPanelOpen = false;

            //shears sidebar
            bgSlashAnimator.SetBool("shear", true);

            // Start a coroutine to delay the closing by the animation length

            //reset label text
            GameObject.Find("panelLabelText").GetComponent<TextMeshProUGUI>().text = "Main";
            ResetPartyMemberPositions();
            rPGPanel.InitializePanel();
            fireTypeSelectionMenu.SetActive(false);
            changeFireTypeButton.SetActive(false);

            //grows party icons
            SetPartyStatusForMenuChars("inParty", false);

            //things that should happen at the end of the close elements by animation length
            rpgPanelAnimator.SetBool("inParty", false);
        }
        else
        {
            isPanelOpen = true;
            //unshears side bar
            bgSlashAnimator.SetBool("shear", false);
            bgSlashAnimator.SetBool("inGear", false);

            //close elements of main menu that don't carry over


            //shrinks party icons
            SetPartyStatusForMenuChars("inParty", true);


            rpgPanelAnimator.SetBool("inParty", true);
            rpgPanelAnimator.SetBool("inSpell", false);
            FindObjectOfType<SpellPanel>().isPanelOpen = false;
            UpdateMemberInfo(0);
            InitializePanel();
            GameObject.Find("panelLabelText").GetComponent<TextMeshProUGUI>().text = "Party";

        }
    }




}
