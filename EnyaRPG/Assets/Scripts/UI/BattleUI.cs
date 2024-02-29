using UnityEngine;
using System.Collections.Generic;
using UnityEngine.UI;
using System;
using System.Collections;  // If you're using Unity's UI system.
using TMPro;
using Unity.VisualScripting;
public class BattleUI : MonoBehaviour
{
    [Header("Action Buttons and Key Mappings")]
    public Button spellButton;
    public Button attackButton;
    public Button healButton;
    public Button igniteButton;
    // Add more buttons if needed
    public List<GameObject> partyConditionUI; // Assign this in the inspector with your UI elements
    public Color activeCharacterColor = new Color(255f / 255f, 165f / 255f, 0f / 255f); // The color for the active character
    public Color defaultColor = Color.white; // The default color for the condition UI
    public float shiftAmount = 10f; // The amount to shift the active character UI to the right

    private Dictionary<KeyCode, Button> keyButtonMappings;
    public GameObject SpellMenu;
    public TextMeshProUGUI spellDescriptionText;
    public List<GameObject> SpellMenuButton;
    public Vector3 selectedShift = new Vector3(-10f, 0f, 0f);  // shift 10 units to the left
    private List<Vector3> originalPositions = new List<Vector3>();

    public GameObject actionMenu;
    public Toggle turnStateToggle;
    public GameObject targetSelectionUI;
    public GameObject selectionPrompt;
    private int currentTargetIndex = 0;
    private Coroutine currentFadeCoroutine = null;
    public enum TargetSelectionContext
    {
        Enemies,
        Allies
    }
    private TargetSelectionContext currentTargetSelectionContext = TargetSelectionContext.Enemies;

    public BattleController battleController;
    public event Action<CharacterBase> OnTargetSelected = delegate { };
    public event Action<int> OnSpellSelected = delegate { };
    public GameObject SpellLabel; // displayed at the top of the screen when a spell is used
    public GameObject statusPage; // Or you can use GameObject if you prefer setActive
    public TextMeshProUGUI characterNameText;
    public Image characterSprite;
    private bool isPlayerStatusActive;
    private bool tabIsHeld = false;
    private int currentListIndex = 0;
    public TextMeshProUGUI buffsText;
    public TextMeshProUGUI debuffsText;
    public TextMeshProUGUI spellsText;
    public bool inputLocked = false;
    [Header("Turn Wheel")]
    public List<Image> turnWheelIcons;
    [Serializable]
    public class HeatGauge
    {
        public Slider heatSlider;
        public GameObject greenArea;
        public GameObject heatArrow;
        public Animator animator;
        public GameObject redArea;
        [HideInInspector] public Material currentMaterial;
        [HideInInspector] public Color originalColor;
    }
    public float redIntensityMultiplier = 1.5f; // You can adjust this value to get the desired effect

    public HeatGauge playerHeatGauge;
    public HeatGauge enemyHeatGauge;

    private Coroutine playerIntensityCoroutine;
    private Coroutine enemyIntensityCoroutine;

    // Assuming you've created a curve in Unity Editor that follows your desired intensity pattern
    public AnimationCurve intensityCurve;
    private Coroutine currentIntensityCoroutine; // To keep a reference to the running coroutine

    public GameObject playerPreviewSlider;
    public GameObject enemyPreviewSlider;
    public List<Toggle> fuelGauge;
    [Header("Animation References")]
    public Animator actionMenuAnimator;
    public Animator spellTabAnimator;
    public Animator activeCharacterAnimator;
    

    public bool TabIsHeld { get => tabIsHeld; set => tabIsHeld = value; }

    public void Awake()
    {
        keyButtonMappings = new Dictionary<KeyCode, Button>
        {
            { KeyCode.Q, spellButton },
            { KeyCode.E, attackButton },
            { KeyCode.R, healButton },
            { KeyCode.C, igniteButton }
            // Add more key-button pairs if needed
        };
        foreach (GameObject button in SpellMenuButton)
        {
            originalPositions.Add(button.transform.localPosition);
        }
    }

