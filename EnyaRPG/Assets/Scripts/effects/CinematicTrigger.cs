using UnityEngine;

[System.Serializable]
public class CinematicSettings
{
    public Vector3 Position;
    public Quaternion Rotation;
    public float FieldOfView;
}
public class CinematicTrigger : MonoBehaviour
{
    private Camera childCamera;
    private CinematicSettings cinematicSettings;

    void Start()
    { 
        // Find the child camera component
        childCamera = GetComponentInChildren<Camera>();
        if (childCamera == null)
        {
            Debug.LogError("No child camera found for cinematic settings.");
            return;
        }

        // Initialize cinematic settings from the child camera
        cinematicSettings = new CinematicSettings
        {
            Position = childCamera.transform.position,
            Rotation = childCamera.transform.rotation,
            FieldOfView = childCamera.fieldOfView
        };

    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player")) // Assuming the player has a tag "Player"
        {
            CameraController cameraController = FindObjectOfType<CameraController>();
            if (cameraController != null)
            {
                cameraController.TriggerCinematicMode(cinematicSettings);
            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player")) // Assuming the player has a tag "Player"
        {
            CameraController cameraController = FindObjectOfType<CameraController>();
            if (cameraController != null)
            {
                cameraController.SwitchToTraversalMode();
            }
        }
    }
}
