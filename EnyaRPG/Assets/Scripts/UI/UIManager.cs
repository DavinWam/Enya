using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI; // Assuming you're using Unity's UI system

public class UIManager : MonoBehaviour
{
    // Enum representing the different UI states
    public enum PageState
    {
        OVERWORLD,
        RPG_MENU,
        BATTLE
    }

    // Current state of the UI
    public PageState currentUIState;

    // Specific UI components for different states might be added here.
    // Example:
    public GameObject overworldUI;
    public GameObject rpgMenuUI;
    public GameObject battleUI;
    private Coroutine exitRPGMENU = null;
    public Button partyButton;
    [System.Serializable]
    public struct FireTypeSpriteMapping
    {
        public FireType fireType;
        public Sprite sprite;
    }

    public List<FireTypeSpriteMapping> fireTypeSprites;
    public Dictionary<FireType, Sprite> fireTypeToSprite = new Dictionary<FireType, Sprite>();

    private void Awake()
    {
        foreach (var mapping in fireTypeSprites)
        {
            fireTypeToSprite[mapping.fireType] = mapping.sprite;
        }
    }
    void Start()
    {
        //loading battle raw is causing some lag, so doing it at start to keep it in memory
        ToggleUIState(PageState.BATTLE);
        ToggleUIState(PageState.OVERWORLD);
        //there should be no case where you aren't starting the game in the overworld: m=
        //WILL CHANGE IF WE ADD A TITLE SCREEN
        //ToggleUIState(PageState.OVERWORLD);
    }
    private void Update()
    {
        // Listening for specific key presses to change UI state
        // For instance, toggling with the "Tab" key
        if (currentUIState == PageState.OVERWORLD && (Input.GetKeyDown(KeyCode.Tab)))
        {
            ToggleUIState(PageState.RPG_MENU);
        }



        // if(currentUIState == PageState.RPG_MENU && Input.GetKeyDown(KeyCode.Escape) )
        // {
        //     ExitRPGMenu();

        // }


        // You can expand on this for other key commands and states
    }
    public Sprite GetSpriteForFireType(FireType type)
    {
        // Check if the dictionary contains the given FireType.
        if (fireTypeToSprite.ContainsKey(type))
        {
            return fireTypeToSprite[type];
        }
        else if(type == FireType.None)
        {
            return null;
        }else{
            Debug.LogWarning("No sprite assigned for FireType: " + type.ToString());
            return null;  // Or return a default sprite if you have one
        }
    }

    //this is for buttons to use
    public void ExitRPGMenu()
    {
        ToggleUIState(PageState.OVERWORLD);
        // RPGPanel panel= FindObjectOfType<RPGPanel>();
        // panel.Close();
        // exitRPGMENU = StartCoroutine(WaitForExitAnimation(panel.GetComponent<Animator>()));
    }

    private IEnumerator WaitForExitAnimation(Animator rpgPanelAnimator)
    {
        // Ensure the animator is not null and is playing the "main" animation
        if (rpgPanelAnimator != null && rpgPanelAnimator.GetCurrentAnimatorStateInfo(0).IsName("main"))
        {
            // Wait for the length of the "main" animation
            yield return new WaitForSeconds(rpgPanelAnimator.GetCurrentAnimatorStateInfo(0).length);
        }

        // Call ToggleUIState after the animation has finished
        ToggleUIState(PageState.OVERWORLD);
        exitRPGMENU = null;
    }

    /// <summary>
    /// Toggle the UI state to the given state. Activate relevant UI components while deactivating others.
    /// </summary>
    /// <param name="state">The UIState to transition to.</param>
   public void ToggleUIState(PageState state)
    {
        currentUIState = state;
        PlayerController playerController = FindObjectOfType<PlayerController>();

        switch (currentUIState)
        {
            case PageState.OVERWORLD:
                // Activate overworld UI components and deactivate others
                LockMouse();
                overworldUI.SetActive(true);
                overworldUI.GetComponent<OverworldUI>().Undisable();                
                rpgMenuUI.SetActive(false);
                battleUI.SetActive(false);
                
                // Enable player movement
                if (playerController)
                    playerController.enabled = true;

                break;

            case PageState.RPG_MENU:
                // Activate RPG menu components and deactivate others
                UnlockMouse();
                rpgMenuUI.SetActive(true);
                FindObjectOfType<RPGPanel>().GetComponent<Animator>().SetTrigger("forceMain");
                overworldUI.GetComponent<OverworldUI>().Disable();
                battleUI.SetActive(false);
                partyButton.Select();


                if (state == PageState.RPG_MENU)
                {
                    rpgMenuUI.SetActive(true);
                    Animator rpgMenuAnimator = rpgMenuUI.GetComponent<Animator>();
                    rpgMenuAnimator.Play("main", -1, 0f); // Reset to default state
                }

                // Disable player movement
                if (playerController)
                    playerController.enabled = false;

                break;

            case PageState.BATTLE:
                // Activate battle UI components and deactivate others
                LockMouse();
                battleUI.SetActive(true);
                overworldUI.GetComponent<OverworldUI>().ForceClose();
                rpgMenuUI.SetActive(false);

                break;

            default:
                Debug.LogError("Unhandled UIState: " + currentUIState);
                break;
        }
    }
    public void StartBattleUI()
    {
        ToggleUIState(PageState.BATTLE);
    }

    private void UnlockMouse()
    {
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    private void LockMouse()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }
}