    private void Update()
    {

        // Check if all animations are in the "Idle" state
        if (inputLocked) return;

        if (keyButtonMappings == null)
        {
            keyButtonMappings = new Dictionary<KeyCode, Button>
        {
            { KeyCode.Q, spellButton },
            { KeyCode.E, attackButton },
            { KeyCode.R, healButton },
            { KeyCode.C, igniteButton }
                // Add more key-button pairs if needed
            };
        }
            foreach (var keyMapping in keyButtonMappings)
            {
                if (Input.GetKeyDown(keyMapping.Key) && actionMenu.activeSelf)
                {
                    keyMapping.Value.onClick.Invoke();
                }
            }
        
        if (SpellMenu.activeSelf)
        {
            UpdateSpellDescription();
        }
        if (actionMenu.activeSelf)
        {
            StartCoroutine(AlignUIWithCenter(actionMenu, battleController.activeCharacter.GameObject()));
        }

        // see buffs and debufss

        if (Input.GetKeyDown(KeyCode.Tab))
        {
            TabIsHeld = true;
            if (currentTargetSelectionContext == TargetSelectionContext.Allies || !targetSelectionUI.activeSelf)
            {
                isPlayerStatusActive = true;
                // Find the index of the active character in the alivePlayers list
                currentListIndex = battleController.playerParty.FindIndex(character => character == battleController.activeCharacter);

                ShowStatusPageForCharacter(battleController.activeCharacter);
            }
            else
            {
                isPlayerStatusActive = false;
                currentListIndex = currentTargetIndex;
                CharacterBase selectedTarget = battleController.aliveEnemies[currentTargetIndex].GetComponent<CharacterBase>();
                ShowStatusPageForCharacter(selectedTarget);
            }
        }
        if (Input.GetKeyUp(KeyCode.Tab))
        {
            TabIsHeld = false;
            HideStatusPage();
        }
        if (TabIsHeld)
        {
            // Handle scrolling through lists
            if (Input.GetKeyDown(KeyCode.W) || Input.GetKeyDown(KeyCode.UpArrow) || Input.GetKeyDown(KeyCode.S) || Input.GetKeyDown(KeyCode.DownArrow))
            {
                if (isPlayerStatusActive)
                {
                    isPlayerStatusActive = false;
                    ShowStatusPageForCharacter(battleController.aliveEnemies[0].GetComponent<CharacterBase>());
                }
                else
                {
                    isPlayerStatusActive = true;
                    ShowStatusPageForCharacter(battleController.alivePlayers[0].GetComponent<CharacterBase>());
                }

            }

            //add the left right/ad scrolling here
            if (Input.GetKeyDown(KeyCode.A) || Input.GetKeyDown(KeyCode.LeftArrow))
            {
                currentListIndex = Mathf.Max(0, currentListIndex - 1);
                if (isPlayerStatusActive)
                {
                    
                    ShowStatusPageForCharacter(battleController.playerParty[currentListIndex].GetComponent<CharacterBase>());
                }
                else
                {
            
                    ShowStatusPageForCharacter(battleController.aliveEnemies[currentListIndex].GetComponent<CharacterBase>());
                }
            }
            else if (Input.GetKeyDown(KeyCode.D) || Input.GetKeyDown(KeyCode.RightArrow))
            {
                currentListIndex = Mathf.Min((isPlayerStatusActive ? battleController.playerParty.Count : battleController.aliveEnemies.Count) - 1, currentListIndex + 1);
                // Update status page based on currentListIndex
                if (isPlayerStatusActive)
                {

                    ShowStatusPageForCharacter(battleController.playerParty[currentListIndex].GetComponent<CharacterBase>());
                }
                else
                {

                    ShowStatusPageForCharacter(battleController.aliveEnemies[currentListIndex].GetComponent<CharacterBase>());
                }
            }

            return;
        }

        if (targetSelectionUI.activeSelf)
        {

            if (Input.GetKeyDown(KeyCode.A) || Input.GetKeyDown(KeyCode.LeftArrow))
            {
                Debug.Log("Shifting left");
                MoveTargetSelection(false); // Move left
            }
            else if (Input.GetKeyDown(KeyCode.D) || Input.GetKeyDown(KeyCode.RightArrow))
            {
                Debug.Log("Shifting right");
                MoveTargetSelection(true); // Move right
            }

            if (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.Space) && actionMenu.activeSelf == false)
            {
                ConfirmSelection();
            }


        }



    }
    // Call this function to update the party condition UI
    public void UpdatePartyConditionUI()
    {
        // Set all inactive first
        SetAllPartyConditionUIInactive();

        // Now enable and update as many as there are party members
        for (int i = 0; i < battleController.playerParty.Count; i++)
        {
            GameObject conditionUI = partyConditionUI[i];
            CharacterBase partyMember = battleController.playerParty[i].GetComponent<CharacterBase>();

            conditionUI.SetActive(true); // Enable the UI
            UpdateCharacterConditionUI(conditionUI, partyMember); // Update the UI with character's data

            // If this is the active character, shift it to the right and change color
            if (battleController.activeCharacter == partyMember)
            {
                conditionUI.transform.localPosition += Vector3.right * shiftAmount; // Adjust the x-position
                conditionUI.GetComponent<Image>().color = activeCharacterColor; // Set the color for the active character
            }
            else
            {
                conditionUI.transform.localPosition = new Vector3(-765f, conditionUI.transform.localPosition.y, conditionUI.transform.localPosition.z); // Reset x-position
                conditionUI.GetComponent<Image>().color = defaultColor; // Reset color
            }
        }
    }

    // Call this function to set all party condition UI elements inactive and reset color/position
    public void SetAllPartyConditionUIInactive()
    {
        foreach (var conditionUI in partyConditionUI)
        {
            conditionUI.SetActive(false);
            conditionUI.transform.localPosition = new Vector3(-765f, conditionUI.transform.localPosition.y, conditionUI.transform.localPosition.z); // Reset x-position
            conditionUI.GetComponent<Image>().color = defaultColor; // Reset color
        }
    }

    private void UpdateCharacterConditionUI(GameObject conditionUI, CharacterBase character)
    {
        Slider hpBar = conditionUI.transform.Find("HPBar").GetComponent<Slider>();
        Slider subBar = hpBar.transform.Find("subBar").GetComponent<Slider>(); // Assuming the SubBar is a child of HPBar
        TextMeshProUGUI hpText = conditionUI.transform.Find("HPText").GetComponent<TextMeshProUGUI>();
        Slider mpBar = conditionUI.transform.Find("MPBar").GetComponent<Slider>();
        TextMeshProUGUI mpText = conditionUI.transform.Find("MPText").GetComponent<TextMeshProUGUI>();
        Image icon = conditionUI.transform.Find("Icon").GetComponent<Image>();

        subBar.maxValue = hpBar.maxValue;
        subBar.value = hpBar.value;
        float currentHealth = character.characterStats.GetEffectiveStat(StatType.CURRENT_HEALTH);
        float maxHealth = character.characterStats.GetEffectiveStat(StatType.HEALTH);
        float currentMana = character.characterStats.GetEffectiveStat(StatType.CURRENT_MANA);
        float maxMana = character.characterStats.GetEffectiveStat(StatType.MANA);

        hpBar.value = currentHealth / maxHealth;
        hpText.text = ((int)currentHealth).ToString();
        mpBar.value = currentMana / maxMana;
        mpText.text = ((int)currentMana).ToString();
        icon.sprite = character.characterSprite;


        // Start coroutine to update SubBar
        StartCoroutine(UpdateSubBar(subBar, currentHealth / maxHealth));
    }

    // Coroutine to gradually decrease the SubBar value
    private IEnumerator UpdateSubBar(Slider subBar, float targetValue)
    {
        float duration = 2.5f; // Duration over which the bar will decrease
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

    private bool IsAnimationIdle(Animator animator)
    {
        if (animator == null)
        {
            return true;
        }
        if (animator.GetCurrentAnimatorStateInfo(0).IsName("Idle") && animator.GetCurrentAnimatorStateInfo(0).normalizedTime < 1.0f)
            return true;
        return false;
    }

    public void ShowStatusPageForCharacter(CharacterBase activeCharacter)
    {
        // if (activeCharacter == null)

        // Update UI elements with active character's information
        characterNameText.text = activeCharacter.characterName;
        characterSprite.sprite = activeCharacter.characterSprite;

        // Loop through the list of status effects and separate them into buffs and debuffs
        buffsText.text = ""; // Reset
        debuffsText.text = ""; // Reset
        spellsText.text = "";// reset
        foreach (var effect in activeCharacter.characterStats.activeStatusEffects)
        {
            if (effect is Buff)
            {
                buffsText.text += $"{effect.label}:\n       {effect.GetDescription()}\n\n"; // Append to buffsText
            }
            else if (effect is Debuff)
            {
                debuffsText.text += $"{effect.label}:\n     {effect.GetDescription()}\n\n"; // Append to debuffsText
            }
            // If you have more status effect types in the future, just add more conditions here.
        }
    // Check if the active character is an EnemyCharacter to display its spells
        if (activeCharacter is EnemyCharacter enemyCharacter)
        {
            foreach (EnemySpell spell in enemyCharacter.GetEnemyStats().spells)
            {
                spellsText.text += $"{spell.spellName}:\n   {spell.description}\n\n"; // Append spell info
            }
        }    else if (activeCharacter is PlayerCharacter playerCharacter)
        {
            foreach (Spell spell in playerCharacter.GetPlayerStats().spellList)
            {
                spellsText.text += $"{spell.spellName}:\n   {spell.description}\n\n"; // Append spell info
            }
        }

        // Show the status page
        statusPage.SetActive(true);
    }

    public void HideStatusPage()
    {
        // Hide the status page
        statusPage.SetActive(false);
    }

    public IEnumerator SetTurnStateIndicator(TurnState turnState)
    {
        this.GetComponent<Animator>().SetTrigger("SwitchTurn");
        yield return new WaitForSeconds(.25f);
        if (turnStateToggle != null)
        {
            switch (turnState)
            {
                case TurnState.BURN:
                    turnStateToggle.isOn = true; // "On" for BURN
                    break;

                case TurnState.FREEZE:
                    turnStateToggle.isOn = false; // "Off" for FREEZE
                    break;

                default:
                    Debug.LogError("Unrecognized turn state: " + turnState);
                    break;
            }
        }
        else
        {
            Debug.LogError("The turnStateIndicator GameObject does not have a Toggle component.");
        }
        yield break;
    }

    public void UpdateFuelUI()
    {
        for (int i = 0; i < battleController.maxFuel; i++)
        {
            if (i < battleController.fuel)
            {
                //Debug.Log(fuelGauge[i]);

                fuelGauge[i].isOn = true;
                fuelGauge[i].GameObject().GetComponent<Animator>().SetTrigger("GotFuel"); // Play the add animation
            }
            else
            {
                fuelGauge[i].isOn = false;
            }
        }
    }

    public void UpdatePlayerHeat(float heatValue)
    {
        playerHeatGauge.heatSlider.value = heatValue;
        HeatState heatColor = battleController.GetHeatColor(heatValue);
        HandleHeatIntensity(heatColor, playerHeatGauge);
        UpdateHeatText(playerHeatGauge.heatArrow, heatColor);
    }

    public void UpdateEnemyHeat(float heatValue)
    {
        enemyHeatGauge.heatSlider.value = heatValue;
        HeatState heatColor = battleController.GetHeatColor(heatValue);
        HandleHeatIntensity(heatColor, enemyHeatGauge);
        UpdateHeatText(enemyHeatGauge.heatArrow, heatColor);
    }
    public IEnumerator UpdateBothHeatsSequentially(float playerHeatValue, float enemyHeatValue)
    {
        yield return StartCoroutine(UpdateHeatCoroutine(playerHeatValue, playerHeatGauge));
        yield return StartCoroutine(UpdateHeatCoroutine(enemyHeatValue, enemyHeatGauge));
    }

    private IEnumerator UpdateHeatCoroutine(float heatValue, HeatGauge heatGauge)
    {
        HeatState previousHeatColor = battleController.GetHeatColor(heatGauge.heatSlider.value);
        UpdateHeat(heatValue, heatGauge);


        // Wait for the animation to complete if color changed
        HeatState newHeatColor = battleController.GetHeatColor(heatValue);
        if (previousHeatColor != newHeatColor)
        {
            float animationDuration = UpdateHeatGaugeAnimation(heatGauge.animator, newHeatColor);
            yield return new WaitForSeconds(animationDuration);
        }
    }

    public void UpdateHeat(float heatValue, HeatGauge heatGauge)
    {
        heatGauge.heatSlider.value = heatValue;
        HeatState heatColor = battleController.GetHeatColor(heatValue);
        HandleHeatIntensity(heatColor, heatGauge);
        UpdateHeatText(heatGauge.heatArrow, heatColor);
        if (battleController.GetHeatColor(heatGauge.heatSlider.value) != heatColor)
            UpdateHeatGaugeAnimation(heatGauge.animator, heatColor);
    }

    public void UpdateHeatText(GameObject heatArrow, HeatState heatState)
    {
        Image arrow = heatArrow.GetComponent<Image>();
        Transform arrowTransform = heatArrow.GetComponent<Transform>();
        if (heatState == HeatState.Green)
        {
            arrowTransform.localScale = new Vector3(1, 1, 1);
            arrow.color = Color.green;
        }
        else if (heatState == HeatState.Red)
        {
            arrowTransform.localScale = new Vector3(1, -1, 1);
            arrow.color = Color.red;
        }
        else
        {
            arrowTransform.localScale = new Vector3(1, 0.2f, 1);
            arrow.color = Color.gray;
        }
    }
    private float UpdateHeatGaugeAnimation(Animator animator, HeatState heatColor)
    {
        string triggerName = "";

        switch (heatColor)
        {
            case HeatState.Green:
                triggerName = "ToGreen";
                break;
            case HeatState.Yellow:
                triggerName = "ToYellow";
                break;
            case HeatState.Red:
                triggerName = "ToRed";
                break;
        }

        animator.SetTrigger(triggerName);

        // Assuming the state names in Animator are the same as the trigger names
        AnimationClip[] clips = animator.runtimeAnimatorController.animationClips;
        foreach (AnimationClip clip in clips)
        {
            if (clip.name == triggerName)
            {
                return clip.length;
            }
        }

        return 0f; // Return a default value if the clip isn't found
    }

    private void HandleHeatIntensity(HeatState heatState, HeatGauge heatGauge)
    {
        ResetAllIntensity(heatGauge);  // Call this at the start
        GameObject targetArea = null;

        if (heatState == HeatState.Green) targetArea = heatGauge.greenArea;
        else if (heatState == HeatState.Red)
        {
            targetArea = heatGauge.redArea;
            heatGauge.originalColor *= redIntensityMultiplier; // Apply the multiplier for red
        }

        if (heatGauge == playerHeatGauge && playerIntensityCoroutine != null)
        {
            StopCoroutine(playerIntensityCoroutine);
            playerIntensityCoroutine = null;
        }
        else if (heatGauge == enemyHeatGauge && enemyIntensityCoroutine != null)
        {
            StopCoroutine(enemyIntensityCoroutine);
            enemyIntensityCoroutine = null;
        }

        if (targetArea != null)
        {
            heatGauge.currentMaterial = targetArea.GetComponent<Image>().material;
            heatGauge.originalColor = heatGauge.currentMaterial.GetColor("_EmissionColor");

            if (heatGauge == playerHeatGauge)
            {
                playerIntensityCoroutine = StartCoroutine(AdjustEmissionIntensity(heatGauge.currentMaterial));
            }
            else if (heatGauge == enemyHeatGauge)
            {
                enemyIntensityCoroutine = StartCoroutine(AdjustEmissionIntensity(heatGauge.currentMaterial));
            }
        }
    }


    private IEnumerator AdjustEmissionIntensity(Material targetMaterial)
    {
        float time = 0f;
        Color originalColor = targetMaterial.GetColor("_EmissionColor");

        while (true)
        {
            float intensity = Mathf.Clamp(intensityCurve.Evaluate(time), 0, float.MaxValue);

            // This ensures the multiplication never makes the color negative
            Color newEmission = new Color(
                originalColor.r * Mathf.Max(0, intensity),
                originalColor.g * Mathf.Max(0, intensity),
                originalColor.b * Mathf.Max(0, intensity)
            );

            targetMaterial.SetColor("_EmissionColor", newEmission);

            time += Time.deltaTime;
            if (time > intensityCurve.keys[intensityCurve.length - 1].time)
            {
                time = 0;
            }

            yield return null;
        }
    }


    private void ResetAllIntensity(HeatGauge heatGauge)
    {
        List<GameObject> allAreas = new List<GameObject> { heatGauge.greenArea, heatGauge.redArea /*, Add other areas if present */ };
        foreach (GameObject area in allAreas)
        {
            if (area.GetComponent<Image>().material != null)
            {
                Material mat = area.GetComponent<Image>().material;
                Color baseColor = mat.GetColor("_BaseColor"); // Assuming you have a "_BaseColor" property in your shader.
                mat.SetColor("_EmissionColor", baseColor);
            }
        }
    }
    public void ToggleCharacterSelectionUI(bool show, TargetSelectionContext context)
    {
        currentTargetSelectionContext = context;
        currentTargetIndex = 0;
        targetSelectionUI.SetActive(show);
        selectionPrompt.SetActive(show);
        enemyPreviewSlider.SetActive(show && context == TargetSelectionContext.Enemies);
        playerPreviewSlider.SetActive(show);

        if (currentFadeCoroutine != null)
        {
            StopCoroutine(currentFadeCoroutine);
            battleController.aliveEnemies[currentTargetIndex].GetComponent<SpriteRenderer>().color = Color.white;
            currentFadeCoroutine = null; // Ensure this is set to null here
        }

        if (show)
        {
            StartCoroutine(UpdateTargetUIPosition());
        }
        else
        {
            FindObjectOfType<CameraController>().currentMode = CameraMode.Battle;
        }

        LockInputForDuration(0.1f);
    }

    private void LockInputForDuration(float duration)
    {
        StartCoroutine(LockInputCoroutine(duration));
    }

    public IEnumerator LockInputCoroutine(float duration)
    {
        inputLocked = true;
        yield return new WaitForSeconds(duration);
        inputLocked = false;
    }

    public void UnhighlightAllTurnWheelIcons()
    {
        foreach (Image icon in turnWheelIcons)
        {
            // Reset the icon to its default state
            // This can be an alpha change, color change, or whatever your default "unhighlighted" state is.
            if (icon.color != Color.clear)
                icon.color = Color.white;  // Assuming white is your unhighlighted color.
        }
    }

    private void ConfirmSelection()
    {
        if (tabIsHeld) return;
        UnhighlightAllTurnWheelIcons();

        List<GameObject> currentTargetList = currentTargetSelectionContext == TargetSelectionContext.Enemies
            ? battleController.aliveEnemies
            : battleController.alivePlayers;

        if (currentTargetIndex >= 0 && currentTargetIndex < currentTargetList.Count)
        {
            CharacterBase selectedTarget = currentTargetList[currentTargetIndex].GetComponent<CharacterBase>();
            if (selectedTarget != null)
            {
                OnTargetSelected(selectedTarget);
            }
        }
    }

    public void ClearOnTargetSelectedSubscribers()
    {
        OnTargetSelected = null;
    }

    public void MoveTargetSelection(bool moveRight)
    {
        List<GameObject> currentTargetList = currentTargetSelectionContext == TargetSelectionContext.Enemies
            ? battleController.aliveEnemies
            : battleController.alivePlayers;
        // Stop ongoing fade coroutine if any
        if (currentFadeCoroutine != null)
        {
            StopCoroutine(currentFadeCoroutine);
            currentTargetList[currentTargetIndex].GetComponent<SpriteRenderer>().color = Color.white;
            currentFadeCoroutine = null; // Ensure this is set to null here
        }

        // Move to the next target if moveRight is true, else move to the previous target
        currentTargetIndex += moveRight ? 1 : -1;

        // Loop around the list if needed
        if (currentTargetIndex >= currentTargetList.Count)
        {
            currentTargetIndex = 0;
        }
        else if (currentTargetIndex < 0)
        {
            currentTargetIndex = currentTargetList.Count - 1;
        }

        Debug.Log("starting selection");
        StartCoroutine(UpdateTargetUIPosition());
    }


    private IEnumerator FadeSpriteColor(GameObject target, Color startColor, Color endColor, float duration)
    {
        SpriteRenderer spriteRenderer = target.GetComponent<SpriteRenderer>();
        if (spriteRenderer == null) yield break;

        float direction = 1f; // 1 for forward, -1 for reverse
        float elapsedTime = 0;

        while (true)
        {
            spriteRenderer.color = Color.Lerp(startColor, endColor, (elapsedTime / duration));
            elapsedTime += Time.deltaTime * direction;

            if (elapsedTime >= duration || elapsedTime <= 0)
            {
                direction *= -1; // Reverse the direction
                elapsedTime = Mathf.Clamp(elapsedTime, 0, duration);
            }

            yield return null;
        }
    }


    public IEnumerator UpdateTargetUIPosition()
    {
        inputLocked = true;
        if (currentTargetSelectionContext == TargetSelectionContext.Enemies)
        {
            Debug.Log("target is enemy");
            if (battleController.aliveEnemies.Count > 0)
            {
                if (currentTargetIndex >= battleController.aliveEnemies.Count)
                {
                    currentTargetIndex = battleController.aliveEnemies.Count - 1;
                }
                GameObject targetEnemy = battleController.aliveEnemies[currentTargetIndex];

                Debug.Log("found target object");

                // Start the color fade for the targeted character.
                if (currentFadeCoroutine == null)
                {
                    currentFadeCoroutine = StartCoroutine(FadeSpriteColor(targetEnemy, Color.gray, Color.white, 1f));
                }

                Debug.Log("updating camera position");
                yield return StartCoroutine(FindObjectOfType<CameraController>().FocusOnTarget(targetEnemy.GetComponent<CharacterBase>(), battleController.aliveEnemies));
                Debug.Log("exited updating camera position");

                // Highlight the turn wheel icon corresponding to the target enemy.
                HighlightTurnWheelIcon(targetEnemy);

                yield return StartCoroutine(AlignUIWithCenter(targetSelectionUI, targetEnemy));

                inputLocked = false;

                // Preview of what spell selection will do to enemy heat gauge.
                enemyPreviewSlider.GetComponent<Slider>().value = battleController.enemyHeat + battleController.getGeatHeatGain(targetEnemy.GetComponent<CharacterBase>());
                if (battleController.activeCharacter.GetComponent<PlayerCharacter>() is PlayerCharacter player)
                {
                    Spell spell = player.GetSelectedSpell();
                    if (spell)
                    {
                        playerPreviewSlider.GetComponent<Slider>().value = battleController.playerHeat + battleController.SpellHeat(spell);
                    }

                }

            }
        }
        else
        {
            if (battleController.aliveEnemies.Count > 0)
            {
                if (currentTargetIndex >= battleController.alivePlayers.Count)
                {
                    currentTargetIndex = battleController.alivePlayers.Count - 1;
                }
                GameObject targetPlayer = battleController.alivePlayers[currentTargetIndex];
                inputLocked = true;
                yield return StartCoroutine(FindObjectOfType<CameraController>().FocusOnTarget(targetPlayer.GetComponent<CharacterBase>(), battleController.alivePlayers));
                // Highlight the turn wheel icon corresponding to the target enemy.
                HighlightTurnWheelIcon(targetPlayer);

                yield return StartCoroutine(AlignUIWithCenter(targetSelectionUI, targetPlayer));

                inputLocked = false;
                if (battleController.activeCharacter.GetComponent<PlayerCharacter>() is PlayerCharacter player)
                {
                    playerPreviewSlider.GetComponent<Slider>().value = battleController.playerHeat + battleController.SpellHeat(player.GetSelectedSpell());
                }
            }
        }
        yield break;
    }

    private int GetIndexOfTargetInTurnQueue(GameObject targetEnemy)
    {
        List<CharacterBase> turnQueueList = new List<CharacterBase>(battleController.turnQueue);

        for (int i = 0; i < turnQueueList.Count; i++)
        {
            if (turnQueueList[i].gameObject == targetEnemy)
            {
                return i + 1;
            }
        }
        return -1;  // Return -1 if not found.
    }



    private void HighlightTurnWheelIcon(GameObject targetEnemy)
    {
        // Reset all icons first.
        foreach (var icon in turnWheelIcons)
        {
            if (icon.color != Color.clear)
                icon.color = Color.white; // assuming white is the default color.
        }

        // Get index of the target enemy in the turn queue.
        int turnQueueIndex = GetIndexOfTargetInTurnQueue(targetEnemy);

        if (turnQueueIndex != -1 && turnQueueIndex < turnWheelIcons.Count)
        {
            // Set the color of the icon corresponding to the target enemy. 
            // Assuming yellow is the highlight color.
            turnWheelIcons[turnQueueIndex].color = Color.yellow;
        }
    }
    public RectTransform bu;
    public IEnumerator AlignUIWithCenter(GameObject uiObject, GameObject targetObject)
    {
        if (!targetObject) yield break;

        Renderer targetRenderer = targetObject.transform.Find("sprite").GetComponentInChildren<Renderer>();
        if (!targetRenderer) yield break;

        Vector3 targetCenter = targetRenderer.bounds.center;
        Vector2 screenPos = Camera.main.WorldToScreenPoint(targetCenter);

        RectTransform uiRect = uiObject.GetComponent<RectTransform>();
        Canvas canvas = uiObject.GetComponentInParent<Canvas>();
        CanvasScaler canvasScaler = canvas.GetComponent<CanvasScaler>();

        // Adjust the screen position based on the scale factor if needed
        if (canvasScaler && canvasScaler.uiScaleMode == CanvasScaler.ScaleMode.ScaleWithScreenSize)
        {
            screenPos /= canvasScaler.scaleFactor;
        }

        // Convert screen position to a position relative to the canvas
        Vector2 canvasSize = canvasScaler.referenceResolution;
        Vector2 localPoint = new Vector2(
            (screenPos.x - (Screen.width / 2)) / (Screen.width / 2) * (canvasSize.x / 2),
            (screenPos.y - (Screen.height / 2)) / (Screen.height / 2) * (canvasSize.y / 2)
        );

        // Apply a slight offset to account for the difference observed with Debug.DrawLine
        localPoint += new Vector2(0, targetRenderer.bounds.extents.y * canvasScaler.scaleFactor);

        // Apply the local point to the UI object's anchored position
        uiRect.anchoredPosition = localPoint;

        yield break;
    }


    public IEnumerator UpdateTurnWheel(Queue<CharacterBase> turnQueue)
    {
        Animator turnWheelAnimator = turnWheelIcons[0].transform.parent.transform.parent.GetComponent<Animator>();

        // Trigger the animation.
        turnWheelAnimator.SetTrigger("ShiftTurnWheel");

        // Wait for the animation to complete. You might want to replace this with a more accurate method
        // like an animation event or using AnimatorStateInfo.
        float animationDuration = 0.15f;  // Replace with your actual animation duration.
        yield return new WaitForSeconds(animationDuration);

        List<CharacterBase> turnOrder = new List<CharacterBase>(turnQueue);

        // Initially disable all icons.
        foreach (var icon in turnWheelIcons)
        {
            if (icon)
            {
                icon.color = Color.clear;
            }
        }

        // Ensure we don't go out of bounds.
        int maxTurnsToShow = Mathf.Min(turnOrder.Count, turnWheelIcons.Count);

        for (int i = 0; i < maxTurnsToShow; i++)
        {
            Sprite characterSprite = turnOrder[i].characterSprite;
            turnWheelIcons[i].sprite = characterSprite;
            turnWheelIcons[i].color = Color.white;
        }
        yield return new WaitForSeconds(.5f - animationDuration);
    }


    public void OnButtonSelect(GameObject selectedButton)
    {
        for (int i = 0; i < SpellMenuButton.Count; i++)
        {
            if (SpellMenuButton[i] == selectedButton)
            {
                SpellMenuButton[i].transform.localPosition = originalPositions[i] + selectedShift;
            }
        }
    }

    public void OnButtonDeselect(GameObject deselectedButton)
    {
        for (int i = 0; i < SpellMenuButton.Count; i++)
        {
            if (SpellMenuButton[i] == deselectedButton)
            {
                SpellMenuButton[i].transform.localPosition = originalPositions[i];
            }
        }
    }


    // This method will be called by the spell buttons
    public void SpellButtonClicked(int index)
    {
        if (tabIsHeld) return;
        OnSpellSelected.Invoke(index);
    }
    private void UpdateSpellDescription()
    {
        // Determine the currently selected button's index
        GameObject currentSelected = UnityEngine.EventSystems.EventSystem.current.currentSelectedGameObject;
        if (currentSelected != null)
        {
            int selectedIndex = SpellMenuButton.IndexOf(currentSelected);

            PlayerCharacter activePlayer = battleController.activeCharacter as PlayerCharacter;
            if (activePlayer != null && activePlayer.characterStats is PlayerStats playerStats)
            {
                if (selectedIndex != -1 && selectedIndex < playerStats.spellList.Count)
                {
                    Spell selectedSpell = playerStats.spellList[selectedIndex];

                    // Append the base power of the spell to its description
                    string fullDescription = $"{selectedSpell.description}\nBasePower: {selectedSpell.basePower * 100}";

                    spellDescriptionText.text = fullDescription;
                }
            }
        }
    }
    public void ResetAllButtonPositions()
    {
        for (int i = 0; i < SpellMenuButton.Count; i++)
        {
            GameObject button = SpellMenuButton[i];
            if (button.activeSelf)
            {
                button.transform.localPosition = originalPositions[i];
            }
        }
    }

    public void backFromSpellList()
    {
        StartCoroutine(LockInputCoroutine(.5f));
        SpellMenu.GetComponent<Animator>().SetTrigger("CloseSpellTab");
        FindObjectOfType<CameraController>().SwitchToBattleMode();
        PlayerCharacter activePlayer = battleController.activeCharacter as PlayerCharacter;
        ShowActionMenu(activePlayer);

        SpellMenu.SetActive(false);
    }
    public void EnableSpellTab()
    {
        SpellMenu.SetActive(true);
        ResetAllButtonPositions();
        StartCoroutine(LockInputCoroutine(.5f));
        SpellMenu.GetComponent<Animator>().SetTrigger("OpenSpellTab");
        PlayerCharacter activePlayer = battleController.activeCharacter as PlayerCharacter;


        // Check if the active character is a player and has stats associated with it.
        if (activePlayer != null && activePlayer.characterStats is PlayerStats playerStats)
        {

            PopulateSpellButtons(activePlayer.characterStats as PlayerStats);
            // Select the first button
            UnityEngine.EventSystems.EventSystem.current.SetSelectedGameObject(SpellMenuButton[0]);
        }
    }
    public void PopulateSpellButtons(PlayerStats playerStats)
    {
        // Populate the spell buttons
        for (int i = 0; i < SpellMenuButton.Count; i++)
        {
            if (i < playerStats.spellList.Count) // If there is a corresponding spell
            {
                SetupSpellButton(SpellMenuButton[i], playerStats.spellList[i]);
                SpellMenuButton[i].SetActive(true); // Enable the button

            }
            else
            {
                SpellMenuButton[i].SetActive(false); // Disable the button if no corresponding spell
            }
        }
    }

    private void SetupSpellButton(GameObject button, Spell spellData)
    {
        // Set the spell name
        Transform spellNameTransform = button.transform.Find("SpellName");  // assuming the child object's name is "SpellName"
        if (spellNameTransform)
        {
            TextMeshProUGUI spellNameText = spellNameTransform.GetComponent<TextMeshProUGUI>();
            if (spellNameText)
            {
                spellNameText.text = spellData.spellName;
            }
        }

        // Set the mana cost
        Transform manaCostTransform = button.transform.Find("ManaCost");
        if (manaCostTransform)
        {
            TextMeshProUGUI manaCostText = manaCostTransform.GetComponent<TextMeshProUGUI>();
            if (manaCostText)
            {
                manaCostText.text = spellData.manaCost.ToString();
            }
        }
        Transform flameIconTransform = button.transform.Find("FlameIcon");
        if (flameIconTransform)
        {
            Image flameIconSpriteRenderer = flameIconTransform.GetComponent<Image>();
            UIManager uiManager = FindObjectOfType<UIManager>();

            flameIconSpriteRenderer.sprite = uiManager.GetSpriteForFireType(spellData.fireType);

        }

    }
    public void ShowActionMenu(CharacterBase activeCharacter)
    {
        if (!activeCharacter) return;

        StartCoroutine(AlignUIWithCenter(actionMenu, activeCharacter.gameObject));
        actionMenu.SetActive(true);
        UpdateItemCount();
        actionMenu.GetComponentInChildren<Animator>().SetTrigger("OpenActionMenu");
        attackButton.Select();  // This makes the attackButton the currently selected button.


    }
    public void HideActionMenu()
    {
        actionMenu.GetComponentInChildren<Animator>().SetTrigger("ForceClose");
        actionMenu.SetActive(false);
    }

    public void UpdateItemCount()
    {
        // Find the HealItemCount GameObject within the ActionMenu
        Transform healItemCountTransform = actionMenu.GetComponentInChildren<Animator>().GameObject().transform.Find("HealItemCount");
        if (healItemCountTransform != null)
        {

            // Find the ItemCount TextMeshProUGUI within HealItemCount
            TextMeshProUGUI itemCountText = healItemCountTransform.Find("ItemCount").GetComponent<TextMeshProUGUI>();
            if (itemCountText != null)
            {
                // Assuming you have access to the PartyManager instance
                PartyManager partyManager = battleController.partyManager; // Or however you access your PartyManager
                if (partyManager != null)
                {
                    Debug.Log("changing text");
                    itemCountText.text = partyManager.currentHealingItemCount.ToString();
                }
            }
        }
    }




}
