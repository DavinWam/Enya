using System.Collections;
using TMPro;
using UnityEngine;

public class ArrowScript : MonoBehaviour
{
    public string interactText;
    private TextMeshPro interactionText;
    public bool shouldBeDestroyed = true;
    private void OnTriggerEnter(Collider other)
    {
        DisplayText();
    }

    public void DisplayText()
    {
        StartCoroutine(DisplayTextForSeconds(3f)); // Display the text for 5 seconds
    }

    private IEnumerator DisplayTextForSeconds(float duration)
    {
        interactionText = new GameObject("InteractionText", typeof(TextMeshPro)).GetComponent<TextMeshPro>();
        interactionText.transform.position = transform.position + new Vector3(0,7f,0);
        interactionText.text = interactText;
        interactionText.gameObject.SetActive(true);
        interactionText.fontSize = 10;
        interactionText.alignment = TextAlignmentOptions.Center;
        yield return new WaitForSeconds(duration);

        if (shouldBeDestroyed)
        {
            Destroy(gameObject);
        } else
        {
            GetComponent<MeshRenderer>().enabled = false;
            yield return new WaitForSeconds(duration);
            GetComponent<MeshRenderer>().enabled = true;

        }

        interactionText.gameObject.SetActive(false);
        Destroy(interactionText.gameObject); // Optionally destroy the text object after hiding it
    }
}