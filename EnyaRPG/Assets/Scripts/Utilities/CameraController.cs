using System.Collections;
using System.Collections.Generic;
using UnityEngine;


// Enum defining the camera modes
public enum CameraMode
{
    Battle,
    Traversal,
    characterFocus,
    targetCharacter,
    characterAction,
    Attacking,
    Cinematic
}

public class CameraController : MonoBehaviour
{
    // Cinematic mode properties
    private Vector3 cinematicPosition;
    private Quaternion cinematicRotation;
    private float cinematicFOV;


    [Header("Encounter Manager")]
    [SerializeField] private EncounterManager encounterManager;
    // Attributes
    public CameraMode currentMode = CameraMode.Traversal;
    public Vector3 battlePosition;
    public Vector3 traversalPosition;
    public Transform followTarget;
    public float followSpeed = 5.0f;
    public float zoomLevel = 60.0f;
    public float rotationAngle = 0.0f;
    public float teleportDistanceThreshold = 75f; // Distance threshold for teleporting

    [Header("Wobble Effect")]
    public float wobbleMagnitude = 0.1f;  // The extent of the wobble
    public float wobbleFrequency = 1.0f;  // How quickly the camera wobbles
    private float wobbleTime;             // Internal time tracker for the wobble
    [Header("Hit Shake")]
    public float shakeDuration = 0.5f;
    public float shakeMagnitude = 0.7f;
    public float dampingSpeed = 1.0f;

    Vector3 initialPosition;
    float currentShakeDuration = 0f;

    bool isShaking = false;

    public float targetZoom = .2f;
   [Header("Hit Shake")]
    private Vector3 velocity = Vector3.zero; // Required for SmoothDamp
    public float smoothTime = 0.1f; // Time taken to reach the target
    private Vector3 offset;//general offset variable
    public Vector3 spellMenuOffset;
    private void Start()
    {
        if (currentMode == CameraMode.Traversal)
        {
            offset = traversalPosition;
        }
        else
        {
            offset = battlePosition;
        }
        SwitchToTraversalMode();
    }

    private void Update()
    {
        if (currentMode == CameraMode.Cinematic)
        {
            // Apply cinematic settings
            transform.position = Vector3.Lerp(transform.position, cinematicPosition, followSpeed * Time.deltaTime);
            transform.rotation = Quaternion.Lerp(transform.rotation, cinematicRotation, followSpeed * Time.deltaTime);
            Camera.main.fieldOfView = Mathf.Lerp(Camera.main.fieldOfView, cinematicFOV, followSpeed * Time.deltaTime);
        }
    
        
        if (currentMode == CameraMode.Traversal)
            {
                if(followTarget != null)
                {
                     offset = traversalPosition;
                    UpdateTraversalCamera();
                }
               
            }
                    
        if (currentMode == CameraMode.Battle )
        {
            if (isShaking)
            {
                if (currentShakeDuration > 0)
                {
                    transform.localPosition = initialPosition + Random.insideUnitSphere * shakeMagnitude;
                    currentShakeDuration -= Time.deltaTime * dampingSpeed;
                }
                else
                {
                    isShaking = false;
                    transform.localPosition = initialPosition;
                }
            }else{
                AdjustForBattle();
                wobbleTime += Time.deltaTime; // Increment wobble time
            }
        }
        
        if(currentMode == CameraMode.characterAction){
            if (isShaking)
            {

            }
        }

    }

    private void UpdateTraversalCamera()
    {    

        Vector3 desiredPosition = CalculateDesiredPosition();
        // Check the distance to the follow target
        if (Vector3.Distance(transform.position, desiredPosition) > teleportDistanceThreshold)
        {
            // If too far away, teleport the camera to the desired position
            transform.position = desiredPosition;
        }else{
            Vector3 lookaheadPosition = CalculateLookaheadPosition(desiredPosition);
            Vector3 obstructionFreePosition = FindObstructionFreePosition(lookaheadPosition);
            transform.position = Vector3.SmoothDamp(transform.position, obstructionFreePosition, ref velocity, smoothTime);
        }

    }

