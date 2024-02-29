
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class OverworldUI : MonoBehaviour
{
    [Header("UI Elements")]
    public GameObject playerConditionPanel;    // UI element for player's condition
    public GameObject interactionPrompt;       // UI prompt for interactions
    public GameObject questLogIndicator;       // UI element for quest log 
    public GameObject gearInfoPrefab; // Reference to the gear info UI prefab
    private Animator overworldUIAnimator; // Animator component attached to the Overworld UI
    private bool disabled = true;
    public GameData gameData;
    public List<GameObject> partyConditionUI; // Assign this in the inspector with your UI elements
    private bool isAltHeld = false;            // Track if the Alt key is held
    private UIManager uIManager;
    private void Start()
    {
        overworldUIAnimator = GetComponent<Animator>();
        uIManager = FindObjectOfType<UIManager>();
    }
    private void Update()
    {
        if(disabled == false){
            HandleAltKey();
            // If the Alt key is held, keep updating the party condition UI.
            if (isAltHeld && uIManager.currentUIState == UIManager.PageState.OVERWORLD)
            {
                UpdatePartyConditionUI();
            }
        }

    }
  // Modified method to take a list of Gear items
public void DisplayItemInfo(List<Gear> gears)
{
    Canvas canvas = GetComponentInParent<Canvas>();
    if (canvas == null) return;

    float yOffset = 0; // Initial offset for the first item

    foreach (Gear newGear in gears)
    {
        GameObject infoPanel = Instantiate(gearInfoPrefab, canvas.transform, false);

        RectTransform rectTransform = infoPanel.GetComponent<RectTransform>();
        rectTransform.anchoredPosition = new Vector2(-710, 440 - yOffset);

        yOffset += rectTransform.rect.height + 5;

        TextMeshProUGUI gearText = infoPanel.GetComponentInChildren<TextMeshProUGUI>();
        if (gearText != null)
        {
            gearText.text = $"Got {newGear.gearName}!";
        }
    }
}

public void DisplayCharacterInfo(GameObject character)
{
    PlayerCharacter playerCharacter = character.GetComponent<PlayerCharacter>();
    if (playerCharacter == null) return;

    Canvas canvas = GetComponentInParent<Canvas>();
    if (canvas == null) return;

    GameObject infoPanel = Instantiate(gearInfoPrefab, canvas.transform, false);

    RectTransform rectTransform = infoPanel.GetComponent<RectTransform>();
    rectTransform.anchoredPosition = new Vector2(-710, 440);

    TextMeshProUGUI characterText = infoPanel.GetComponentInChildren<TextMeshProUGUI>();
    if (characterText != null)
    {
        characterText.text = $"{playerCharacter.characterName} was added to the party!";
    }
}


        // Call this function to update the party condition UI
    // Update the party condition UI elements
    private void UpdatePartyConditionUI()
    {
        // Set all inactive first
        SetAllPartyConditionUIInactive();

        // Update player's condition UI (index 0)

        UpdateCharacterConditionUI(partyConditionUI[0], FindObjectOfType<PlayerController>().GetComponent<PlayerCharacter>());
        
        // Now enable and update party members' UI (indices 1 to 3)
        for (int i = 1; i < partyConditionUI.Count; i++)
        {
            if (i - 1 < gameData.partyManager.activePartyMembersPrefabs.Count)
            {
                Debug.Log("got clone");
                GameObject cloneCharacter = gameData.partyManager.activePartyMembersPrefabs[i - 1];
                UpdateCharacterConditionUI(partyConditionUI[i], cloneCharacter);

            }
        }
    }


    // Set all party condition UI elements inactive and reset color/position
    private void SetAllPartyConditionUIInactive()
    {
        foreach (var conditionUI in partyConditionUI)
        {
            conditionUI.SetActive(false);
        }
    }
    // Call this function to update a single character's condition UI
    private void UpdateCharacterConditionUI(GameObject conditionUI, CharacterBase character)
    {
        conditionUI.SetActive(true);
        Slider hpBar = conditionUI.transform.Find("HPBar").GetComponent<Slider>();
        TextMeshProUGUI hpText = conditionUI.transform.Find("HPText").GetComponent<TextMeshProUGUI>();
        Slider mpBar = conditionUI.transform.Find("MPBar").GetComponent<Slider>();
        TextMeshProUGUI mpText = conditionUI.transform.Find("MPText").GetComponent<TextMeshProUGUI>();
        Image icon = conditionUI.transform.Find("Icon").GetComponent<Image>();

        // Assuming the health and mana are floats and within 0 to 1 range for slider values
        float currentHealth = character.characterStats.GetEffectiveStat(StatType.CURRENT_HEALTH);
        float maxHealth = character.characterStats.GetEffectiveStat(StatType.HEALTH);
        float currentMana = character.characterStats.GetEffectiveStat(StatType.CURRENT_MANA);
        float maxMana = character.characterStats.GetEffectiveStat(StatType.MANA);
        
        hpBar.value = currentHealth / maxHealth;
        hpText.text = ((int)currentHealth).ToString();
        mpBar.value = currentMana / maxMana;
        mpText.text = ((int)currentMana).ToString();
        
        icon.sprite = character.characterSprite;
        icon.sprite = character.characterSprite; // Assuming this is a direct reference to the character's sprite
    }
    private void UpdateCharacterConditionUI(GameObject conditionUI, GameObject clone)
    {
        conditionUI.SetActive(true);
        Slider hpBar = conditionUI.transform.Find("HPBar").GetComponent<Slider>();
        TextMeshProUGUI hpText = conditionUI.transform.Find("HPText").GetComponent<TextMeshProUGUI>();
        Slider mpBar = conditionUI.transform.Find("MPBar").GetComponent<Slider>();
        TextMeshProUGUI mpText = conditionUI.transform.Find("MPText").GetComponent<TextMeshProUGUI>();
        Image icon = conditionUI.transform.Find("Icon").GetComponent<Image>();

        PlayerStats stats = gameData.partyManager.GetStat(clone);
        // Assuming the health and mana are floats and within 0 to 1 range for slider values
        float currentHealth = stats.GetEffectiveStat(StatType.CURRENT_HEALTH);
        float maxHealth = stats.GetEffectiveStat(StatType.HEALTH);
        float currentMana = stats.GetEffectiveStat(StatType.CURRENT_MANA);
        float maxMana = stats.GetEffectiveStat(StatType.MANA);
        
        hpBar.value = currentHealth / maxHealth;
        hpText.text = ((int)currentHealth).ToString();
        mpBar.value = currentMana / maxMana;
        mpText.text = ((int)currentMana).ToString();
        
        icon.sprite = clone.GetComponent<PlayerCharacter>().characterSprite;
        icon.sprite = clone.GetComponent<PlayerCharacter>().characterSprite; // Assuming this is a direct reference to the character's sprite
    }
    private void HandleAltKey()
    {
        if (Input.GetKey(KeyCode.LeftAlt) || Input.GetKey(KeyCode.RightAlt))
        {
            if (!isAltHeld) // Only trigger once per key press
            {
                isAltHeld = true;
                UpdatePartyConditionUI();
                ShowOverworldUI();
            }
        }
        else if (isAltHeld) // Key is released
        {
            isAltHeld = false;
            HideOverworldUI();
        }
    }

    private void ShowOverworldUI()
    {   
        //update the UI before showing
        UpdatePartyConditionUI();
        // Play the slide-in animation
        overworldUIAnimator.SetBool("SlideIn", true);
        
        // For each party member, toggle on the player condition UI
        //foreach(PlayerCharacter pc in PartyManager.Instance.CurrentPartyMembers)
       // {
            // Get or instantiate the player condition panel for this character.
            // Set its values (HP, MP, etc.) based on the character's stats.
            // Set the panel to active.
       // }
    }

    private void HideOverworldUI()
    {
        // Reverse the slide-in animation for slide-out effect
        overworldUIAnimator.SetBool("SlideIn", false);
    }
    public void Disable(){
        disabled = true;
        ForceClose();
    }
    public void Undisable(){
        disabled = false;
    }
    public void ForceClose(){
        //overworldUIAnimator.SetTrigger("ForceClose");
    }


}
