using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InteractableChest : MonoBehaviour, IInteractable
{
    public GameData gameData; // Reference to the GameData containing the PartyManager
    public SoundManager soundManager;
    public bool hasOpened = false;
    public ParticleSystem openEffect; // Particle effect to play when the chest is opened
    private Animator animator; // Animator to play the chest opening animation
    public GameObject icon; // Text to display when the chest is interactable
    public List<Gear> rewards;
    private AudioSource audioSource; // AudioSource component
    void Start()
    {
        animator = GetComponent<Animator>();
        audioSource = GetComponent<AudioSource>();
    }

    public void Interact(Transform interactorTransform)
    {
        if(hasOpened) return;
        // Play the open chest animation
        animator.SetTrigger("OpenChest");
       PlayOpenSound();
        // Turn on the particle effect
        openEffect.Play();

        // Add the chest to the inventory
        gameData.partyManager.AddToInventory(rewards);


        StartCoroutine(StopAnimations());
        hasOpened = true;
        removeText();
        this.enabled = false;
    }
    private IEnumerator StopAnimations(){
        yield return new WaitForSeconds(4f);
        removeText();
        openEffect.Stop();

    }
    public string GetInteractText()
    {
        return "Open Chest";
    }
    private void PlayOpenSound()
    {
        if (audioSource != null )
        {
            audioSource.volume = 1f;//soundManager.GetVolume();
            audioSource.Play();
        }
    }

    public Transform GetTransform()
    {
        return this.transform;
    }

    public void displayText()
    {
        if(hasOpened) return;

        icon.SetActive(true);
    }

    public void removeText()
    {
        icon.SetActive(false);
    }


}
