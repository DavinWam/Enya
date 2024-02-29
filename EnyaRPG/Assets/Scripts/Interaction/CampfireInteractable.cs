using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CampfireInteractable : MonoBehaviour, IInteractable
{
    public List<CharacterBase> characterBases;
    public GameData gameData;
    private bool isActive = false;
    public int healRate = 5;
    public float waitTime = 1f;
    public ParticleSystem healParticles;
    public void displayText()
    {
        characterBases = GetInteractableObject();
        isActive = true;
        if (healParticles != null)
        {
            healParticles.gameObject.SetActive(true);
        }
        
        // Calculate an offset based on the capsule collider dimensions
        CapsuleCollider capsuleCollider = GetComponent<CapsuleCollider>();
        Vector3 respawnOffset = Vector3.back * (capsuleCollider.radius/2f);

        // Set the respawn location in the GameData scriptable object with the offset
        gameData.respawnLocation = transform.position + respawnOffset;


        gameData.partyManager.ReplenishHealingItems();
        StartCoroutine(heal(waitTime, characterBases));
    }

    public IEnumerator heal(float waitTime,List<CharacterBase> characterBases)
    {
        while(isActive)
        {
            yield return new WaitForSeconds(waitTime);

            foreach (CharacterBase characterBase in characterBases)
            {
                ((PlayerStats)(characterBase.characterStats)).RegenerateMana(characterBase.characterStats.GetEffectiveStat(StatType.MANA)*healRate);
                characterBase.characterStats.Heal(characterBase.characterStats.GetEffectiveStat(StatType.HEALTH)*(healRate));
            }
            Debug.Log(gameData.partyManager.cloneStats.ToArray().ToString()); 
            foreach(CharacterStats characterStats in gameData.partyManager.cloneStats){
                Debug.Log("made it here");
                Debug.Log(characterStats.GetEffectiveStat(StatType.HEALTH));
                ((PlayerStats)characterStats).RegenerateMana(characterStats.GetEffectiveStat(StatType.MANA)*healRate);
                characterStats.Heal(characterStats.GetEffectiveStat(StatType.HEALTH)*(healRate));
                Debug.Log(characterStats.GetEffectiveStat(StatType.HEALTH));
            }

        }
        
    }
    public string GetInteractText()
    {
        return "";
    }

    public Transform GetTransform()
    {
        return transform;
    }

    public void Interact(Transform interactorTransform)
    {
        
    }

    public void removeText()
    {
        isActive = false;
        healParticles.gameObject.SetActive(false);
    }

    private List<CharacterBase> GetInteractableObject()
    {
        List<CharacterBase> characterBases = new List<CharacterBase>();
        float interactRange = 8f;
        Collider[] colliderArray = Physics.OverlapSphere(transform.position, interactRange);
        foreach (Collider collider in colliderArray)
        {
            if (collider.TryGetComponent(out CharacterBase characterBase))
            {
                characterBases.Add(characterBase);
            }
        }

        

        return characterBases;
    }
}
