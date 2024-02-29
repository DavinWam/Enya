using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class Monologue : MonoBehaviour, IInteractable
{
    public string[] interactText;
    public TextMeshPro interactionText;

    public TextMeshProUGUI textMeshProUGUI;
    public float conversationDuration = 5.0f;
    private IEnumerator coroutine;
    private bool isTalking = false;
    private int currentTextIndex = 0;
    private GameObject canvas;
    public Transform mainCamera;
    public void Interact(Transform interactorTransform)
    {

    }

    public void showDialogue()
    {

    }

    private IEnumerator ConversationCoroutine(float waitTime)
    {
        string currentText = GetInteractText();

        for (int i = 0; i < currentText.Length; i++)
        {
            textMeshProUGUI.text = currentText.Substring(0, i);
            if (currentText[i]== '.')
            {
                yield return new WaitForSeconds(0.45f);
            } else
            {
                yield return new WaitForSeconds(0.05f); // Adjust the delay between characters

            }
        }

        yield return new WaitForSeconds(waitTime);

        // Show the next text or hide the chat bubble if there are no more texts
        currentTextIndex++;
        if (currentTextIndex < interactText.Length)
        {
            StartCoroutine(ConversationCoroutine(conversationDuration));
        }
        else
        {
            canvas.SetActive(false);

            textMeshProUGUI.gameObject.SetActive(false);
            isTalking = false;
        }
    }

    public string GetInteractText()
    {
        if (interactText.Length == 0)
            return string.Empty;

        return interactText[currentTextIndex];
    }

    public Transform GetTransform()
    {
        return transform;
    }

    public void displayText()
    {
        currentTextIndex = 0;
            Debug.Log("TERERT");
            canvas = gameObject.transform.GetChild(0).gameObject;
            canvas.SetActive(true);
            textMeshProUGUI.gameObject.SetActive(true);

            // Set the initial text

            isTalking = true;
            coroutine = ConversationCoroutine(conversationDuration);
            StartCoroutine(coroutine);
        
    }

    public void removeText()
    {
        canvas.SetActive(false);
        StopAllCoroutines();

    }

}
