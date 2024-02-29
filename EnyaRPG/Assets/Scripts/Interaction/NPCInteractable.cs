using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class NPCInteractable : MonoBehaviour, IInteractable
{
    [SerializeField] private string interactText;

    public TextMeshPro interactionText;
    public float conversationDuration = 5.0f;
    private GameObject ChatBubble;
    private IEnumerator coroutine;
    private bool isTalking = false;
    private TextMeshPro textMeshPro;

    public void Interact(Transform interactorTransform)
    {
        interactionText.gameObject.SetActive(false);
        ChatBubble = gameObject.transform.GetChild(0).gameObject;
        textMeshPro =  ChatBubble.transform.Find("text").GetComponent<TextMeshPro>();
        textMeshPro.text = GetInteractText();
        ChatBubble.transform.position = transform.position + Vector3.up + Vector3.up;
        ChatBubble.SetActive(true);
        isTalking = true;
        //animator.SetTrigger("Talk");
        coroutine = conversation(conversationDuration);
        Debug.Log("Interact");
        StartCoroutine(coroutine);
    }

    IEnumerator conversation(float waitTime)
    {
        yield return new WaitForSeconds(waitTime);

        ChatBubble.gameObject.SetActive(false);
        isTalking = false;
        yield return null;
    }
    public string GetInteractText()
    {
        return interactText;
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
