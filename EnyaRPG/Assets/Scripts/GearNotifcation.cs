using UnityEngine;

public class GearNotification : MonoBehaviour
{
    public Animator animator; // Reference to the Animator component

    void Start()
    {

        // Destroy the game object after the animation ends
        Destroy(gameObject, 5f);
    }
}
