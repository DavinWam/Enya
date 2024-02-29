using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class InteractableFireUnlocker : MonoBehaviour, IInteractable
{
    public GameData gameData;
    public FireType fireTypeToUnlock;
    public GameObject icon;
    public ParticleSystem unlockEffect;
    private Animator animator;
    private AudioSource audioSource;

    public bool hasOpened = false;

    void Start()
    {
        animator = GetComponent<Animator>();
        audioSource = GetComponent<AudioSource>();
        gameData.unlockedFireTypes = new List<FireType>();
        gameData.partyManager.activePartyMembersPrefabs = new List<GameObject>();
        gameData.partyManager.generalPartyMembersPrefabs = new List<GameObject>();
        gameData.partyManager.inactivePartyMembersPrefabs = new List<GameObject>();
        gameData.partyManager.cloneStats = new List<PlayerStats>();
    }

    public void Interact(Transform interactorTransform)
    {
        if (gameData.unlockedFireTypes.Contains(fireTypeToUnlock)) return;
        if(hasOpened) return;

        // Unlock the fire type
        gameData.unlockedFireTypes.Add(fireTypeToUnlock);
        unlockEffect.Play();
        PlayUnlockSound();
        animator.SetTrigger("OpenChest");

        StartCoroutine(ManagePartyMembersAfterUnlock());
        icon.SetActive(false);
        this.enabled = false;
    }

    private IEnumerator ManagePartyMembersAfterUnlock()
    {
        int memberIndex = -1;

        // Find the index of the party member with the matching FireType
        for (int i = 0; i < gameData.potentialPartyMembersStats.Count; i++)
        {
            if (gameData.potentialPartyMembersStats[i].fireType == fireTypeToUnlock)
            {
                memberIndex = i;
                break;
            }
        }

        // Check if a member with the required FireType was found
        if (memberIndex != -1)
        {
            GameObject memberPrefab = gameData.potentialPartyMembersList[memberIndex];
            // Add the member to the general party members list
            gameData.partyManager.generalPartyMembersPrefabs.Add(memberPrefab);



            PlayerStats cloneStats = gameData.potentialPartyMembersStats[memberIndex].Clone();
            int level = gameData.partyManager.playerStats.level;
            Debug.Log(level);
            while(cloneStats.level < level)
            {
                cloneStats.LevelUp();
            }
            Debug.Log(cloneStats.level);
            // Clone the member's stats and then remove it from potential members
            gameData.partyManager.cloneStats.Add(cloneStats);



            // Add the member to either active or inactive party members list
            if (gameData.partyManager.activePartyMembersPrefabs.Count < 3)
            {
                gameData.partyManager.activePartyMembersPrefabs.Add(memberPrefab);
            }
            else
            {
                gameData.partyManager.inactivePartyMembersPrefabs.Add(memberPrefab);
            }
            
            FindObjectOfType<OverworldUI>().DisplayCharacterInfo(memberPrefab);
        }

        yield break;
    }
    private void PlayUnlockSound()
    {
        // Implement sound play logic here
    }

    public string GetInteractText()
    {
        return "Unlock Fire Type";
    }

    public Transform GetTransform()
    {
        return this.transform;
    }

    public void DisplayText()
    {
        icon.SetActive(true);
    }

    public void RemoveText()
    {
        icon.SetActive(false);
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
