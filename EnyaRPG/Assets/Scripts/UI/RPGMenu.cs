using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
public enum UIState
{
    PARTY,
    GEAR,
    SPELLS,
    ITEMS,
    OPTIONS,
    MAIN,
    // ... any other states ...
}

public class RPGMenu : MonoBehaviour
{
    public UIState currentState;
    public GameObject partyPanel;
    public GameObject gearPanel;
    public GameObject spellPanel;
    public GameObject itemsPanel;
    public GameObject optionsPanel;

    public List<GameObject> menuUIElements; // List to keep track of UI elements

    [Header("Navigation")]
    public Button[] menuButtons; // Drag your UI buttons here in the desired navigation order
    private void Start()
    {
        // Initialization: Ensure all panels are closed on start
       // CloseAllPanels();
        SetState(UIState.MAIN);
    }
    public void SetState(UIState state)
    {
        CloseAllPanels(); // Close all panels before opening a new one

        currentState = state;
        OpenPanel(state);
    }

    public void OpenPanel(UIState panelToOpen)
    {
        switch (panelToOpen)
        {
            case UIState.MAIN:
                Open();
                break;
            case UIState.PARTY:
                //partyPanel.Open();
                break;
            case UIState.GEAR:
                //gearPanel.Open();
                break;
            case UIState.SPELLS:
                // spellPanel.Open();
                break;
            case UIState.ITEMS:
                //itemsPanel.Open();
                break;
            case UIState.OPTIONS:
                //optionsPanel.Open();
                break;
        }
    }
    public void Open()
    {
        
        gameObject.SetActive(true);
        foreach (GameObject element in menuUIElements)
        {
            element.SetActive(true);
        }
        SelectButton(menuButtons[0]);
    }

    public void Close()
    {
        foreach (GameObject element in menuUIElements)
        {
            element.SetActive(false);
        }
        gameObject.SetActive(false);
    }
    public void CloseAllPanels()
    {
        Close();
        // partyPanel.Close();
        // gearPanel.Close();
        // spellPanel.Close();
        // itemsPanel.Close();
        // optionsPanel.Close();
        // Close other panels if there are more
    }

    public void TogglePanel(UIState panelToToggle)
    {
        if (currentState == panelToToggle)
        {
            CloseAllPanels();
            currentState = UIState.MAIN; // Set to a default state
        }
        else
        {
            SetState(panelToToggle);
        }
    }

    private void SelectButton(Button button)
    {
        EventSystem.current.SetSelectedGameObject(button.gameObject);
    }
}

// Note: The panel classes (PartyPanel, GearPanel, etc.) should have Open, Close, and possibly UpdatePanel methods for the above implementation to work correctly.
