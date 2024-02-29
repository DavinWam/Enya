using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class StoryLineInteractable : MonoBehaviour, IInteractable
{
    [SerializeField] private string[] interactText;

    public TextMeshPro interactionText;
    public float conversationDuration = 5.0f;
    private GameObject ChatBubble;
    private IEnumerator coroutine;
    private bool isTalking = false;
    private TextMeshPro textMeshPro;
    private int currentTextIndex = 0;
    public Transform mainCamera;
    public void Interact(Transform interactorTransform)
    {
        currentTextIndex = 0;
        if (!isTalking)
        {
            interactionText.transform.position = this.transform.position + new Vector3(0, 2f, 0);
            interactionText.gameObject.SetActive(true);
            ChatBubble = gameObject.transform.GetChild(0).gameObject;
            textMeshPro = ChatBubble.transform.Find("text").GetComponent<TextMeshPro>();

            // Set the initial text
            textMeshPro.text = GetInteractText();

            ChatBubble.transform.position = mainCamera.position + new Vector3(-0.12f,-6.5f,1.2f);
            textMeshPro.transform.position = ChatBubble.transform.position + new Vector3(0.2f, 1.1f, 0.13f);
            ChatBubble.SetActive(true);
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
            textMeshPro.text = currentText.Substring(0, i);
            yield return new WaitForSeconds(0.05f); // Adjust the delay between characters
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
            ChatBubble.gameObject.SetActive(false);
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
        interactionText.transform.position = transform.position + transform.up;
        interactionText.gameObject.SetActive(!isTalking);
    }

    public void removeText()
    {
        interactionText.gameObject.SetActive(false);
        Debug.Log("HERE!");
    }

}