    private Vector3 CalculateDesiredPosition()
    {
        // Calculate the desired position based on player's position and offset
        return followTarget.position + offset;
    }

    private Vector3 CalculateLookaheadPosition(Vector3 desiredPosition)
    {
        // Adjust for lookahead based on player's movement
        PlayerController playerController = followTarget.GetComponent<PlayerController>();
        if (playerController != null)
        {
            Vector3 movementDirection = playerController.GetMovementDirection();
            Vector3 forwardOffset = movementDirection * playerController.currentSpeed / 3f; // Adjust for desired lookahead distance
            return desiredPosition + forwardOffset;
        }

        return desiredPosition;
    }


    private Vector3 FindObstructionFreePosition(Vector3 desiredPosition)
    {
        
        Vector3 directionToPlayer = (followTarget.position - transform.position).normalized;
        float distanceToPlayer = Vector3.Distance(transform.position, followTarget.position);

        if (distanceToPlayer > teleportDistanceThreshold)
        {
            // Teleport the camera to the desired position if it's too far away
            return desiredPosition;
        }
        // Define the layer mask to ignore specific layers (e.g., IgnoreCamera)
        int layerMask = 1 << LayerMask.NameToLayer("IgnoreCamera");
        layerMask = ~layerMask; // Invert the mask to ignore the specified layer

        // Check for obstructions using raycast with layer mask
        RaycastHit hit;
        if (Physics.Raycast(followTarget.position, (desiredPosition - followTarget.position).normalized, out hit, Vector3.Distance(followTarget.position, desiredPosition), layerMask))
        {
            if (hit.collider.gameObject != followTarget.gameObject)
            {
                // An obstruction is detected, pull in the camera
                desiredPosition = hit.point;
            }
        }


        // No significant obstruction - use desired position
        return desiredPosition;
    }
    public void TriggerCinematicMode(CinematicSettings settings)
    {
        currentMode = CameraMode.Cinematic;
        cinematicPosition = settings.Position;
        cinematicRotation = settings.Rotation;
        cinematicFOV = settings.FieldOfView;
    }


    public IEnumerator HandleOverheatEffects(Vector3 targetPosition)
    {

        // 2. Zoom in on target
        Zoom(-10f,targetPosition); // adjust as needed

        // 3. Move the camera slightly in the X direction towards the target (over 1 second for smoothness)
        Vector3 originalPosition = transform.position;
        Vector3 targetCameraPosition = new Vector3(targetPosition.x + .5f, transform.position.y, transform.position.z);  // adjust the "2f" for desired X offset
        float elapsedTime = 0f;
        float duration = 1f;  // adjust for desired movement speed
        while (elapsedTime < duration)
        {
            transform.position = Vector3.Lerp(originalPosition, targetCameraPosition, (elapsedTime / duration));
            elapsedTime += Time.deltaTime;
            yield return null;
        }

    }
    public void FocusOnActiveCharacter(CharacterBase activeCharacter)
    {
        Bounds characterBounds = new Bounds(activeCharacter.transform.position, Vector3.zero);
        currentMode = CameraMode.characterFocus;
        FrameBoundsFor2DSprite(activeCharacter.transform, .25f); // Duration can be adjusted
    }
    
