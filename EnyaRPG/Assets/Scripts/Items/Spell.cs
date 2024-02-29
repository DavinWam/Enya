using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.VFX;
public enum EffectType
{
    HEAL,
    AOE_HEAL,
    ADJACENT_AOE_DAMAGE,
    SINGLE_TARGET_DAMAGE,
    MULTIHIT_TARGET_DAMAGE,
    AOE_DAMAGE,
    BUFF,
    DEBUFF,
    AOE_BUFF,
    SPAWN
}
public enum SpellAnimationType
{
    None,
    MoveToTarget,
    self,
    SpawnAtTarget,
    SpawnAtTargetWithOffset
}
[CreateAssetMenu(menuName = "Spells/Spell")]

public class Spell : ScriptableObject
{
    // Enum representing the different effects a spell can have


    [Header("Basic Info")]
    public string spellName; // The name of the spell.
    public string description; // A brief description or lore about the spell.

    [Header("Spell Properties")]
    public int manaCost; // The amount of mana required to cast the spell.
    public float basePower; // Represents the base strength or effectiveness of the spell.
    public int unlockLevel; // Level required to use this spell.
    public int numHits; // Number of hits applied for any given attack.
    public FireType fireType; // Fire type of the spell.
    public GameObject fireIcon;
   
    public StatusEffect applySelf;
    public StatusEffect applyTarget;
    public IPreDamageCondition preDamageConditions;
    public IPostDamageCondition postDamageConditions;

    public EffectType effectType; // Effect type of the spell.

    [Header("Prefab")]
    public GameObject spellPrefab; // Assign this in the Unity Inspector
    //public ParticleSystem impactEffect; // The visual effect that plays on impact.
    public SpellAnimationType spellAnimationType;
    [Header("Animation Settings")]
    public AnimationCurve xCurve; // Controls the horizontal movement
    public AnimationCurve yCurve;   // Controls the vertical movement


    public Vector3 vfxSpawnOffset; // Used if the animation type involves an offset.

    public IEnumerator PlaySpellEffect(CharacterBase user, CharacterBase target)
    {
        GameObject vfxInstance = null;
        VisualEffect visualEffect = null; 
        float effectLifetime;

        switch (spellAnimationType)
        {
            case SpellAnimationType.MoveToTarget:
                // Instantiate VFX prefab at user position and move to target
                if(target == null) { break; }
                vfxInstance = Instantiate(spellPrefab, user.transform.position, Quaternion.identity);
                // Then move the prefab instance to the target
                visualEffect = vfxInstance.GetComponent<VisualEffect>();
                effectLifetime = visualEffect.GetFloat("lifetime");
              //   FindObjectOfType<CameraController>().ShiftCameraForAttack(user,effectLifetime);
                // Now move the effect with the calculated speed
                yield return MoveEffectToTarget(vfxInstance, user.transform.position,target.transform.position, effectLifetime);

                Debug.Log("move");
                vfxInstance.GetComponent<VisualEffect>().Stop();
                yield break;
            case SpellAnimationType.SpawnAtTarget:
                // Instantiate VFX prefab directly at target position
                if(target == null) { break; }
                vfxInstance = Instantiate(spellPrefab, target.transform.position, Quaternion.identity);
                // Then move the prefab instance to the target
                visualEffect = vfxInstance.GetComponent<VisualEffect>();
                effectLifetime = visualEffect.GetFloat("lifetime");
              //   FindObjectOfType<CameraController>().ShiftCameraForAttack(user,effectLifetime);
                break;

            case SpellAnimationType.SpawnAtTargetWithOffset:
                // Instantiate VFX prefab at target position with an offset
                if(target == null) { break; }
                vfxInstance = Instantiate(spellPrefab, target.transform.position + vfxSpawnOffset, Quaternion.identity);
                // Then move the prefab instance to the target
                visualEffect = vfxInstance.GetComponent<VisualEffect>();
                effectLifetime = visualEffect.GetFloat("lifetime");
              //  FindObjectOfType<CameraController>().ShiftCameraForAttack(user,effectLifetime);
                break;
            case SpellAnimationType.self:
                //instantiate at user of the spell
                if(target == null) { break; }
                vfxInstance = Instantiate(spellPrefab, user.transform.position, Quaternion.identity);
                // Then move the prefab instance to the target
                visualEffect = vfxInstance.GetComponent<VisualEffect>();
                effectLifetime = visualEffect.GetFloat("lifetime");
                // FindObjectOfType<CameraController>().ShiftCameraForAttack(user,effectLifetime);
                break;
            case SpellAnimationType.None:
                // No visual effect is instantiated
                Debug.Log("no animation");
                break;
        }
        if (vfxInstance != null)
        {
            SpellEffectTrigger spellEffectTrigger = vfxInstance.GetComponent<SpellEffectTrigger>();


            // Wait for the animation to complete
            visualEffect = vfxInstance.GetComponent<VisualEffect>();
            yield return new WaitForSeconds(visualEffect.GetFloat("lifetime"));
            FindObjectOfType<CameraController>().SwitchToBattleMode();
            visualEffect.Stop();
            if(spellEffectTrigger){
                spellEffectTrigger.DestroyDelay();
            }
            

        }

        yield break;
    }

    private IEnumerator MoveEffectToTarget(GameObject effectInstance, Vector3 startPosition, Vector3 endPosition,  float effectLifetime)
    {
        float startTime = Time.time;
        float endTime = startTime + effectLifetime;
        float currentTime = startTime;

        while (currentTime < endTime)
        {
            float normalizedTime = (currentTime - startTime) / effectLifetime; // Normalize time to [0, 1]
            float xCurveValue = xCurve.Evaluate(normalizedTime); // Get X curve value
            float yCurveValue = yCurve.Evaluate(normalizedTime); // Get Y curve value

            // Calculate new position
            float newX = Mathf.Lerp(startPosition.x, endPosition.x, xCurveValue);
            float newY = startPosition.y + yCurveValue * (endPosition.x - startPosition.x); // Apply Y offset

            effectInstance.transform.position = new Vector3(newX, newY, effectInstance.transform.position.z);

            // Wait for next frame
            yield return null;
            currentTime = Time.time;
        }

        // Ensure the effect is at the end position at the end of the effect
        effectInstance.transform.position = endPosition;
    }






}


