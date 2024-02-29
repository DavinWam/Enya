using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    public float sprintSpeed = 4.0f; // Sprint speed
    public float movementSpeed = 2.0f;
    public float currentSpeed;
    private CharacterController characterController;
    public SoundManager soundManager;
    private Vector3 playerVelocity = Vector3.zero;
    private bool groundedPlayer;
    private float gravityValue = -10f;
    private bool isInteracting;
    private Animator animator; // Reference to the Animator component
    public bool isRunning = false; // Check if the player is running
    public AudioSource audioSource; // AudioSource component
    private bool isPlayingRunningSound = false;
    public float checkDistance = 1.0f; // Distance to check ahead for ledges or non-walkable surfaces


    //private Interactable currentInteractable;

    // Start is called before the first frame update
    void Start()
    {
        characterController = GetComponent<CharacterController>();
        animator = GetComponentInChildren<Animator>();
        audioSource = GetComponent<AudioSource>();
        soundManager.currentAreaAudioSource = null;
    }

    // Update is called once per frame
    void Update()
    {
        Move();

    }
    public Vector3 GetMovementDirection()
    {
        return new Vector3(Input.GetAxis("Horizontal"), 0, Input.GetAxis("Vertical")).normalized;
    }

    public void Move()
    {
        Transform dust = transform.Find("dustps");

        groundedPlayer = characterController.isGrounded;

        if (groundedPlayer && playerVelocity.y < 0)
        {
            playerVelocity.y = 0f;
        }

        Vector3 move = GetMovementDirection();
      if (move != Vector3.zero)
        {
            // Adjust the starting point of the raycast when on a slope
            Vector3 checkPoint = transform.position + (groundedPlayer ? Vector3.up * 0.1f : Vector3.zero) + move.normalized * checkDistance;

            // Perform the raycast check
            RaycastHit hit;
            bool hitSomething = Physics.Raycast(checkPoint, Vector3.down, out hit);

            // // Check for slopes
            // if (hitSomething && Vector3.Angle(Vector3.up, hit.normal) <= characterController.slopeLimit)
            // {
            //     // On a walkable slope, adjust raycast distance
            //     hitSomething = Physics.Raycast(checkPoint, Vector3.down, out hit, checkDistance);
            // }

            if (hitSomething && (hit.collider.CompareTag("NotWalkable") || !hit.collider))
            {
                // Prevent movement if hitting a non-walkable surface or detecting a ledge
                return;
            }
        }

        // Check if the player is holding the sprint key (left shift)
        bool isSprinting = Input.GetKey(KeyCode.LeftShift);
        currentSpeed = isSprinting ? sprintSpeed : movementSpeed;

        characterController.Move(move * Time.deltaTime * currentSpeed);

        SpriteRenderer spriteRenderer = gameObject.GetComponentInChildren<SpriteRenderer>();
        // Flip sprite based on movement direction
        if (move.x != 0f || move.z != 0f) // Check if there is any movement
        {
            isRunning = true;
            if(groundedPlayer){
                dust.gameObject.SetActive(true);
             }//else{
            //     dust.gameObject.SetActive(false);
            // }
            

            if (move.x > 0f) // Moving right
            {
                spriteRenderer.flipX = false;
                dust.rotation = Quaternion.Euler(0f, 180f, 0f); // Right
            }
            else if (move.x < 0f) // Moving left
            {
                spriteRenderer.flipX = true;
                dust.rotation = Quaternion.Euler(0f, 0f, 0f); // Left
            }

            if (move.z > 0f) // Moving up
            {
                dust.rotation = Quaternion.Euler(0f, 90f, 0f); // Up
            }
            else if (move.z < 0f) // Moving down
            {
                dust.rotation = Quaternion.Euler(0f, 270f, 0f); // Down
            }

            // Handle diagonal movement
            if (move.x != 0f && move.z != 0f)
            {
                // Calculate the angle for the dust based on the direction of movement
                float angle = Mathf.Atan2(move.z, move.x) * Mathf.Rad2Deg;
                
                // Correct the angle for specific diagonal directions
                if (move.x < 0f && move.z > 0f) // AS movement
                {
                    angle += 180f; // Invert angle
                }
                else if (move.x > 0f && move.z < 0f) // WD movement
                {
                    angle += 180f; // Invert angle
                }
                dust.rotation = Quaternion.Euler(0f, angle+ 90, 0f);
            }
        }
        else
        {
            isRunning = false;
            dust.gameObject.SetActive(false);
        }


        // Handle animations
        if (isRunning)
        {
            animator.SetBool("IsRunning", true);
        }
        else
        {
            animator.SetBool("IsRunning", false);
        }

        playerVelocity.y += gravityValue * Time.deltaTime;
        characterController.Move(playerVelocity * Time.deltaTime);
        
        if (isRunning)
        {
            if (!isPlayingRunningSound)
            {
                PlayRunningSound();
            }
        }
        else
        {
            if (isPlayingRunningSound)
            {
                StopRunningSound();
            }
        }

    }

    private void PlayRunningSound()
    {
        if (audioSource != null)
        {
            audioSource.volume = soundManager.GetVolume()+.2f;
            audioSource.loop = true;
            audioSource.Play();
            isPlayingRunningSound = true;
        }
    }

    private void StopRunningSound()
    {
        if (audioSource != null)
        {
            audioSource.Stop();
            isPlayingRunningSound = false;
        }
    }
}
