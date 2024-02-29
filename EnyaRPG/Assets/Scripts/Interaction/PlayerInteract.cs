using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerInteract : MonoBehaviour
{
    public float interactRange = 2f;
    public IInteractable interactable1;
    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.E))
        {
            if (interactable1 != null)
            {
                interactable1.Interact(transform);
            }
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if(interactable1 != null){
            interactable1.removeText();
        }
        interactable1 = GetInteractableObject();
        
        if (interactable1 != null)
        {
            interactable1.displayText();
        }
        Debug.Log("Interact1");
        
    }

    private void OnTriggerExit(Collider other)
    {
        if(interactable1 != null){
            interactable1.removeText();   
        }
        interactable1 = null;
    }

    public IInteractable GetInteractableObject()
    {
        Collider[] colliderArray = Physics.OverlapSphere(transform.position, interactRange);
        IInteractable closestInteractable = null;
        float closestDistance = float.MaxValue;

        foreach (Collider collider in colliderArray)
        {
            if (collider.TryGetComponent(out IInteractable interactable))
            {
                float distance = Vector3.Distance(transform.position, interactable.GetTransform().position);
                if (distance < closestDistance)
                {
                    closestDistance = distance;
                    closestInteractable = interactable;
                }
            }
        }

        return closestInteractable;
    }

}