    private void FrameBoundsFor2DSprite(Transform spriteTransform, float duration)
    {
        // Calculate a suitable distance from the sprite. Adjust this value as needed.
        float distanceFromSprite = 5.0f;

        // Calculate the camera position. This assumes that the sprite is facing along the Y-axis.
        Vector3 cameraPosition = spriteTransform.position - (Vector3.forward * distanceFromSprite) + spellMenuOffset;

        // The camera should look directly at the sprite's position.
        Quaternion cameraRotation = Quaternion.LookRotation(spriteTransform.position - cameraPosition);

        // Use a coroutine to smoothly move the camera.
        StartCoroutine(SmoothMoveCamera(cameraPosition, cameraRotation, duration));
    }
    public IEnumerator FocusOnTarget(CharacterBase target, List<GameObject> objectsToFrame)
    {
        Debug.Log($"starting focus on {target.characterName}");
        if(objectsToFrame.Count == 0) yield break; // Guard clause for empty list


        // Calculate the combined bounds of all objects including the target
        Bounds combinedBounds = new Bounds(target.transform.position, Vector3.zero);
        foreach (GameObject obj in objectsToFrame)
        {
            combinedBounds.Encapsulate(obj.transform.position);
        }

        // Center the bounds on the target
        Vector3 center = target.transform.position+ new Vector3(0,2f,-2);
        combinedBounds.center = center;

        // Move the camera to frame the combined bounds with the target at the center
        if(currentMode != CameraMode.targetCharacter)
        { 
            Debug.Log("normal zoom");
            currentMode = CameraMode.targetCharacter;
            yield return StartCoroutine(FrameBoundsFor2DSprite(combinedBounds, center, targetZoom)); // Duration can be adjusted
        }else{
            Debug.Log("quick zoom");
            yield return StartCoroutine(FrameBoundsFor2DSprite(combinedBounds, center, targetZoom/2f)); // Duration can be adjusted
        }
    }


    private IEnumerator FrameBoundsFor2DSprite(Bounds bounds, Vector3 center, float duration)
    {
        Debug.Log("generating camera frame bounds");
        // Calculate a suitable distance from the bounds. This depends on the size of the bounds.
        float distanceFromBounds = bounds.size.magnitude; // Adjust this value based on your needs

        // Calculate the camera position. This assumes that the sprites are facing along the Y-axis.
        Vector3 cameraPosition = center - (Vector3.forward * distanceFromBounds) +new Vector3(0,1,-2);

        // The camera should look directly at the center position.
        Quaternion cameraRotation = Quaternion.LookRotation(center - cameraPosition);

        // Use a coroutine to smoothly move the camera.
        yield return StartCoroutine(SmoothMoveCamera(cameraPosition, cameraRotation, duration));
    }

    private IEnumerator SmoothMoveCamera(Vector3 targetPosition, Quaternion targetRotation, float duration)
    {
        Debug.Log("moving camera");
        float elapsed = 0f;
        Vector3 startPosition = transform.position + new Vector3(0,0,-1);
        Quaternion startRotation = transform.rotation;

        while (elapsed < duration)
        {
            transform.position = Vector3.Lerp(startPosition, targetPosition, elapsed / duration);
            transform.rotation = Quaternion.Lerp(startRotation, targetRotation, elapsed / duration);
            elapsed += Time.deltaTime;
            yield return null;
        }

        transform.position = targetPosition;
        transform.rotation = targetRotation;
        Debug.Log($"finished movement {transform.position}");
        yield break;
    }

    public IEnumerator ShiftCameraForCharacterTurn(CharacterBase character, float duration)
    {
        // Determine the direction of the shift based on the character type
        Vector3 shiftDirection = (character is PlayerCharacter) ? Vector3.left : Vector3.right;
        Vector3 newPosition = transform.position + shiftDirection * 3.0f; // Adjust the shift magnitude as needed

        // Start the coroutine to move the camera
        yield return StartCoroutine(SmoothMoveCamera(newPosition, transform.rotation, duration));
        currentMode = CameraMode.characterAction;
        StartCoroutine(WobbleCoroutine(newPosition));
        
    }
    private IEnumerator WobbleCoroutine(Vector3 newPosition)
    {
        Vector3 originalPosition = transform.position;

        while (currentMode == CameraMode.characterAction )
        {
            if(isShaking == false){
                // Calculate wobble offset
                float wobbleX = Mathf.Sin(Time.time * wobbleFrequency) * wobbleMagnitude;
                float wobbleY = Mathf.Sin((Time.time + Mathf.PI * 0.5f) * wobbleFrequency) * wobbleMagnitude;

                // Apply wobble offset to the camera position
                transform.position = originalPosition + new Vector3(wobbleX, wobbleY, 0);
            }else
            {
                if (currentShakeDuration > 0)
                {
                    transform.localPosition = newPosition + Random.insideUnitSphere * shakeMagnitude;
                    currentShakeDuration -= Time.deltaTime * dampingSpeed;
                }
                else
                {
                    isShaking = false;
                }
            }


            yield return null;
        }

        // Reset camera position after wobble
        transform.position = originalPosition;
    }

