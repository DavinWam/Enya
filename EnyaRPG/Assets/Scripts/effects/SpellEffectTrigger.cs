using UnityEngine;

public class SpellEffectTrigger : MonoBehaviour
{
    public CameraController cameraController; // Reference to the CameraController
    public float shakeInterval = 1f; // Interval between shakes
    private bool hasShaken = false;
    private float nextShakeTime;


    
    private void Start()
    {
        cameraController = FindObjectOfType<CameraController>();
        // Initialize the next shake time
        nextShakeTime = Time.time;
        
    }
    public void DestroyDelay(){
        GetComponent<Collider>().enabled = false;
        Destroy(this, 60);
    }
    private void OnTriggerEnter(Collider other)
    {
        BattleController bc = FindObjectOfType<BattleController>();

        Debug.Log($"tag:{other.tag}");
        // Check if the collider tag is opposite to the active character's type
        if (!hasShaken && IsOppositeTag(bc.activeCharacter, other.tag))
        {
            // Trigger the camera shake
            cameraController.TriggerShake();

            // Process actions
            bc.ProcessActions();

            hasShaken = true;
            // Set the next shake time
            nextShakeTime = Time.time + shakeInterval;
        }
    }

    private void OnTriggerStay(Collider other)
    {
        BattleController bc = FindObjectOfType<BattleController>();

        // Check if the collider tag is opposite to the active character's type and time for next shake has elapsed
        if (Time.time >= nextShakeTime && IsOppositeTag(bc.activeCharacter, other.tag))
        {
            Debug.Log($"tag:{other.tag}");
            // Trigger the camera shake
            //cameraController.TriggerShake();
            // Update the next shake time
            nextShakeTime = Time.time + shakeInterval;
        }
    }

    private bool IsOppositeTag(CharacterBase activeCharacter, string colliderTag)
    {
        if (activeCharacter is PlayerCharacter)
        {
            return colliderTag.Equals("Enemy");
        }
        else if (activeCharacter is EnemyCharacter)
        {
            return colliderTag.Equals("Player") || colliderTag.Equals("Clone");
        }
        return false;
    }
}
