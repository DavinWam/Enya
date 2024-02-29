using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class CreditScene : MonoBehaviour
{

    public GameObject confirmationPanel; // Reference to the confirmation panel
    
    public void switchToMenu()
    {
        PlayerController playerController = FindObjectOfType<PlayerController>();
        UnlockMouse();
        // Disable player movement
        if (playerController)
            playerController.enabled = false;
    }
    private void UnlockMouse()
    {
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    // Call this method when the quit button is clicked
    public void ConfirmQuit()
    {
        Application.Quit();
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false; // Quit in the Unity Editor
#endif

    }

    // Call this method from the "Yes" button on the confirmation panel
    public void Quit()
    {
        confirmationPanel.SetActive(true); // Show the confirmation panel
    }

    // Call this method from the "No" button on the confirmation panel
    public void CancelQuit()
    {
        confirmationPanel.SetActive(false); // Hide the confirmation panel
    }


}