    public void TriggerShake()
    {
        initialPosition = transform.localPosition;
        currentShakeDuration = shakeDuration;
        isShaking = true;
    }
    private Vector3 ComputeWobbleOffset()
    {
        // Calculate wobble using sin function
        float wobbleX = Mathf.Sin(wobbleTime * wobbleFrequency) * wobbleMagnitude;
        float wobbleY = Mathf.Sin((wobbleTime + Mathf.PI * 0.5f) * wobbleFrequency) * wobbleMagnitude; // Offset phase for variety

        // Use the right and up vectors of the camera to calculate the wobble within the plane
        Vector3 wobbleOffset = transform.right * wobbleX + transform.up * wobbleY;

        return wobbleOffset;
    }


    public void SwitchToBattleMode()
    {
        wobbleTime = 0f;  // Reset wobble time
        currentMode = CameraMode.Battle;
        battlePosition = encounterManager.currentEncounter.offset;
        Camera.main.fieldOfView = 65f;
        AdjustForBattle();
    }


    public void AdjustForBattle()
    {
        // Determine the combined bounds of all the GameObjects
        Bounds combinedBounds = new Bounds(encounterManager.currentEncounter.enemyBattlePositions[0], Vector3.zero);

        foreach (Vector3 pos in encounterManager.currentEncounter.enemyBattlePositions)
        {
            combinedBounds.Encapsulate(pos);
        }

        foreach (Vector3 pos in encounterManager.currentEncounter.partyBattlePositions)
        {
            combinedBounds.Encapsulate(pos);
        }

        // Move camera to frame the bounds
        float camDistance = combinedBounds.extents.magnitude;
        Vector3 cameraPosition = combinedBounds.center - transform.forward * camDistance;

        // Add the wobble effect to the desired camera position
        cameraPosition += ComputeWobbleOffset();

        // Smoothly adjust the camera's position
        transform.position = Vector3.SmoothDamp(transform.position, cameraPosition + battlePosition, ref velocity, smoothTime);

        // Make the camera look at the center of the bounds
        transform.LookAt(combinedBounds.center);
        // Adjust the x rotation to be around 10 degrees while keeping the other rotations intact
        transform.rotation = Quaternion.Euler(10f, transform.rotation.eulerAngles.y, transform.rotation.eulerAngles.z);
    }


    public void SwitchToTraversalMode()
    {
        offset = traversalPosition;
        Camera.main.fieldOfView = 80f;
        transform.rotation = Quaternion.Euler(45,0,0);
        currentMode = CameraMode.Traversal;
        // other necessary changes for traversal mode
    }

    public void Zoom(float amount,Vector3 targetPosition)
    {
        // Calculate the new position along the vector between the camera and the player
        float zoomFactor = 1.0f + amount; // Adjust this value as needed
        Vector3 direction = (transform.position - targetPosition).normalized;
        offset -= direction * zoomFactor;
    }

    public void Rotate(float angle)
    {
        // Rotate the camera around the player's up axis (Y-axis)
        transform.RotateAround(followTarget.position, Vector3.up, angle);

        // After rotation, compute the new offset based on the camera's new position
        offset = transform.position - followTarget.position;
    }

    public void ResetRotation()
    {
        transform.rotation = Quaternion.Euler(0, rotationAngle, 0);
        // Update the offset accordingly after resetting rotation
        if (currentMode == CameraMode.Traversal)
            offset = traversalPosition;
        else
            offset = battlePosition;
    }

}