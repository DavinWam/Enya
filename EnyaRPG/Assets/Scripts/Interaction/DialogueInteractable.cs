using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class DialogueInteractable : MonoBehaviour, IInteractable
{
    public string[] interactText;
    public TextMeshPro interactionText;

    public TextMeshProUGUI textMeshProUGUI;
    public float conversationDuration = 1f;
    private IEnumerator coroutine;
    private bool isTalking = false;
    private int currentTextIndex = 0;
    private GameObject canvas;
    public Transform mainCamera;
    public void Interact(Transform interactorTransform)
    {
        currentTextIndex = 0;
        if (!isTalking)
        {
            interactionText.transform.position = this.transform.position + new Vector3(0, 2f, 0);
            interactionText.gameObject.SetActive(true);
            canvas = gameObject.transform.GetChild(1).gameObject;
            canvas.SetActive(true);
            textMeshProUGUI.gameObject.SetActive(true);

            // Set the initial text

            isTalking = true;
            coroutine = ConversationCoroutine(conversationDuration);
            StartCoroutine(coroutine);
        }
    }

    public void showDialogue()
    {

    }

    private IEnumerator ConversationCoroutine(float waitTime)
    {
        string currentText = GetInteractText();

        for (int i = 0; i <= currentText.Length; i++)
        {
            textMeshProUGUI.text = currentText.Substring(0, i);
            if (i == currentText.Length)
            {
                yield return new WaitForSeconds(0.05f);
            }
            else
            {
                yield return new WaitForSeconds(0.03f); // Adjust the delay between characters

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
            foreach (BoxCollider collider in GetComponentsInChildren<BoxCollider>())
            {
                collider.enabled = false;
            }
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
        interactionText.transform.position = transform.position + transform.up;
        interactionText.gameObject.SetActive(!isTalking);
    }

    public void removeText()
    {
        interactionText.gameObject.SetActive(false);
        Debug.Log("HERE!");
    }
    private void Update()
    {
        if (isTalking)
        {
            if (Input.GetKeyDown(KeyCode.Return))
            {
                SkipToNextDialogue();
            }
            else if (Input.GetKeyDown(KeyCode.Escape) || (Input.GetKeyDown(KeyCode.Q)))
            {
                ExitDialogue();
            }
        }
    }

    private void SkipToNextDialogue()
    {
        StopCoroutine(coroutine);
        currentTextIndex++;

        if (currentTextIndex < interactText.Length)
        {
            coroutine = ConversationCoroutine(conversationDuration);
            StartCoroutine(coroutine);
        }
        else
        {
            ExitDialogue();
        }
    }

    private void ExitDialogue()
    {
        StopCoroutine(coroutine);
        canvas.SetActive(false);
        textMeshProUGUI.gameObject.SetActive(false);
        isTalking = false;
        foreach (BoxCollider collider in GetComponentsInChildren<BoxCollider>())
        {
            collider.enabled = false;
        }
    }


}
